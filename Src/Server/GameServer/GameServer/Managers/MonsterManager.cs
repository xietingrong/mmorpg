using GameServer.Entities;
using GameServer.Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class MonsterManager
    {
        private Map Map;
        private int idx;
        public Dictionary<int, Monster> Monsters = new Dictionary<int, Monster>();
        public void Init(Map map)
        {
            this.Map = map;
        }
        internal Monster Create(int spawnMonID,int spawnLevel,NVector3 psstion,NVector3 dirtion)
        {
            Monster monster = new Monster(spawnMonID, spawnLevel, psstion, dirtion);
            EntityManager.Instance.AddEntity(this.Map.ID, this.Map.InstanceId, monster);
            monster.Info.Id = monster.entityId;
            monster.Info.mapId = this.Map.ID;
            Monsters[idx]=monster;
            idx++;
            this.Map.MonsterEnter(monster);
            return monster;
        }
    }
}
