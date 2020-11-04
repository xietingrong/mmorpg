using GameServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AI
{
    class AIBoss:AIBase
    {
        public const string ID = "AIBoss";

        public AIBoss(Creature mons) : base(mons)
        {

        }
    }
}
