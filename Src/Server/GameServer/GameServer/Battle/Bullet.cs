using Common;
using GameServer.Battle;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battle
{
    class Bullet
    {
        private Skill skill;
        NSkillHitInfo hitInfo;
        bool TimeMode = true;
        float duration = 0;
        float flyTime = 0;
        public bool Stoped = false;
        public Bullet(Skill skill, Creature target, NSkillHitInfo hitInfo)
        {
            this.skill = skill;
            this.hitInfo = hitInfo;
            int distance = skill.Owner.Distance(target);
            if(TimeMode)
            {
                duration = distance / this.skill.Define.BulletSpeed;
            }
            Log.InfoFormat(" Bullet.CastBullet[{0}] casterId:[1] Distance:[2] Time;{3}", this.skill.Define.Name, hitInfo.casterId,distance,this.duration);
        }
        public void Update()
        {
            if (Stoped) return;
            if(TimeMode)
            {
                this.UpdateTime();
            }
            else
            {
                this.UpdatePos();
            }
        }

        private void UpdatePos()
        {
            //int distance = skill.Owner.Distance(target);
            //if(distance >50)
            //{
            //    pos += speed * Time.deltaTime;
            //}
            //else
            //{
            //    this.hitInfo.isBullet = true;
            //    this.skill.DoHit(this.hitInfo);
            //    this.stoped = true;
            //}
        }

        private void UpdateTime()
        {
            this.flyTime += Time.deltaTime;
            if(this.flyTime >duration)
            {
                this.hitInfo.isBullet = true;
                this.skill.DoHit(this.hitInfo);
                this.Stoped = true;
            }
        }
    }
}
