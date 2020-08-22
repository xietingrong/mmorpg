using Common;
using GameServer.Entities;
using GameServer.Managers;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    class GuildService : Singleton<GuildService>
    {
        public GuildService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildCreateRequest>(this.OnGuildCreate);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildListRequest>(this.OnGuildList);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinRequest>(this.OnGuildJoinRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinResponse>(this.OnGuildJoinResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildLeaveRequest>(this.OnGuildLeave);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildAdminRequest>(this.OnGuildAdmin);
        }


        public void Init()
        {
            GuildManager.Instance.Init();
        }
        public void OnGuildCreate(NetConnection<NetSession> sender, GuildCreateRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildCreat:: GuildName:{0} character:[{1}]{2}", request.GuildName, character.Id, character.Name);

            sender.Session.Response.guildCreate = new GuildCreateResponse();
            if (character.Guild == null)
            {
                sender.Session.Response.guildCreate.Result = Result.Failed;
                sender.Session.Response.guildCreate.Errormsg = "已经有工会了";
                sender.SendResponse();
                return;
            }
            if (GuildManager.Instance.CheckNameExisted(request.GuildName))
            {
                sender.Session.Response.guildCreate.Result = Result.Failed;
                sender.Session.Response.guildCreate.Errormsg = "工会名称已存在";
                sender.SendResponse();
                return;
            }
            GuildManager.Instance.CreateGuild(request.GuildName, request.GuildNotice, character);
            sender.Session.Response.guildCreate.guildInfo = character.Guild.GuildInfo(character);
            sender.Session.Response.guildCreate.Result = Result.Success;
            sender.SendResponse();
        }
        private void OnGuildList(NetConnection<NetSession> sender, GuildListRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildList::  character:[{0}]{1}", character.Id, character.Name);
            sender.Session.Response.guildList = new GuildListResponse();
            sender.Session.Response.guildList.Result = Result.Success;
            sender.SendResponse();
        }
        private void OnGuildJoinRequest(NetConnection<NetSession> sender, GuildJoinRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinRequest:: GuildID:{0} character:[{1}]{2}", request.Apply.GuildId, request.Apply.characterId, request.Apply.Name);
            var guild = GuildManager.Instance.GetGuild(request.Apply.GuildId);
            if(guild ==null)
            {
                sender.Session.Response.guildJoinRes = new GuildJoinResponse();
                sender.Session.Response.guildJoinRes.Result = Result.Success;
                sender.Session.Response.guildJoinRes.Errormsg = "工会不存在";
                sender.SendResponse();
                return;

            }
            request.Apply.characterId = character.Data.ID;
            request.Apply.Name = character.Data.Name;
            request.Apply.Class = character.Data.Class;
            request.Apply.Level = character.Data.Level;
            if(guild.JoinApply(request.Apply))
             {
                var leader = SessionManager.Instance.GetSession(guild.Data.LeaderID);
                if(leader!= null)
                {//给队长发送申请加入请求
                    leader.Session.Response.guildJoinReq = request;
                    leader.SendResponse();
                }
            }
            else
            {
                sender.Session.Response.guildJoinRes = new GuildJoinResponse();
                sender.Session.Response.guildJoinRes.Result = Result.Failed;
                sender.Session.Response.guildJoinRes.Errormsg = "请勿重复请求";
                sender.SendResponse();
            }
        }
        private void OnGuildJoinResponse(NetConnection<NetSession> sender, GuildJoinResponse request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinResponse:: GuildID:{0} character:[{1}]{2}", request.Apply.GuildId, request.Apply.characterId, request.Apply.Name);
            var guild = GuildManager.Instance.GetGuild(request.Apply.GuildId);
            if (request.Result== Result.Success)
            {
                guild.JoinAppove(request.Apply);
            }
            var requester = SessionManager.Instance.GetSession(request.Apply.characterId);
            if(requester != null)
            {
                requester.Session.Character.Guild = guild;
                //互相加好友
                requester.Session.Response.guildJoinRes = request;
                requester.Session.Response.guildJoinRes.Result = Result.Success;
                requester.Session.Response.guildJoinRes.Errormsg = "加入工会成功";
                sender.SendResponse();
            }
        }
        private void OnGuildLeave(NetConnection<NetSession> sender, GuildLeaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildLeavelResponse:: character:{0}", character.Id);
            sender.Session.Response.guildLeave = new GuildLeaveResponse();
            character.Guild.Leave(character);
            sender.Session.Response.guildLeave.Result = Result.Success;
            DBService.Instance.Save();
            sender.SendResponse();
        }

        private void OnGuildAdmin(NetConnection<NetSession> sender, GuildAdminRequest message)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildAdmin:: character:{0}", character.Id);

            sender.Session.Response.guildAdmin = new GuildAdminResponse();
            if(character.Guild == null)
            {
                sender.Session.Response.guildAdmin.Result = Result.Failed;
                sender.Session.Response.guildAdmin.Errormsg = "你没有工会不要乱来";
                sender.SendResponse();
            }
            character.Guild.ExecuteAdmin(message.Command, message.Target,character.Id);
            var target = SessionManager.Instance.GetSession(message.Target);
            if(target!= null)
            {
                target.Session.Response.guildAdmin = new GuildAdminResponse();
                target.Session.Response.guildAdmin.Result = Result.Success;
                target.Session.Response.guildAdmin.Command = message;
                target.SendResponse();
            }
            sender.Session.Response.guildAdmin.Result = Result.Success;
            sender.Session.Response.guildAdmin.Command = message;
            sender.SendResponse();
        }
    } 
}
