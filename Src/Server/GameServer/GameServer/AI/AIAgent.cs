using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AI
{
    class AIAgent
    {
        private Creature mons;
        private AIBase ai;
        public AIAgent(Creature mons)
        {
            this.mons = mons;
            string ainame = mons.Define.AI;
            if (string.IsNullOrEmpty(ainame))
                ainame = AIMonsterPassive.ID;
            switch (ainame)
            {
                case AIMonsterPassive.ID:
                     this.ai = new AIMonsterPassive(mons);
                    break;
                case AIBoss.ID:
                    this.ai = new AIBoss(mons);
                    break;
                case AIPet.ID:
                    this.ai = new AIPet(mons);
                    break;
            }
        }
        internal void Update()
        {
            if (this.ai != null )
            {
                this.ai.Update();
            }

        }


        internal void OnDamage(NDamageInfo damage, Creature source)
        {
            if (this.ai != null) {
                this.ai.OnDamage(damage, source);
            }
        }
        internal void OnOwner(Character father, int dest)
        {
            if (this.ai != null)
            {
                this.ai.OnOwner(father,dest);
            }
        }
    }
}


