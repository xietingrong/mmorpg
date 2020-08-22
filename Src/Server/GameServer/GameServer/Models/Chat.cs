using GameServer.Entities;
using GameServer.Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    class Chat
    {
        Character Owner;
        public int localIdx;
        public int worldIdx;
        public int systemIdx;
        public int teamIdx;
        public int guildIdx;
        public Chat(Character owner)
        {
            this.Owner = owner;
        }
        public void PostProcess(NetMessageResponse nessage)
        {
            if(nessage.Chat == null)
            {
                nessage.Chat = new ChatResponse();
                nessage.Chat.Result = Result.Success;
            }
            this.localIdx = ChatManager.Instance.GetLocalMessages(this.Owner.Info.mapId, this.localIdx, nessage.Chat.localMessages);
            this.worldIdx = ChatManager.Instance.GetWorldMessages(this.worldIdx, nessage.Chat.worldMessages);
            this.systemIdx = ChatManager.Instance.GetSystemMessages(this.systemIdx, nessage.Chat.systemMssages);
            if (this.Owner.Team != null)
                this.teamIdx = ChatManager.Instance.GetTeamMessages(this.Owner.Team.Id, this.teamIdx, nessage.Chat.teamMessages);
            if (this.Owner.Guild != null)
                this.guildIdx = ChatManager.Instance.GetGuildMessages(this.Owner.Guild.Id, this.guildIdx, nessage.Chat.guildMessages);
        }
    }
}
