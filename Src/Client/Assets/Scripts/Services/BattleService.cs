using Entities;
using Managers;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Services
{
    class BattleService : Singleton<BattleService>, IDisposable
    {
        public void Init()
        {
            MessageDistributer.Instance.Subscribe<SkillCastResponse>(this.OnSkillCast);
        }

      
        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<SkillCastResponse>(this.OnSkillCast);
        }
        public void SendSkillCast(int skillId,int casterId,int targetId, NVector3 postion)
        {
            if (postion == null) postion = new NVector3();
          
            Debug.LogFormat("SendSkillCast: skill:{0} caster:{1} post:{2}", skillId, casterId, targetId, postion.ToString());
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.skillCast = new SkillCastRequest();
       
            message.Request.skillCast.castInfo.skillId = skillId;
            message.Request.skillCast.castInfo.casterId = casterId;
            message.Request.skillCast.castInfo.targetId = targetId;
            message.Request.skillCast.castInfo.Postion = postion;
            NetClient.Instance.SendMessage(message);
        }
        private void OnSkillCast(object sender, SkillCastResponse message)
        {
            Debug.LogFormat("OnSkillCast: skill:{0} caster:{1} post:{2}", message.castInfo.skillId, message.castInfo.casterId, message.castInfo.targetId, message.castInfo.Postion.ToString());
            if(message.Result == Result.Success)
            {
                Creature caster = EntityManager.Instance.GetEntity(message.castInfo.casterId) as Creature;
                if(caster == null)
                {
                    Creature target = EntityManager.Instance.GetEntity(message.castInfo.casterId) as Creature;
                    caster.CastSkill(message.castInfo.skillId, target, message.castInfo.Postion);
                }
            }
            else
            {
                ChatManager.Instance.AddSystemMessage(message.Errormsg);
            }
        }

    }
}
