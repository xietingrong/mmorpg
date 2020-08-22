using Common;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class ChatManager:Singleton<ChatManager>
    {
        public List<ChatMessage> System = new List<ChatMessage>();
        public List<ChatMessage> World = new List<ChatMessage>();
        public Dictionary<int, List<ChatMessage>> local = new Dictionary<int, List<ChatMessage>>();
        public Dictionary<int, List<ChatMessage>> Team = new Dictionary<int, List<ChatMessage>>();
        public Dictionary<int, List<ChatMessage>> Guild = new Dictionary<int, List<ChatMessage>>();
        public void Init()
        {

        }
        public void AddMessage(Character from, ChatMessage message)
        {
            message.FromId = from.Id;
            message.FromName = from.Name;
            message.Time = Time.timestamp;
            switch(message.Channel)
            {
                case ChatChannel.Local:
                    this.AddLocalMessage(from.Info.mapId, message);
                    break;
                case ChatChannel.World:
                    this.AddWorldMessage( message);
                    break;
                case ChatChannel.System:
                    this.AddSystemMessage( message);
                    break;
                case ChatChannel.Team:
                    this.AddTeamMessage(from.Team.Id, message);
                    break;
                case ChatChannel.Guild:
                    this.AddGuildMessage(from.Guild.Id, message);
                    break;
            }
        }

        private void AddGuildMessage(int guildId, ChatMessage message)
        {
            List<ChatMessage> messages = null;
            if (!this.Guild.TryGetValue(guildId, out messages))
            {
                messages = new List<ChatMessage>();
                this.Guild[guildId] = messages;
            }
            messages.Add(message);
        }

        private void AddTeamMessage(int Teamid, ChatMessage message)
        {
            List<ChatMessage> messages = null;
            if (!this.Team.TryGetValue(Teamid, out messages))
            {
                messages = new List<ChatMessage>();
                this.Team[Teamid] = messages;
            }
            messages.Add(message);
        }

        private void AddSystemMessage(ChatMessage message)
        {
            this.System.Add(message);
        }

        private void AddWorldMessage(ChatMessage message)
        {
            this.World.Add(message);
        }

        private void AddLocalMessage(int mapId, ChatMessage message)
        {
            List<ChatMessage> messages = null;
            if (!this.local.TryGetValue(mapId, out messages))
            {
                messages = new List<ChatMessage>();
                this.local[mapId] = messages;
            }
            messages.Add(message);
        }
        public int GetLocalMessages(int mapId,int idx, List<ChatMessage> result)
        {
            List<ChatMessage> messages = null;
            if (!this.local.TryGetValue(mapId, out messages))
            {
                return 0;
            }
            return GetNewMessage(idx, result, messages);
        }

   
        public int GetWorldMessages( int idx, List<ChatMessage> result)
        {
            return GetNewMessage(idx, result, this.World);
        }
        public int GetSystemMessages(int idx, List<ChatMessage> result)
        {
            return GetNewMessage(idx, result, this.System);
        }
        public int GetTeamMessages(int teamId, int idx, List<ChatMessage> result)
        {
            List<ChatMessage> messages = null;
            if (!this.Team.TryGetValue(teamId, out messages))
            {
                return 0;
            }
            return GetNewMessage(idx, result, messages);
        }
        public int GetGuildMessages(int guildId,int idx, List<ChatMessage> result)
        {
            List<ChatMessage> messages = null;
            if (!this.Guild.TryGetValue(guildId, out messages))
            {
                return 0;
            }
            return GetNewMessage(idx, result, messages);
        }
        private int GetNewMessage(int idx, List<ChatMessage> result, List<ChatMessage> messages)
        {
            if(idx== 0)
            {
                if(messages.Count> GameDefine.MaxChatRecoredNum)
                {
                    idx = messages.Count - GameDefine.MaxChatRecoredNum;
                }
            }
            for(;idx< messages.Count; idx++)
            {
                result.Add(messages[idx]);
            }
            return idx;
        }

    }

}
