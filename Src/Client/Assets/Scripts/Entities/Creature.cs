using Battle;
using Common.Battle;
using Common.Data;
using Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Entities
{

    public class Creature : Entity
    {
        public NCharacterInfo Info;

        public CharacterDefine Define;

        public Attributes Attributes;

        public SkillManager SkillMgr;
        public BuffManager BuffMgr;
        public EffectManager EffectMgr;
        bool battleState = false;
        public Action<Buff> OnBuffAdd;
        public Action<Buff> OnBuffRemove;
        public bool BattleStates
        {
            get { return battleState; }
            set {
                if (battleState != value)
                {
                    battleState = value;
                    this.SetStandby(value);
                }
            }
        }
        public Skill CastringSkill = null;


        public int Id
        {
            get { return this.Info.Id; }
        }

        public string Name
        {
            get
            {
                if (this.Info.Type == CharacterType.Player)
                    return this.Info.Name;
                else
                    return this.Define.Name;
            }
        }

   

        public bool IsPlayer
        {
            get
            {
                return this.Info.Type == CharacterType.Player;
            }
        }

        public bool IsCurrentPlayer
        {
            get
            {
                if (!IsPlayer) return false;
                return this.Info.Id == Models.User.Instance.CurrentCharacterInfo.Id;
            }
        }

  

        internal int Distance(Creature target)
        {
            return (int)Vector3Int.Distance(this.position, target.position);
        }
        internal int Distance(Vector3Int position)
        {
            return (int)Vector3Int.Distance(this.position, position);
        }
        public Creature(NCharacterInfo info) : base(info.Entity)
        {
            this.Info = info;
            this.Define = DataManager.Instance.Characters[info.ConfigId];
            this.Attributes = new Attributes();
            this.Attributes.Init(this.Define, this.Info.Level, this.GetEquips(), this.Info.attrDynamic);
            this.SkillMgr = new SkillManager(this);
            this.BuffMgr = new BuffManager(this);
            this.EffectMgr = new EffectManager(this);
        }
        public void UpdateInfo(NCharacterInfo info)
        {
            this.SetEntityData(info.Entity);
            this.Info = info;
            this.Attributes.Init(this.Define, this.Info.Level, this.GetEquips(), this.Info.attrDynamic);
            this.SkillMgr.UpdateSkills();
        }

        internal void FaceTo(Vector3Int postion)
        {
            this.SetDirection(GameObjectTool.WorldToLogic(GameObjectTool.LogicToWorld(postion - this.position).normalized));
            this.UpdateEntityData();
            if (this.Controller != null)
                this.Controller.UpdateDirection();
        }

        public virtual List<EquipDefine> GetEquips()
        {
            return null;
        }
        public void MoveForward()
        {
            Debug.LogFormat("MoveForward");
            this.speed = this.Define.Speed;
        }

        public void MoveBack()
        {
            Debug.LogFormat("MoveBack");
            this.speed = -this.Define.Speed;
        }

        public void Stop()
        {
            Debug.LogFormat("Stop");
            this.speed = 0;
        }
     
        internal void CastSkill(int skillId, Creature target, NVector3 postion)
        {
            this.SetStandby(true);
            var skill = this.SkillMgr.GetSkill(skillId);
            skill.BeginCast(target,postion);
        }

        private void SetStandby(bool standby)
        {
            if (this.Controller != null)
                this.Controller.SetStandby(standby);
        }
        public void PlayAnim(string SkillAnim)

        {
            if (this.Controller != null)
                this.Controller.PlayAnim(SkillAnim);
        }
        public override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);
            this.SkillMgr.OnUpdate(delta);
            this.BuffMgr.OnUpdate(delta);
        }

        

        public void SetDirection(Vector3Int direction)
        {
            Debug.LogFormat("SetDirection:{0}", direction);
            this.direction = direction;
        }

        public void SetPosition(Vector3Int position)
        {
            Debug.LogFormat("SetPosition:{0}", position);
            this.position = position;
        }
        //public void DoDamage(NDamageInfo damage)
        //{
        //    this.Attributes.HP -= damage.Damage;
        //    if (this.Attributes.HP <= 0)
        //    {
        //        this.IsDeath = true;
        //        damage.WillDead = true;
        //    }
        //   this.DoDamage(damage,source)
        //}
        public void DoDamage(NDamageInfo damage,bool playHurt)
        {
            Debug.LogFormat("DoDamage:{0}", damage.Damage);
            this.Attributes.HP  -= damage.Damage;
            //if(this.Attributes.HP <= 0)
            //    this.PlayAnim("Dead");
            //else
            if (playHurt)
                this.PlayAnim("Hurt");
            if (this.Controller != null)
                UIWorldElementManager.Instance.ShowPopupText(PopupType.Damage, this.Controller.GetTransform().position + this.GetPopupOffset(), -damage.Damage, damage.Crit);
        }
        internal void DoSkillHit(NSkillHitInfo hit)
        {
            var skill = this.SkillMgr.GetSkill(hit.skillId);
            skill.DoHit(hit);
        }

        internal void DOBuffAction(NBuffInfo buff)
        {
            switch (buff.Action)
            {
                case BuffACTION.Add:
                    this.AddBuff(buff.buffId, buff.buffType, buff.casterId);
                    break;
                case BuffACTION.Remove:
                    this.RemoveBuff(buff.buffId);
                    break;
                case BuffACTION.Hit:
                    this.DoDamage(buff.Damge,false);
                    break;
                default:
                    break;
            }
        }

        public void AddBuff(int buffId, int buffType, int casterId)
        {
            var buff =this.BuffMgr.AddBuff(buffId, buffType, casterId);
            if(buff != null && this.OnBuffAdd!= null)
            {
                this.OnBuffAdd(buff);
            }
        }
       
        public void RemoveBuff(int buffId)
        {
            var buff = this.BuffMgr.RemoveBuff(buffId);
            if (buff != null&& this.OnBuffRemove!= null)
            {
                this.OnBuffRemove(buff);
            }
        }
        internal void AddBuffEffect(BuffEffect effect)
        {
            this.EffectMgr.AddBuffEffect(effect);
        }
        internal void RemoveBuffEffect(BuffEffect effect)
        {
            this.EffectMgr.RemoveBuffEffect(effect);
        }
        public void PlayEffect(EffectType type, string name, NVector3 postion)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (this.Controller != null)
                this.Controller.PlayEffect(type, name, postion, 0);
        }
        public void PlayEffect(EffectType type,string name,Creature target,float duration = 0)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (this.Controller != null)
                this.Controller.PlayEffect(type, name, target, duration);
        }
        public Vector3 GetPopupOffset()
        {
            return new Vector3(0, this.Define.Height , 0);
        }
        internal Vector3 GetHitOffset()
        {
            return new Vector3(0, this.Define.Height * 0.8f, 0);
        }
    }
}
