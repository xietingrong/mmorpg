using Battle;
using Common.Data;
using Managers;
using Models;
using SkillBridge.Message;
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
         overlay.enabled = false;
          cdText.enabled = false;
    }
    void Update()
    {
        if (this.skill == null) return;
        if(this.skill.CD > 0)
        {
            if (!overlay.enabled) overlay.enabled = true;
            if (!cdText.enabled) this.cdText.enabled = true;

            overlay.fillAmount = this.skill.CD / this.skill.Define.Cd;
            this.cdText.text = ((int)Math.Ceiling(this.skill.CD)).ToString();

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
    public void OnPositionSelected(Vector3 pos)
    {
        BattleManager.Instance.CurrentPostion = GameObjectTool.WorldToLogicN(pos);
        this.CastSkil();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(this.skill.Define.CastTarget == Common.Battle.TargetType.Position)
        {
            TargetSelector.ShowSelector(User.Instance.CurrentCharacter.position, this.skill.Define.CastRange, this.skill.Define.AOERange, this.OnPositionSelected);
            return;
        }
        CastSkil();
    }
    private void CastSkil()
    {
        SkillResult result = this.skill.canCast(BattleManager.Instance.CurrentTarget);
        switch (result)
        {
            case SkillResult.InvalidTarget:
                MessageBox.Show("技能：[" + this.skill.Define.Name + "]目标无效");
                break;
            case SkillResult.OutOfMp:
                MessageBox.Show("技能：[" + this.skill.Define.Name + " ]MP 不足");
                break;
            case SkillResult.CoolDown:
                MessageBox.Show("技能：[" + this.skill.Define.Name + " ]长、正在冷却");
                break;
            case SkillResult.OutOfRange:
                MessageBox.Show("技能：[" + this.skill.Define.Name + " ]施法范围");
                break;
        }
      
        BattleManager.Instance.CastSkill(this.skill);
       
    }


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

