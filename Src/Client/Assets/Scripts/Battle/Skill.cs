using Common.Battle;
using Common.Data;
using Entities;
using Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SkillResult = Common.Battle.SkillResult;

namespace Battle
{
    public class Skill
    {
        public NSkillInfo Info;
        public Creature Owner;
        public SkillDefine Define;
        private float cd;
        public Skill(NSkillInfo info,Creature owner)
        {
            this.Info = info;
            this.Owner = owner;
            this.Define = DataManager.Instance.Skills[(int)this.Owner.Define.Class][this.Info.Id];
            this.cd = 0;
        }
        public SkillResult canCast( Creature target)
        {
            if(this.Define.CastTarget == TargetType.Target )
            {
                if(target == null|| target == this.Owner)
                      return SkillResult.InvalidTarget;
            }
            if (this.Define.CastTarget == TargetType.Target && BattleManager.Instance.CurrentPostion ==null)
            {
                return SkillResult.InvalidTarget;
            }
            if(this.Owner.Attributes.MP < this.Define.MPCost)
            {
                return SkillResult.OutOfMP;
            }
            if(this.cd >0)
            {
                return SkillResult.Cooldown;
            }
            return SkillResult.OK;
        }
        public void OnUpdate(float delta)
        {
            if(this.isCasting)
            {

            }
            UpdateCD(delta);
        }
        private void UpdateCD(float delta)
        {
            if(this.cd > 0)
            {
                this.cd -= delta;
            }
            if (cd < 0)
                this.cd = 0;
        }
    }
}
