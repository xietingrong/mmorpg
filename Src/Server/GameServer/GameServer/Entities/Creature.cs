using Common.Battle;
using Common.Data;
using GameServer.AI;
using GameServer.Battle;
using GameServer.Core;
using GameServer.Managers;
using GameServer.Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Entities
{
    class Creature : Entity
    {
        public int Spwnum = 0;
        public int CanAttNum = 0;
        public bool IsAttOver = false;
        public bool IsAtt = false;
        public int ZhengYing = 0;//1红方 2绿方
        public AIAgent AI;
        public bool IsAiUse = false;
        public int Id { get; set; }
        public string Name { get { return this.Info.Name; } }

        public NCharacterInfo Info;
        public CharacterDefine Define;
        public SkillManager SkillMgr;
        public BuffManager BuffMgr;
        public EffectManager EffectMgr;
        public Attributes Attributes;
        public bool isDeath = false;
        public CharacterState state;
        public CharState BattleState;
        public Map map;
        
       
        public Creature(CharacterType type, int configId, int level, Vector3Int pos, Vector3Int dir) :
           base(pos, dir)
        {
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Level = level;
            this.Info.ConfigId = configId;
            this.Info.Entity = this.EntityData;
            this.Info.EntityId = this.entityId;
            this.Define = DataManager.Instance.Characters[configId];
            this.Info.Name = this.Define.Name;
          
            InitSkills();
            InitBuffs();
            this.Attributes = new Attributes();
            this.Attributes.Init(this.Define, this.Info.Level, this.GetEquips(), this.Info.attrDynamic);
            this.Info.attrDynamic = this.Attributes.DynamicAttr;
        }

  
        internal void DoDamage(NDamageInfo damage,Creature source)
        {
            this.BattleState = CharState.InBattle;
            this.Attributes.HP -= damage.Damage;
            if(this.Attributes.HP <0)
            {
                this.isDeath = true;
                damage.WillDead = true;
            }
            this.OnDamage(damage, source);
        }
       
       
        internal  int Distance(Creature target)
        {
            return (int)Vector3Int.Distance(this.Position, target.Position);
        }
        public int Distance(Vector3Int position)
        {
            return (int)Vector3Int.Distance(this.Position, position);
        }
        private float usecd = 0;
        public override void Update()
        {
            this.SkillMgr.Update();
            this.BuffMgr.Update();
            
            //if(IsAiUse && !IsAtt)
               this.AI.Update();
            if (usecd > 0)
                usecd -= Time.deltaTime;
            if (usecd <= 0)
                IsAttOver = true;
        }

        void InitSkills()
        {
            SkillMgr = new SkillManager(this);
            this.Info.Skills.AddRange(this.SkillMgr.Infos);
        }

        private void InitBuffs()
        {
            BuffMgr = new BuffManager(this);
            EffectMgr = new EffectManager(this);
        }

        public virtual List<EquipDefine> GetEquips()
        {
            return null;
        }

        internal void CastSkill(BattleContext context, int skillId)
        {
            Skill skill = this.SkillMgr.GetSkill(skillId);
            context.Result = skill.Cast(context);
            if(context.Result == SkillResult.Ok)
            {
                this.BattleState = CharState.InBattle;
            }
            if(context.CastSkill == null)
            {
                if(context.Result ==SkillResult.Ok)
                {
                    context.CastSkill = new NSkillCastInfo()
                    {
                        casterId = this.entityId,
                        targetId = context.Target.entityId,
                        skillId = skill.Define.ID,
                        Postion = new NVector3(),
                        Result = context.Result,
                    };
                    this.IsAtt = true;
                    usecd = skill.Define.Cd;
                    context.Battle.AddCastSkillInfo(context.CastSkill);
                }
            }
            else
            {
                context.CastSkill.Result = context.Result;
                context.Battle.AddCastSkillInfo(context.CastSkill);
            }
        }

        internal void AddBuff(BattleContext context, BuffDefine buffDefine)
        {
            this.BuffMgr.AddBuff(context, buffDefine);
        }
        protected virtual void OnDamage(NDamageInfo damge,Creature source)
        {

        }
        public virtual void OnEnterMap(Map map)
        {
            this.map = map;
            if(CharacterType.Player == this.Info.Type && this.map.Define.Type == MapType.Arena)
            {
                this.ZhengYing = 1;
                this.map.AddBattle(this);
            }
        }
        public void OnLeave(Map map)
        {
            this.map = null;
        }

        public virtual Skill FindSkill(BattleContext context, SkillType type)
        {
            Skill cancast = null;
         
            return cancast;
        }
        public virtual void MoveTo(Vector3Int position)
        {
           
        }
        public virtual void UpdateMovement()
        {
            
        }

        public virtual void SopMove()
        {
           
        }
    }
}
