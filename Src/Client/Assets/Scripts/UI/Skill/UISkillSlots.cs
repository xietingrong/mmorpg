using Battle;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UISkillSlots:MonoBehaviour
{
    public UISkillSlot[] slots;
    void Start()
    {
      
    }

    public void UpdateSkills()
    {
        if (User.Instance.CurrentCharacter == null) return;
        var Skills = User.Instance.CurrentCharacter.SkillMgr.skills; 
        int skillIdx = 0;

        foreach (var skill in Skills)
        { 
            slots[skillIdx].SetSkill(skill);
            skillIdx++;
        }
    }
}


