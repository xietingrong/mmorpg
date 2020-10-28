using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Data;
using Entities;
using SkillBridge.Message;

namespace Battle
{
    public class Buff
    {
        public int BuffId;
        private Creature Owner;
        public BuffDefine Define;
        internal bool Stoped;
        public float time;
        private int hit;
    
        private int casterId;

        public Buff(Creature owner, int buffId, BuffDefine define, int casterId)
        {
            this.Owner = owner;
            this.BuffId = buffId;
            this.Define = define;
            this.casterId = casterId;
            this.OnAdd();
        }

        private void OnAdd()
        {
            if (this.Define.Effect != Common.Battle.BuffEffect.None)
            {
                this.Owner.AddBuffEffect(this.Define.Effect);
            }
            AddAttr();
           
        }

     

        internal void OnRemove()
        {
            RemoveAttr();
            Stoped = true;
            if (this.Define.Effect != Common.Battle.BuffEffect.None)
            {
                this.Owner.RemoveBuffEffect(this.Define.Effect);
            }
        }
        private void AddAttr()
        {
            if (this.Define.DEFRatio != 0)
            {
                this.Owner.Attributes.Buff.DEF += this.Owner.Attributes.Basic.DEF * this.Define.DEFRatio;
                this.Owner.Attributes.InitFinalAttributes();
            }
        }
        private void RemoveAttr()
        {
            if (this.Define.DEFRatio != 0)
            {
                this.Owner.Attributes.Buff.DEF -= this.Owner.Attributes.Basic.DEF * this.Define.DEFRatio;
                this.Owner.Attributes.InitFinalAttributes();
            }
        }

        internal void OnUpdate(float delta)
        {
            if (Stoped) return;
            this.time += delta;
            //if (this.Define.Interval > 0)
            //{
            //    if (this.time > this.Define.Interval * (this.hit + 1))
            //    {
            //        this.DeBuffDamage();
            //    }
            //}
            if (this.time > this.Define.Duraction)
            {
                this.OnRemove();
            }
        }
    }
}
