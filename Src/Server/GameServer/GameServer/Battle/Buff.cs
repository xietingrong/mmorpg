using Common;
using Common.Data;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    class Buff
    {
        public int BuffId;
        private Creature Owner;
        private BuffDefine Define;
        private BattleContext Context;
        internal bool Stoped;
        private float time;
        private int hit;

        public Buff(int buffId, Creature owner, BuffDefine define, BattleContext context)
        {
            this.BuffId = buffId;
            this.Owner = owner;
            this.Define = define;
            this.Context = context;

            this.OnAdd();
        }

        private void OnAdd()
        {
            if(this.Define.Effect!= Common.Battle.BuffEffect.None)
            {
                this.Owner.EffectMgr.AddBuffEffect(this.Define.Effect);
            }
            AddAttr();
            NBuffInfo buff = new NBuffInfo()
            {
                buffId = this.BuffId,
                buffType = this.Define.ID,
                casterId = this.Context.Caster.entityId,
                ownerId = this.Owner.entityId,
                Action = BuffACTION.Add,
            };
            Context.Battle.AddBuffAction(buff);
            
        }
        private void OnRemove()
        {
            RemoveAttr();
            Stoped = true;
            if (this.Define.Effect != Common.Battle.BuffEffect.None)
            {
                this.Owner.EffectMgr.RemoveEffect(this.Define.Effect);
            }
            NBuffInfo buff = new NBuffInfo()
            {
                buffId = this.BuffId,
                buffType = this.Define.ID,
                casterId = this.Context.Caster.entityId,
                ownerId = this.Owner.entityId,
                Action = BuffACTION.Remove,
            };
            Context.Battle.AddBuffAction(buff);
        }


        internal void Update()
        {
            if (Stoped) return;
            this.time += Time.deltaTime;
            if(this.Define.Interval >0)
            {
                if(this.time > this.Define.Interval * (this.hit +1))
                {
                    this.DeBuffDamage();
                }
            }
            if(this.time >this.Define.Duraction)
            {
                this.OnRemove();
            }
        }
        private void AddAttr()
        {
            if(this.Define.DEFRatio!= 0)
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
      
       

        private void DeBuffDamage()
        {
            this.hit++;
            NDamageInfo damage = this.CalcBuffDamage(Context.Caster);
            Log.InfoFormat("Buff[{0}].DoBuffDamage[{1}] Damage:{2} Crit:{3}", this.Define.Name, this.Owner.Name, damage.Damage, damage.Crit);
            this.Owner.DoDamage(damage, Context.Caster);
            NBuffInfo buff = new NBuffInfo()
            {
                buffId = this.BuffId,
                buffType = this.Define.ID,
                casterId = this.Context.Caster.entityId,
                ownerId = this.Owner.entityId,
                Action = BuffACTION.Hit,
            };
            Context.Battle.AddBuffAction(buff);
        }

        private NDamageInfo CalcBuffDamage(Creature caster)
        {
            float ad = this.Define.AD + caster.Attributes.AD * this.Define.ADFactor;
            float ap = this.Define.AP + caster.Attributes.AP * this.Define.APFactor;

            float addmg = ad * (1 - this.Owner.Attributes.DEF / (this.Owner.Attributes.DEF + 100));
            float apdmg = ad * (1 - this.Owner.Attributes.MDEF / (this.Owner.Attributes.MDEF + 100));

            float final = addmg + apdmg;
           
   
            NDamageInfo damage = new NDamageInfo();
            damage.entityId = this.Owner.entityId;
            damage.Damage = Math.Max(1, (int)final);
            return damage;
        }
    }
}
