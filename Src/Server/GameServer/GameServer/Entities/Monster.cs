using Common;
using Common.Battle;
using Common.Data;
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
        //AIAgent AI;
        //Creature Target;
        //public Map map;
        private Vector3Int moveTarget;
        private Vector3 movepPostion;
        public int MonsterId;
        private int dest;
      
        public Monster(int tid, int level, Vector3Int pos, Vector3Int dir,int spw) : base(CharacterType.Monster, tid, level, pos, dir)
        {
            Spwnum = spw;
            this.AI = new AIAgent(this);
            IsAiUse = true;
        }
        public override void OnEnterMap(Map map)
        {
            base.OnEnterMap(map);
        }

        public override void Update()
        {
           
            base.Update();
            this.UpdateMovement();
           
        }

        public override  Skill FindSkill(BattleContext context,SkillType type)
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
        internal  void OnOwner(Character father,int dest )
        {
            if (this.AI != null)
            {
                this.AI.OnOwner(father,dest);
                this.dest = dest;
              
            }
        }
        public override void MoveTo(Vector3Int position)
        {
            if(state == CharacterState.Idle)
            {
                state = CharacterState.Move;
            }
            if(this.moveTarget != position)
            {
                this.moveTarget = position;
                this.movepPostion = this.Position;
                this.Direction = (this.moveTarget - this.Position).normallizd;
                this.Speed = this.Define.Speed;
                NEntitySync sync = new NEntitySync();
                sync.Entity = this.EntityData;
                sync.Event = EntityEvent.MoveFwd;
                sync.Id = this.entityId;
                this.map.UpdateEntity(sync);
            }
        }
        public override void UpdateMovement()
        {
            int value = 150;
            if (dest != 0)
                value = dest;

            if (state == CharacterState.Move)
            {
                int distance = this.Distance(this.moveTarget);
                if (distance <= Math.Abs(value))
                {
                    this.SopMove();
                }

                if(this.Speed >0)
                {
                    this.Direction = (this.moveTarget - this.Position).normallizd;
                    Vector3 dir = this.Direction;

                    this.movepPostion += dir * this.Speed * Time.deltaTime / 100f;
            
                    this.Position = this.movepPostion;
                }
            }
        }

        public override void SopMove()
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
