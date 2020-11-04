using Common;
using GameServer.Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class ArenaManager : Singleton<ArenaManager>
    {
        public const int ArenaMapID = 5;
        public const int MaxInstance = 100;
        Queue<int> InstanceIndexes = new Queue<int>();
        Arena[] Areans = new Arena[MaxInstance];
        public void Init()
        {
            for (int i = 0; i < MaxInstance; i++)
            {
                InstanceIndexes.Enqueue(i);
            }
        }
        public Arena NewArena(ArenaInfo info, NetConnection<NetSession> red, NetConnection<NetSession> blue = null)
        {
            var instance = InstanceIndexes.Dequeue();
            var map = MapManager.Instance.GetInstance(ArenaMapID, instance);
            Arena arena = new Arena(map, info, red, blue);
            this.Areans[instance] = arena;
            arena.PlayerEnter();
            return arena;
        }

        public Arena GetAren(int arenaId)
        {
            return this.Areans[arenaId];
        }
        internal void Update()
        {
            for(int i =0; i<Areans.Length;i++)
            {
                if(this.Areans[i]!= null)
                {
                    this.Areans[i].Update();
                }
            }
        }
    }
}

