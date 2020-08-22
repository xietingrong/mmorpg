using Common;
using GameServer.Entities;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    class QuestService : Singleton<QuestService>
    {
        public QuestService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<QuestSubmitRequest>(this.OnSubmitRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<QuestAcceptRequest>(this.OnAcceptRequest);
        }

        private void OnAcceptRequest(NetConnection<NetSession> sender, QuestAcceptRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("QuestAcceptRequest:: character:{0}:QuestId{1}", character.Id, request.QuestId);
            Result result = character.QuestManager.AcceptQuest(sender, request.QuestId);
            sender.Session.Response.questAccept = new QuestAcceptResponse();
            sender.Session.Response.questAccept.Result = result;
            sender.SendResponse();
        }

        private void OnSubmitRequest(NetConnection<NetSession> sender, QuestSubmitRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("QuestSubmitRequest:: character:{0}:QuestId{1}", character.Id, request.QuestId);
            Result result = character.QuestManager.SubmitQuest(sender, request.QuestId);
            sender.Session.Response.questSubmit = new QuestSubmitResponse();
            sender.Session.Response.questSubmit.Result = result;
            sender.SendResponse();
        }

        public void Init()
        {

        }
    }
}
