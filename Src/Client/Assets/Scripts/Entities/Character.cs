using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Battle;
using Common.Data;
using Managers;
using SkillBridge.Message;
using UnityEngine;

namespace Entities
{
    public class Character : Creature
    {
        internal bool ready;

        public Character(NCharacterInfo info):base(info)
        {

        }
        public override List<EquipDefine> GetEquips()
        {
            return  EquipManager.Instance.GetEquipDefines();
        }

    }
}
