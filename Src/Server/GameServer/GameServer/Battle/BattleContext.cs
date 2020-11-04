using GameServer.Core;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    class BattleContext
    {
        public Battle Battle;
        public Creature Caster;
        public Creature Target;
        public NSkillCastInfo CastSkill;
        public SkillResult Result;
        public NVector3 Position
        {
            get
            {
                if (this.CastSkill == null)
                    return new Vector3Int();
                return this.CastSkill.Postion;
            }
        }
        public BattleContext(Battle battle)
        {
            this.Battle = battle;
        }

        
    }
}
