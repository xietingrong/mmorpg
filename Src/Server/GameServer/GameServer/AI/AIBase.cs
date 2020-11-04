using Common;
using Common.Battle;
using GameServer.Battle;
using GameServer.Core;
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
        private Creature owner;
        private Creature target;
        private Character father;
        private int dest;
        Skill normalSkill;
        public AIBase (Creature mons)
        {
            this.owner = mons;
            normalSkill = this.owner.SkillMgr.NormalSkill;
        }
        internal void Update()
        {
            if (father != null && this.owner.BattleState != Common.Battle.CharState.InBattle)
            {
                FollowOwner();
            }
            //else if (this.owner.BattleState == Common.Battle.CharState.InBattle)
            //{
                this.UpdateBattle();
            //}

        }
        private void UpdateBattle()
        {
             TryCastTarget();
            //if (this.target == null)
            //{
            //    this.owner.BattleState = Common.Battle.CharState.None;
            //}
           
            if (!TryCastSkill())
            {
                if (!TryCastNormal())
                {
                    //FollowTarget();
                }
            }
        }
        //
        private void TryCastTarget()
        {
            List<Creature> list =this.owner.map.Battle.UpdateCanAttacKlist(this.owner);
            foreach ( var value in  list)
            {
                if(value != this.owner)
                {
                    if ((value.Spwnum - 1) % 6 == (this.owner.Spwnum - 1) % 6)
                    {
                        target = value;
                        return;
                    }
                }
            }
            if(list.Count >0)
               target = list[0];
        }
        private bool TryCastSkill()
        {
            if (this.target != null)
            {
                BattleContext context = new BattleContext(this.owner.map.Battle)
                {
                    Target = this.target,
                    Caster = this.owner
                };
               
                Skill skill = this.owner.FindSkill(context, SkillType.Skill | SkillType.Passive);
                if(skill != null)
                {
                    this.owner.CastSkill(context, skill.Define.ID);
                   
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
              
                if(normalSkill!= null)
                {
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
            }
            return true;
        }
        private void FollowTarget()
        {
            int distance = this.owner.Distance(this.target);
            if (distance >  200)
            {
                this.owner.MoveTo(this.target.Position);
            }
            else
            {
                this.owner.SopMove();
            }
        }
        private Vector3Int postionold = new Vector3Int();
        private void FollowOwner()
        {
            int value = 0;
            if (dest != 0)
                value = dest ;
            int distance = this.owner.Distance(this.father);
            if (distance > Math.Abs(value+100))
            {
                this.owner.MoveTo(this.father.Position);
            }
            else if(distance <= Math.Abs(value))
            {
                this.owner.SopMove();
            }
        }
        internal void OnDamage(NDamageInfo damage, Creature source)
        {
            //this.target = source;
        }
      
        internal void OnOwner(Character father,int dest)
        {
            this.father = father;
            this.dest = dest;
        }
    }
}
