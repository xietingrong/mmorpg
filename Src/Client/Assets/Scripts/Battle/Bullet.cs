using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Battle
{
    class Bullet
    {
        Skill skill;

        int hit = 0;
        public float duration = 0;
        float flyTime = 0;
        public bool Stoped = false;
        public Bullet(Skill skill)
        {
            this.skill = skill;
            var target = skill.Target;
            int distance = 0;
            if(target!= null)
                distance= skill.Owner.Distance(target);
            this.hit = skill.Hit;
            duration = distance / this.skill.Define.BulletSpeed;
        }
        public void Update()
        {
            if (Stoped) return;
            this.flyTime += Time.deltaTime;
            if(this.flyTime > duration)
            {
                this.skill.DoHitDamages(this.hit);
                this.Stop();
            }
        }
        public void Stop()
        {
            this.Stoped = true;
        }
    }
}
