using Common.Battle;
using GameServer.Battle;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AI
{
    class AIBase
    {
        private Monster owner;
        private Creature target;
        Skill normalSkill;
        public AIBase (Monster monster)
        {
            this.owner = monster;
            normalSkill = this.owner.SkillMgr.NormalSkill;
        }
        internal void Update()
        {
            if(this.owner.BattleState == Common.Battle.CharState.InBattle)
            {
                this.UpdateBattle();
            }
        }
        private void UpdateBattle()
        {
            if (this.target == null)
            {
                this.owner.BattleState = Common.Battle.CharState.None;
            }
            if (!TryCastSkill())
            {
                if (!TryCastNormal())
                {
                    FollowTarget();
                }
            }
        }
     
        private bool TryCastSkill()
        {
            if(this.target!= null)
            {
                BattleContext context = new BattleContext(this.owner.map.Battle)
                {
                    Target = this.target,
                    Caster = this.owner
                };
                Skill skill = this.owner.FindSkill(context, SkillType.Skill | SkillType.Passive);
                if(skill != null)
                {
                    this.owner.CastSkill(context, normalSkill.Define.ID);
                    return true;
                }
              

            }
            return false;
        
        }
        private bool TryCastNormal()
        {
            if (this.target != null)
            {
                BattleContext context = new BattleContext(this.owner.map.Battle)
                {
                    Target = this.target,
                    Caster = this.owner
                };
                var result = normalSkill.CanCast(context);
                if (result == SkillResult.Ok)
                {
                    this.owner.CastSkill(context, normalSkill.Define.ID);

                }
                if (result == SkillResult.OutOfRange)
                {
                    return false;
                }

            }
            return true;
        }
        private void FollowTarget()
        {
            int distance = this.owner.Distance(this.target);
            if(distance > normalSkill.Define.CastRange -50)
            {
                this.owner.MoveTo(this.target.Position);
            }
            else
            {
                this.owner.SopMove();
            }
        }

        internal void OnDamage(NDamageInfo damage, Creature source)
        {
            this.target = source;
        }
    }
}
