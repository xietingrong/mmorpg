using Common;
using Common.Data;
using GameServer.Managers;
using GameServer.Services;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    class Arena
    {
        const float READY_TIME = 11F;
        const float ROUND_TIME = 60F;
        const float RESULT_TIME = 5F;

        public Map map;
        public ArenaInfo ArenaInfo;
        public NetConnection<NetSession> Red;
        public NetConnection<NetSession> Blue;

        Map SourceMapRed;
        Map SourceMapBlue;

        int RedPoint = 9;
        int BluePoint = 10;

        private bool redReady;
        private bool blueReady;

        private ArenaStatus ArenaStatus;
        private float timer = 0f;
        private ArenaRoundStatus RoundStatus;

        public int Round;
        private bool Ready {
            get
            {
                return this.redReady && this.blueReady;
            }
        }

        public Arena(Map map, ArenaInfo arena, NetConnection<NetSession> red, NetConnection<NetSession> blue)
        {
            this.map = map;
            this.ArenaInfo = arena;
            this.Red = red;
            this.Blue = blue;
            arena.ArenaId = map.InstanceId;
            this.ArenaStatus = ArenaStatus.Wait;
            this.RoundStatus = ArenaRoundStatus.None;
        }


        internal void PlayerEnter()
        {
            this.SourceMapRed = PlayerLeaveMap(this.Red);
            if(this.Blue != null)
                this.SourceMapBlue = PlayerLeaveMap(this.Blue);
            PlayerEnterArena();
        }
        private Map PlayerLeaveMap(NetConnection<NetSession> player)
        {
            var currentMap = MapManager.Instance[player.Session.Character.Info.mapId];
            currentMap.CharacterLeave(player.Session.Character);
            EntityManager.Instance.RemoveEntity(currentMap.ID, currentMap.InstanceId, player.Session.Character);
            return currentMap;
        }
        private void PlayerEnterArena()
        {
            TeleporterDefine redPoint = DataManager.Instance.Teleporters[this.RedPoint];
            this.Red.Session.Character.Position = redPoint.Position;
            this.Red.Session.Character.Direction = redPoint.Direction;
            if (this.Blue != null)
            {
                TeleporterDefine bluePoint = DataManager.Instance.Teleporters[this.BluePoint];
                this.Blue.Session.Character.Position = redPoint.Position;
                this.Blue.Session.Character.Direction = redPoint.Direction;
            }

            this.map.AddCharacter(this.Red, this.Red.Session.Character);
            if (this.Blue != null)
                this.map.AddCharacter(this.Blue, this.Blue.Session.Character);

            this.map.CharacterEnter(this.Red, this.Red.Session.Character);
            if (this.Blue != null)
                this.map.CharacterEnter(this.Blue, this.Blue.Session.Character);

            EntityManager.Instance.AddEntity(this.map.ID, this.map.InstanceId, this.Red.Session.Character);
            if(this.Blue!= null)
                 EntityManager.Instance.AddEntity(this.map.ID, this.map.InstanceId, this.Blue.Session.Character);
        }
        internal void Update()
        {
            if(this.ArenaStatus ==ArenaStatus.Game)
            {
                UpdateRound();
            }
        }

        private void UpdateRound()
        {
           if(this.RoundStatus == ArenaRoundStatus.Ready)
            {
                this.timer -= Time.deltaTime;
                if(timer<0)
                {
                    this.RoundStatus = ArenaRoundStatus.Fight;
                    this.timer = ROUND_TIME;
                    Log.InfoFormat("Arean:[{0}] Round Start", this.ArenaInfo.ArenaId);
                    ArenaService.Instance.SendArenaRoundStart(this);
                }
            }
            else if(this.RoundStatus == ArenaRoundStatus.Fight)
            {
                this.timer -= Time.deltaTime;
                if (timer < 0)
                {
                    this.RoundStatus = ArenaRoundStatus.Result;
                    this.timer =RESULT_TIME;
                    Log.InfoFormat("Arean:[{0}] Round End", this.ArenaInfo.ArenaId);
                    ArenaService.Instance.SendArenaRoundEnd(this);
                }
            }
            else if (this.RoundStatus == ArenaRoundStatus.Result)
            {
                this.timer -= Time.deltaTime;
                if (timer < 0)
                {
                    if(this.Round >=3)
                    {
                        ArenaRsult();
                    }
                    else
                    {
                        NextRound();
                    }
                }
            }
        }

        private void ArenaRsult()
        {
            this.ArenaStatus = ArenaStatus.Result;
            //执行结算
        }

        internal void EntityReady(int entityId)
        {
            if(this.Red.Session.Character.entityId == entityId)
            {
                this.redReady = true;
            }
            if (this.Blue.Session.Character.entityId == entityId)
            {
                this.blueReady = true;
            }
            if(this.Ready)
            {
                this.ArenaStatus = ArenaStatus.Game;
                this.Round = 0;
                NextRound();
            }
        }

        private void NextRound()
        {
            this.Round++;
            this.timer = READY_TIME;
            this.RoundStatus = ArenaRoundStatus.Ready;
            Log.InfoFormat("Arena;[{0} round[{1}]]", this.ArenaInfo.ArenaId, this.Round);
            ArenaService.Instance.SendArenaReady(this);
        }
    }
}
