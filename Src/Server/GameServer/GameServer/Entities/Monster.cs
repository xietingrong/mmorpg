using Common.Battle;
using GameServer.AI;
using GameServer.Battle;
using GameServer.Core;
using GameServer.Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditorInternal;

namespace GameServer.Entities
{
     class Monster : Creature
    {
        AIAgent AI;
        //Creature Target;
        //public Map map;
        private Vector3Int moveTarget;
        private Vector3 movepPostion;
        public Monster(int tid, int level, Vector3Int pos, Vector3Int dir) : base(CharacterType.Monster, tid, level, pos, dir)
        {
            this.AI = new AIAgent(this);
        }
        public override void OnEnterMap(Map map)
        {
            base.OnEnterMap(map);
        }

        public override void Update()
        {
           
            base.Update();
            this.UpdateMovement();
            this.AI.Update();
        }

        public Skill FindSkill(BattleContext context,SkillType type)
        {
            Skill cancast = null;
            foreach(var skill in this.SkillMgr.skills)
            {
                if((skill.Define.Type &type)!=skill.Define.Type)
                  continue;

                var result = skill.CanCast(context);
                if (result == SkillResult.Casting)
                    return null;
                if(result == SkillResult.Ok)
                {
                    cancast = skill;
                }
            }
            return cancast;
        }
        protected override void OnDamage(NDamageInfo damage,Creature source)
        {
            if(this.AI != null)
            {
                this.AI.OnDamage(damage,source);
            }
        }

        internal void MoveTo(Vector3Int position)
        {
            if(state == CharacterState.Idle)
            {
                state = CharacterState.Move;
            }
            if(this.moveTarget != position)
            {
                this.moveTarget = position;
                this.movepPostion = position;
                var dist = (this.moveTarget - this.Position);

                this.Direction = dist.normallizd;
                this.Speed = this.Define.Speed;
                NEntitySync sync = new NEntitySync();
                sync.Entity = this.EntityData;
                sync.Event = EntityEvent.MoveFwd;
                sync.Id = this.entityId;
                this.map.UpdateEntity(sync);
            }
        }
        private void UpdateMovement()
        {
            if(state == CharacterState.Move)
            {
                if(this.Distance(this.moveTarget) < 50)
                {
                    this.SopMove();
                }
                if(this.Speed >0)
                {
                    Vector3 dir = this.Direction;
                    this.movepPostion += dir *this.Speed * Time.deltaTime/100;
                    this.Position = this.movepPostion;
                }
            }
        }

        internal void SopMove()
        {
            this.state = CharacterState.Idle;
            this.moveTarget = Vector3Int.zero;
            this.Speed = 0;

            NEntitySync sync = new NEntitySync();
            sync.Entity = this.EntityData;
            sync.Event = EntityEvent.Idle;
            sync.Id = this.entityId;
            this.map.UpdateEntity(sync);
        }
    }
}
