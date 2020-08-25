using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle
{
    public class SkillManager
    {
        Creature Owner;
        public List<Skill> skills { get; private set; }
        public SkillManager(Creature owner)
        {
            this.Owner = owner;
            this.skills = new List<Skill>();
            this.InitSkills();
        }

        private void InitSkills()
        {
            this.skills.Clear();
            foreach(var skillInfo in this.Owner.Info.Skills)
            {
                Skill skill = new Skill(skillInfo, this.Owner);
                this.AddSkill(skill);
            }
        }

        private void AddSkill(Skill skill)
        {
            this.skills.Add(skill);
        }
        public Skill GetSkill(int skillId)
        {
            for (int i = 0; i<this.skills.Count;i++)
            {
                if (this.skills[i].Define.ID == skillId)
                    return this.skills[i];
            }
                return null;
        }
        public void OnUpdate(float delta)
        {
            for (int i = 0; i < this.skills.Count; i++)
            {
                  this.skills[i].OnUpdate(delta);
            }
        }
    }
}
