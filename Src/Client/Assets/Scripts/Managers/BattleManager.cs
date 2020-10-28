using Battle;
using Entities;
using Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Managers
{
    class BattleManager:Singleton<BattleManager>
    {
        public delegate void TargetChangedHandler(Creature target);
        public event TargetChangedHandler OnTargetChanged;

        private Creature currentTarget;
        public Creature CurrentTarget
        {
            get { return this.currentTarget; }

            set { this.SetTarget(value); }
        }

        private void SetTarget(Creature target)
        {
            if (this.currentTarget != target && this.OnTargetChanged != null)
                this.OnTargetChanged(target);
            this.currentTarget = target;
            Debug.LogFormat("BattleManager.SetTarget:[{0}:{1}]", target.entityId, target.Name);
        }
        public void InIt()
        {

        }
        private NVector3 currentPostion;
        public NVector3 CurrentPostion
        {
            get { return this.currentPostion; }

           set { this.SetPostion(value); }
        }

        private void SetPostion(NVector3 Postion)
        {
            this.currentPostion = Postion;
            Debug.LogFormat("BattleManager.SetPostion:[{0}]", Postion);
        }
        public void CastSkill(Skill skill)
        {
            int target = CurrentTarget != null ? currentTarget.entityId : 0;
            BattleService.Instance.SendSkillCast(skill.Define.ID, skill.Owner.entityId, target, currentPostion);
        }
    }
}
