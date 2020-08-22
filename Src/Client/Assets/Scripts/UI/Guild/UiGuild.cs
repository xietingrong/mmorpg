
using Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIGuild : UIWindow
{
    public GameObject itemPrefab;
    public ListView listMain;
    public Transform itemRoot;
    public UIGuildInfo uiInfo;
    public UIGuildMemberItem seletedItem;
    public GameObject panelAdmin;
    public GameObject panelLeader;
    void Start()
    {
        GuildService.Instance.OnGuildUpdate = UpdateUI;
        this.listMain.onItemSelected += this.OnGuildMemberSelected;
        UpdateUI();
    }
    private void OnDestroy()
    {
        GuildService.Instance.OnGuildUpdate = UpdateUI;
    }
    private void UpdateUI()
    {
        this.uiInfo.Info = GuildManager.Instance.guildInfo;
        ClearList();
        InitItems();
        this.panelAdmin.SetActive(GuildManager.Instance.myMemberInfo.Title > GuildTitle.None);
        this.panelLeader.SetActive(GuildManager.Instance.myMemberInfo.Title == GuildTitle.President);
    }
   

    private void InitItems()
    {
        foreach (var item in GuildManager.Instance.guildInfo.Members)
        {
            GameObject go = Instantiate(itemPrefab, this.listMain.transform);
            UIGuildMemberItem ui = go.GetComponent<UIGuildMemberItem>();

            ui.SetGuildMemberInfo(item);
            this.listMain.AddItem(ui);
        }
    }

    private void ClearList()
    {
        this.listMain.RemoveAll();
    }

    private void Update()
    {

    }
    public void OnGuildMemberSelected(ListView.ListViewItem item)
    {
        this.seletedItem = item as UIGuildMemberItem;

    }
    public void OnClickAppliesList()
    {
        UIManager.Instance.Show<UIGuildApplyList>();
    }
    public void OnClickAppLeave()
    {
       
    }
    public void onClickChat()
    {

    }
    public void OnClickKickout()
    {
        if(seletedItem == null)
        {
            MessageBox.Show("请选择要踢出的成员");
            return;
        }
        MessageBox.Show(string.Format("要踢【{0}】出工会吗？", this.seletedItem.Info.Info.Name), "踢出工会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendAdminCommand(GuildAdminCommand.Kickout, this.seletedItem.Info.Info.Id);
        };
    }
    public void onClickPromote()
    {
        if(seletedItem == null)
        {
            MessageBox.Show("请选择要晋升的成员");
            return;
        }
        if(seletedItem.Info.Title!= GuildTitle.None)
        {
            MessageBox.Show("对方已经身份尊贵");
            return;
        }
      
        MessageBox.Show(string.Format("要晋身【{0}】为工会副会长吗？", this.seletedItem.Info.Info.Name), "晋升", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendAdminCommand(GuildAdminCommand.Promote, this.seletedItem.Info.Info.Id);
        };
    }
    public void onClickDepose()
    {
        if (seletedItem == null)
        {
            MessageBox.Show("请选择要罢免的成员");
            return;
        }
        if (seletedItem.Info.Title == GuildTitle.None)
        {
            MessageBox.Show("对方藐视无职可免");
            return;
        }
        if (seletedItem.Info.Title == GuildTitle.President)
        {
            MessageBox.Show("会长不是你能动的");
            return;
        }
        MessageBox.Show(string.Format("要罢免【{0}】出工会吗？", this.seletedItem.Info.Info.Name), "罢免职务", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendAdminCommand(GuildAdminCommand.Depost, this.seletedItem.Info.Info.Id);
        };
    }
    public void onClickTransfer()
    {
        if (seletedItem == null)
        {
            MessageBox.Show("请选择要把会长转让给你的成员");
            return;
        }
        
        MessageBox.Show(string.Format("要把会长转给【{0}】吗？", this.seletedItem.Info.Info.Name), "专业工会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendAdminCommand(GuildAdminCommand.Transfer, this.seletedItem.Info.Info.Id);
        };
    }
    public void onClickSetNotice()
    {

    }
}



