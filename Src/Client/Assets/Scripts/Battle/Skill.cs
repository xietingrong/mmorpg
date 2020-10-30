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
using SkillResult = SkillBridge.Message.SkillResult;

namespace Battle
{
    public class Skill
    {
        public NSkillInfo Info;
        public Creature Owner;
        public SkillDefine Define;
        // public NDamageInfo Damage;
        public Creature Target;
        private NVector3 TargetPostion;
        private bool IsCasting = false;

        public float castTime;
        private float skillTime;
        private float cd =0;
        public int Hit;
        private SkillStatus Status;

        Dictionary<int, List<NDamageInfo>> HitMap = new Dictionary<int, List<NDamageInfo>>();
        List<Bullet> Bullets = new List<Bullet>();
        public float CD
        {
            get { return cd; }
        }
        public Skill(NSkillInfo info,Creature owner)
        {
            this.Info = info;
            this.Owner = owner;
            this.Define = DataManager.Instance.Skills[this.Owner.Define.TID][this.Info.Id];
            this.cd = 0;
        }
        public SkillResult canCast( Creature target)
        {
            if(this.Define.CastTarget == TargetType.Target )
            {
                if(target == null|| target == this.Owner)
                      return SkillResult.InvalidTarget;
                int distance = this.Owner.Distance(target);
                if(distance >this.Define.CastRange)
                {
                    return SkillResult.OutOfRange;

                }
            }
            if (this.Define.CastTarget == TargetType.Position && BattleManager.Instance.CurrentPostion ==null)
            {
                return SkillResult.InvalidTarget;
            }
            if(this.Owner.Attributes.MP < this.Define.MPCost)
            {
                return SkillResult.OutOfMp;
            }
            if(this.cd >0)
            {
                return SkillResult.CoolDown;
            }
            return SkillResult.Ok;
        }
        public void BeginCast(Creature target,NVector3 pos)
        {
            this.IsCasting = true;
            this.castTime = 0;
            this.skillTime = 0;
            this.cd = this.Define.Cd;
            this.Hit = 0;
            this.Target = target;
            this.TargetPostion = pos;
            this.Owner.PlayAnim(this.Define.SkillAnim) ;
            this.Bullets.Clear();
            this.HitMap.Clear();
            if(this.Define.CastTarget ==Common.Battle.TargetType.Position)
            {
                if(this.TargetPostion!= null)
                    this.Owner.FaceTo(this.TargetPostion.ToVector3Int());
            }
            else if (this.Define.CastTarget == Common.Battle.TargetType.Target)
            {
                if(this.Target!= null)
                    this.Owner.FaceTo(this.Target.position);
            }
            if (this.Define.CastTime > 0)
            {
                this.Status = SkillStatus.Casting;
            }
            else
            {
                StarSkill();
            }

        }
        void StarSkill()
        {
            this.Status = SkillStatus.Runing;
            if (!string.IsNullOrEmpty(this.Define.AOEEffect))
            {
                if (this.Define.CastTarget == Common.Battle.TargetType.Position)
                {
                    this.Owner.PlayEffect(EffectType.Position, this.Define.AOEEffect, this.TargetPostion);
                }
                else if (this.Define.CastTarget == Common.Battle.TargetType.Target)
                {
                    this.Owner.PlayEffect(EffectType.Position, this.Define.AOEEffect, this.Target);
                }
                else if (this.Define.CastTarget == Common.Battle.TargetType.Self)
                {
                    this.Owner.PlayEffect(EffectType.Position, this.Define.AOEEffect, this.Owner);
                }
            }
        }
        public void OnUpdate(float delta)
        {
            UpdateCD(delta);
            if (this.Status == SkillStatus.Casting)
            {
                this.UpdateCasting();
            }
            else if (this.Status == SkillStatus.Runing)
            {
                this.UpdateSkill();
            }
           
        }
        private void UpdateCasting()
        {
            if (this.castTime < this.Define.CastTime)
            {
                this.castTime += Time.deltaTime;
            }
            else
            {
                this.castTime = 0;
                this.StarSkill();
                Debug.LogFormat("Skill{0}.UpdateCasting Finsh", this.Define.Name);
            }
        }
        private void UpdateSkill()
        {
            this.skillTime += Time.deltaTime;
            if (this.Define.Duraction > 0)
            {//持续技能
                if (this.skillTime > this.Define.Interval * (this.Hit + 1))
                {
                    this.DoHit();
                }
                if (this.skillTime >= this.Define.Duraction)
                {
                    this.Status = SkillStatus.None;
                    Debug.LogFormat("Skill{0}UpdateSkill Finsh", this.Define.Name);
                }

            }
            else if (this.Define.HitTimes != null && this.Define.HitTimes.Count > 0)
            {
                if (this.Hit < this.Define.HitTimes.Count)
                {
                    if (this.skillTime > this.Define.HitTimes[this.Hit])
                    {
                        this.DoHit();
                    }

                }
                else
                {
                    if (!this.Define.Bullet)
                    {
                        this.Status = SkillStatus.None;
                        Debug.LogFormat("Skill{0}UpdateSkill Finsh", this.Define.Name);
                    }
                }
            }

            if (this.Define.Bullet)
            {
                bool finish = true;
                foreach (Bullet bullet in this.Bullets)
                {
                    bullet.Update();
                    if (!bullet.Stoped) finish = false;
                }
                if (finish && this.Hit >= this.Define.HitTimes.Count)
                {
                    this.Status = SkillStatus.None;
                    this.IsCasting = false;
                    Debug.LogFormat("Skill{0}UpdateSkill Finsh", this.Define.Name);
                }
            }

        }
        private void DoHit()
        {
           

            if (this.Define.Bullet)
            {
                this.CastBullet();
            }
            else
            {
                this.DoHitDamages(this.Hit);
              
            }
            this.Hit++;
        }

        public void DoHitDamages(int hit)
        {
            List<NDamageInfo> damages;
            if(this.HitMap.TryGetValue(hit,out damages))
            {
                DoHitDamages(damages);
            }
        }

        void CastBullet()
        {
            Bullet bullet = new Bullet(this);
            Debug.LogFormat("Skill{0}.CastBullet[{1}] Target:{2}", this.Define.Name, this.Define.BulletResource, this.Target!= null? this.Target.Name:"");

            this.Bullets.Add(bullet);
            this.Owner.PlayEffect(EffectType.Bullet, this.Define.BulletResource, this.Target, bullet.duration);
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
        internal void DoHit(NSkillHitInfo hit)
        {
            if(hit.isBullet|| !this.Define.Bullet)
            {
                this.DoHit(hit.hitId, hit.Damages);
            }
        }

        internal void DoHit(int hitId, List<NDamageInfo> damages)
        {
            if (hitId > this.Hit)
                this.HitMap[hitId] = damages;
            else
                DoHitDamages(damages);
        }
        private void DoHitDamages(List<NDamageInfo> damages)
        {
            foreach (var dmg in damages)
            {
                Creature target = EntityManager.Instance.GetEntity(dmg.entityId) as Creature;
                if (target == null) continue;
                target.DoDamage(dmg,true);
                if(this.Define.HitEffect!= null)
                {
                    target.PlayEffect(EffectType.Hit, this.Define.HitEffect, target);
                }
            }
        }
    }
}
