using GameServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AI
{
    class AIMonsterPassive: AIBase
    {
        public const string ID = "AIMonsterPassive";
  
        public AIMonsterPassive(Creature mons):base(mons)
        {
           
        }
    }
}
