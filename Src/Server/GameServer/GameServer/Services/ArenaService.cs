using Common;
using GameServer.Entities;
using GameServer.Managers;
using GameServer.Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    class ArenaService : Singleton<ArenaService>
    {
        public ArenaService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ArenaChallengeRequest>(this.OnArenaChallengeRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ArenaChallengeResponse>(this.OnArenaChallengeResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ArenaReadyRequest>(this.OnArenaReady);
        }

     

        public void Dispose()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Unsubscribe<ArenaChallengeRequest>(this.OnArenaChallengeRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Unsubscribe<ArenaChallengeResponse>(this.OnArenaChallengeResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Unsubscribe<ArenaReadyRequest>(this.OnArenaReady);
        }
        public void Init()
        {
            ArenaManager.Instance.Init();
        }
        
        private void OnArenaChallengeRequest(NetConnection<NetSession> sender, ArenaChallengeRequest request)
        {
            Character character = sender.Session.Character;
           // Log.InfoFormat("OnArenaChallengeRequest: RedId:{0}:RedName{1} BlueId:{2} BlueName:{3]", request.ArenaInfo.Red.EntityId, request.ArenaInfo.Red.Name, request.ArenaInfo.Blue.EntityId, request.ArenaInfo.Blue.Name);
            NetConnection<NetSession> blue = null;
            if (request.ArenaInfo.Blue.EntityId > 0)
            {
                blue = SessionManager.Instance.GetSession(request.ArenaInfo.Blue.EntityId);
            }
            else
            {//pve对战 
                var arena = ArenaManager.Instance.NewArena(request.ArenaInfo, sender, null);
                this.SendArenaBegin(arena);
                return;
            }
            if (blue == null)
            {

                sender.Session.Response.arennaChallengeRes = new ArenaChallengeResponse();
                sender.Session.Response.arennaChallengeRes.Result = Result.Failed;
                sender.Session.Response.arennaChallengeRes.Errormsg = "对方不存在或不在线";
                sender.SendResponse();
                return;
            }
            //Log.InfoFormat("ForwardArenaChallengeRequest: RedId:{0}:RedName{1} BlueId:{2} BlueName:{3]", request.ArenaInfo.Red.EntityId, request.ArenaInfo.Red.Name, request.ArenaInfo.Blue.EntityId, request.ArenaInfo.Blue.Name);
            blue.Session.Response.arennaChallengeReq = request;
            blue.SendResponse();
        }

        private void OnArenaChallengeResponse(NetConnection<NetSession> sender, ArenaChallengeResponse reponse)
        {
            Character character = sender.Session.Character;
           // Log.InfoFormat("OnArenaChallengeReponse: RedId:{0}:RedName{1} BlueId:{2} BlueName:{3]", reponse.ArenaInfo.Red.EntityId, reponse.ArenaInfo.Red.Name, reponse.ArenaInfo.Blue.EntityId, reponse.ArenaInfo.Blue.Name);
            var requester = SessionManager.Instance.GetSession(reponse.ArenaInfo.Red.EntityId);

            if (requester == null)
            {
                sender.Session.Response.arennaChallengeRes.Result = Result.Failed;
                sender.Session.Response.arennaChallengeRes.Errormsg = "挑战者已下线";
                sender.SendResponse();
                return;
            }
            if (reponse.Result == Result.Failed)
            {
                requester.Session.Response.arennaChallengeRes = reponse;
                requester.Session.Response.arennaChallengeRes.Result = Result.Failed;
                requester.SendResponse();
                return;
            }
            var arena = ArenaManager.Instance.NewArena(reponse.ArenaInfo, requester, sender);
            this.SendArenaBegin(arena);
        }

      

        public void SendArenaBegin(Arena arena)
        {
            var arenaBegin = new ArenaBeginResponse();
            arenaBegin.Result = Result.Failed;
            arenaBegin.Errormsg = "对方不在线";
            arenaBegin.ArenaInfo = arena.ArenaInfo;
            arena.Red.Session.Response.arenaBegin = arenaBegin;
            arena.Red.SendResponse();

            if(arena.Blue!= null)//pve对战 
            {
                arena.Blue.Session.Response.arenaBegin = arenaBegin;
                arena.Blue.SendResponse();
            }
           
        }

        private void OnArenaReady(NetConnection<NetSession> sender, ArenaReadyRequest message)
        {
            var arena = ArenaManager.Instance.GetAren(message.arenaId);
            arena.EntityReady(message.Entity);
        }
        internal void SendArenaReady(Arena arena)
        {
            var arenaReady = new ArenaReadyResponse();
            arenaReady.Round = arena.Round;
            arenaReady.ArenaInfo = arena.ArenaInfo;
            arena.Red.Session.Response.arenaReady = arenaReady;
            arena.Red.SendResponse();
            arena.Blue.Session.Response.arenaReady = arenaReady;
            arena.Blue.SendResponse();
        }
        internal void SendArenaRoundStart(Arena arena)
        {
            var arenaStart = new ArenaRoundStarResponse();
            arenaStart.Round = arena.Round;
            arenaStart.ArenaInfo = arena.ArenaInfo;
            arena.Red.Session.Response.arenaRoundStar = arenaStart;
            arena.Red.SendResponse();
            arena.Blue.Session.Response.arenaRoundStar = arenaStart;
            arena.Blue.SendResponse();
        }
        internal void SendArenaRoundEnd(Arena arena)
        {
            var arenaEnd = new ArenaRoundEndResponse();
            arenaEnd.Round = arena.Round;
            arenaEnd.ArenaInfo = arena.ArenaInfo;
            arena.Red.Session.Response.arenaRoundEnd = arenaEnd;
            arena.Red.SendResponse();
            arena.Blue.Session.Response.arenaRoundEnd = arenaEnd;
            arena.Blue.SendResponse();
        }
    }
}
