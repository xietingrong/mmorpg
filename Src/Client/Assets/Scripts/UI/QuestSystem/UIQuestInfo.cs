using Managers;
using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestInfo : MonoBehaviour 
{
    public Text title;
    public Text[] targets;
    public Text descripation;
    public UIIconItem rewardItems;
    public Text rewardMoney;
    public Text rewardExp;
    public Text overview;
    public Button npcButton;
    private int npc = 0;
    private void Start()
    {
        
    }
    public void SetQuestinfo(Quest quest)
    {
        this.title.text = string.Format("[{0}]{1}", quest.Define.Type, quest.Define.Name);
        if (this.overview != null) this.overview.text = quest.Define.Overview;
        if (this.overview != null)
        {
            if (quest.Info == null)
            {
                this.descripation.text = quest.Define.Dialog;
            }
            else
            {
                if (quest.Info.Status == QuestStatus.Complated)
                {
                    this.descripation.text = quest.Define.DialogFinish;
                }
            }
        }
        this.rewardMoney.text = quest.Define.RewardGold.ToString();
        this.rewardExp.text = quest.Define.RewardExp.ToString();
        if(quest.Info == null)
        {
            this.npc = quest.Define.AcceptNpc;
        }
        else if(quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
        {
            this.npc = quest.Define.SubmitNpc;
        }
        this.npcButton.gameObject.SetActive(this.npc > 0);

        foreach( var fitter in this.GetComponentsInChildren<ContentSizeFitter>())
        {
            fitter.SetLayoutVertical();
        }
    }
    public void OnClickAbandon()
    {

    }
    public void OnClickNav()
    {
        Vector3 pos = NpcManager.Instance.GetNpcPostion(this.npc);
        User.Instance.CurrentCharacterObject.StartNav(pos);
        UIManager.Instance.Close<UIQuestSystem>();
    }
    private void Update()
    {
        
    }
}

