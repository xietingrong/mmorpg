using GameServer.Entities;
using GameServer.Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    class SkillManager
    {
        private Creature Owner;
        public List<NSkillInfo> Infos { get; private set; }
        public List<Skill> skills { get; private set; }
        public Skill NormalSkill { get; private set; }
        public SkillManager(Creature owner)
        {
            this.Owner = owner;
            this.skills = new List<Skill>();
            this.Infos = new List<NSkillInfo>();
            this.InitSkills();
        }

        private void InitSkills()
        {
            this.skills.Clear();
            this.Infos.Clear();
            if (!DataManager.Instance.Skills.ContainsKey(this.Owner.Define.TID))
                return;
            
            foreach (var define in DataManager.Instance.Skills[this.Owner.Define.TID])
            {
                NSkillInfo info = new NSkillInfo();
                info.Id = define.Key;
                if(this.Owner.Info.Level >= define.Value.UnlockLevel)
                {
                    info.Level = 5;
                }
                else
                {
                    info.Level = 1;
                }
                this.Infos.Add(info);
      
                Skill skill = new Skill(info, this.Owner);
                if(define.Value.Type ==Common.Battle.SkillType.Normal)
                {
                    NormalSkill = skill;
                }
                this.AddSkill(skill);
            }
           
        }
       


        private void AddSkill(Skill skill)
        {
            this.skills.Add(skill);
        }

        internal Skill GetSkill(int skillId)
        {
            for(int i =0; i< this.skills.Count;i++)
            {
                if (this.skills[i].Define.ID == skillId)
                    return this.skills[i];
            }
            return null;
        }

        internal void Update()
        {
            for (int i = 0; i < this.skills.Count; i++)
            {
                this.skills[i].Update();
            }
        }
    }
   
}
