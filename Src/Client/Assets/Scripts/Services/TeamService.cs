using Managers;
using Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Services
{
    class TeamService : Singleton<TeamService>, IDisposable
    {
        public void Init()
        {

        }

        public TeamService()
        {
            MessageDistributer.Instance.Subscribe<TeamInviteRequest>(this.OnTeamInvitRequest);
            MessageDistributer.Instance.Subscribe<TeamInviteResponse>(this.OnTeamInviteResponse);
            MessageDistributer.Instance.Subscribe<TeamInfoResponse>(this.OnTeamInfo);
            MessageDistributer.Instance.Subscribe<TeamLeaveResponse>(this.OnTeamLeave);
        }
    

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<TeamInviteRequest>(this.OnTeamInvitRequest);
            MessageDistributer.Instance.Unsubscribe<TeamInviteResponse>(this.OnTeamInviteResponse);
            MessageDistributer.Instance.Unsubscribe<TeamInfoResponse>(this.OnTeamInfo);
            MessageDistributer.Instance.Unsubscribe<TeamLeaveResponse>(this.OnTeamLeave);
        }
        public void SendTeamInviteRequest(int friendId, String friendName)
        {
            Debug.Log("SendTeamInviteRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.teamInviteReq = new TeamInviteRequest();
            message.Request.teamInviteReq.FromId = User.Instance.CurrentCharacterInfo.Id;
            message.Request.teamInviteReq.FromName = User.Instance.CurrentCharacterInfo.Name;
            message.Request.teamInviteReq.ToId = friendId;
            message.Request.teamInviteReq.ToName = friendName;
            NetClient.Instance.SendMessage(message);
 
        }

        public void SendTeamInviteRequest(bool accept, TeamInviteRequest request)
        {
            Debug.Log("SendTeamInviteReques");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.teamInviteRes = new TeamInviteResponse();
            message.Request.teamInviteRes.Result = accept?Result.Success:Result.Failed;
            message.Request.teamInviteRes.Errormsg = accept ? "组队成功" : "对方拒绝了组队请求";
            message.Request.teamInviteRes.Request= request;
           
            NetClient.Instance.SendMessage(message);
        }

        private void OnTeamInvitRequest(object sender, TeamInviteRequest request)
        {
            var confirn = MessageBox.Show(String.Format("{0}请求加你为好友", request.FromName), "好友请求", MessageBoxType.Confirm, "接受", "拒绝");
            confirn.OnYes = () =>
            {
                this.SendTeamInvitResponse(true, request);
            };
            confirn.OnNo = () =>
            {
                this.SendTeamInvitResponse(false, request);
            };
        }
        private void OnTeamInviteResponse(object sender, TeamInviteResponse message)
        {
            if (message.Result == Result.Success)
            {
                MessageBox.Show(message.Request.ToName+"加入你的队伍", "邀请组队成功");
            }
            else
            {
                MessageBox.Show(message.Errormsg, "邀请组队失败");
            }
        }
       

        public void SendTeamInvitResponse(bool accept, TeamInviteRequest request)
        {
            Debug.Log("SendTeamInvitResponse");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.teamInviteRes = new TeamInviteResponse();
            message.Request.teamInviteRes.Result = accept ? Result.Success : Result.Failed;
            message.Request.teamInviteRes.Errormsg = accept ? "对方同意" : "对方拒绝了组队请求";
            message.Request.teamInviteRes.Request = request;
            NetClient.Instance.SendMessage(message);
        }
        public void OnTeamInfo(object sender, TeamInfoResponse request)
        {
            Debug.Log("OnTeamInfo");
            TeamManager.Instance.UpdateTeamInfo(request.Team);
        }
        public void SendTeamLeaveRequest(int id)
        {
            Debug.Log("SendTeamLeaveRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.teamLeave = new TeamLeaveRequest();
            message.Request.teamLeave.TeamId = User.Instance.TeamInfo.Id;
            message.Request.teamLeave.characterId= User.Instance.CurrentCharacterInfo.Id;
        
            NetClient.Instance.SendMessage(message);
        }
        public void OnTeamLeave(object sender, TeamLeaveResponse message)
        {
            if (message.Result == Result.Success)
            {
                TeamManager.Instance.UpdateTeamInfo(null);
                MessageBox.Show("退出成功", "退出队伍");
            } 
            else
            {
                MessageBox.Show(message.Errormsg, "添加好友失败");
            }
                
        }

    }
}
