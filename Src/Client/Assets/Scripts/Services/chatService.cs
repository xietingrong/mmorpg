using Managers;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Services
{
    class ChatService : Singleton<ChatService>, IDisposable
    {
        public ChatService()
        {
            MessageDistributer.Instance.Subscribe<ChatResponse>(this.OnChat);
        }
        public void SendChat(ChatChannel channel, string content, int toId, string toName)
        {
            Debug.Log("SendChat");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.Chat = new ChatRequest();
            message.Request.Chat.Message = new ChatMessage();
            message.Request.Chat.Message.Channel = channel;
            message.Request.Chat.Message.ToId = toId;
            message.Request.Chat.Message.ToName = toName;
            message.Request.Chat.Message.Message = content;
            NetClient.Instance.SendMessage(message);
        }
        private void OnChat(object sender, ChatResponse message)
        {
            if (message.Result == Result.Success)
            {
                ChatManager.Instance.AddMessage(ChatChannel.Local,message.localMessages);
                ChatManager.Instance.AddMessage(ChatChannel.World, message.worldMessages);
                ChatManager.Instance.AddMessage(ChatChannel.System, message.systemMssages);
                ChatManager.Instance.AddMessage(ChatChannel.Private, message.privateMessages);
                ChatManager.Instance.AddMessage(ChatChannel.Team, message.teamMessages);
                ChatManager.Instance.AddMessage(ChatChannel.Guild, message.guildMessages);
            }
            else
            {
                ChatManager.Instance.AddSystemMessage(message.Errormsg);
            }
        }

        public void Init()
        {

        }
        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<ChatResponse>(this.OnChat);
        }
     
    }
}
