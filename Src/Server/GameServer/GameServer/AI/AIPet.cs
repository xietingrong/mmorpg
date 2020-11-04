using GameServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AI
{
    class AIPet : AIBase
    {
        public const string ID = "AIPet";
 

        public AIPet(Creature mons) : base(mons)
        {

        }
    }
}
