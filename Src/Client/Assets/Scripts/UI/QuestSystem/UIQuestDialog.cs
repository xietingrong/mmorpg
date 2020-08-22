using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIQuestDialog:UIWindow
{
    public UIQuestInfo questInfo;
    public Quest quest;
    public GameObject openButtons;
    public GameObject submiButtons;
    private void Start()
    {
        
    }
    public void SetQuest(Quest quest)
    {
        this.quest = quest;
        this.UpdateQuest();
        if(this.quest.Info == null)
        {
            openButtons.SetActive(true);
            submiButtons.SetActive(false);
        }
        else
        {
            if(this.quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
            {
                openButtons.SetActive(true);
                submiButtons.SetActive(false);
            }
            else
            {
                openButtons.SetActive(false);
                submiButtons.SetActive(false);
            }
        }
    }

    private void UpdateQuest()
    {
      if(this.quest!= null)
        {
            if(this.questInfo!= null)
            {
                this.questInfo.SetQuestinfo(quest);
            }
        }
    }
}

