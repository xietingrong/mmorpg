using Entities;
using Managers;
using Models;
using Services;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMain :MonoSingleton<UIMain> {

    public Text avatarName;
    public Text avatarLevel;
    public UITeam teamwindow;
    public UICreatureInfo targetUI;
    public UISkillSlots skillSlots;
    internal bool Show;

    // Use this for initialization
    void Start () {
        this.UpdateAvatar();
        this.targetUI.gameObject.SetActive(false);
        BattleManager.Instance.OnTargetChanged += onTargetChanged;
        User.Instance.OnCharacterInit += this.skillSlots.UpdateSkills;
        this.skillSlots.UpdateSkills();

    }

    private void onTargetChanged(Creature target)
    {
        if(target!=null)
        {
            if (!targetUI.isActiveAndEnabled) targetUI.gameObject.SetActive(true);
            targetUI.Target = target;
        }
        else
        {
            targetUI.gameObject.SetActive(false);
        }
    }

    void UpdateAvatar()
    {
        this.avatarName.text = string.Format("{0}[{1}]", User.Instance.CurrentCharacterInfo.Name, User.Instance.CurrentCharacterInfo.Id);
        this.avatarLevel.text = User.Instance.CurrentCharacterInfo.Level.ToString();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void OnClickBack()
    {
        Services.UserService.Instance.SendGameLeave();
        SceneManager.Instance.LoadScene("CharSelect");
    }
    public void OnClickText()
    {
        UIManager.Instance.Show<UIText>();
    }
    public void OnClickBag()
    {
        UIManager.Instance.Show<UIBag>();
    }
    public void OnClickCharEquip()
    {
        UIManager.Instance.Show<UICharEquip>();
    }
    public void OnClickQuest()
    {
        UIManager.Instance.Show<UIQuestSystem>();
    }
    public void OnClickRide()
    {
       // UIManager.Instance.Show<UIRide>();
    }
    public void ShowTeamUI(bool show)
    {
        teamwindow.ShowTeam(show);
    }
    public void OnClickGuild()
    {
        GuildManager.Instance.ShowGuild();
    }
}
