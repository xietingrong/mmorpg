using Battle;
using Common.Battle;
using Common.Data;
using Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISkillSlot : MonoBehaviour,IPointerClickHandler
{
    public Image icon;
    public Image overlay;
    public Text cdText;
    Skill skill;
    float overlaySpeed = 0;
    float cdRemain = 0;
 
    private void Start()
    {

    }
    void Update()
    {
        if(overlay.fillAmount >0)
        {
            overlay.fillAmount = this.cdRemain / this.skill.Cd;
            this.cdText.text = ((int)Math.Ceiling(this.cdRemain)).ToString();
            this.cdRemain -= Time.deltaTime;
        }
        else
        {
            if (overlay.enabled) overlay.enabled = false;
            if (this.cdText.enabled) this.cdText.enabled = false;
        }
    }
    public void SetSkill(Skill item)
    {
        this.skill = item;
        if (this.icon != null) this.icon.overrideSprite = Resloader.Load<Sprite>(this.skill.Define.Icon);
        this.SetCD(this.skill.Define.Cd);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SkillResult result = this.skill.canCast(BattleManager.Instance.CurrentTarget);
        switch (result)
        {
            case SkillResult.InvalidTarget:
                MessageBox.Show("技能：[" + this.skill.Define.Name + "] 目标无效");
                break;
            case SkillResult.OutOfMP:
                MessageBox.Show("技能：[" + this.skill.Define.Name + " ]MP 不足");
                break;
            case SkillResult.Cooldown:
                MessageBox.Show("技能：[" + this.skill.Define.Name + " ]长、正在冷却");
                break;
        }
        BattleManager.Instance.CastSkill(this.skill);
    }
    //     MessageBox.Show("释放技能：" + this.skill.Name );
    //     this.SetCD(this.skill.Define.Cd);
    //    this.skill.Cast();
    //}

    private void SetCD(float cd)
    {
        if (!this.overlay.enabled) overlay.enabled = true;
        if (!this.cdText.enabled) cdText.enabled = true;
        this.cdText.text = ((int)Math.Floor(this.cdRemain)).ToString();
        this.overlay.fillAmount = 1f;
        overlaySpeed = 1f / cd;
        cdRemain = cd;
    }
   
}

