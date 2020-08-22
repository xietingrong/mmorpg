
using Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIGuildList:UIWindow
{
    public GameObject itemPrefab;
    public ListView listMain;
    public Transform itemRoot;
    public UIGuildInfo uiInfo;
    public UIGuildItem selectedItem;
    void Start()
    {    
        this.listMain.onItemSelected += this.OnGuildMemberSelected;
        this.uiInfo.Info = null;
        GuildService.Instance.OnGuildListResult += UpdateGuildList;
        GuildService.Instance.SendGuildListRequest();
    }
    private void OnDestroy()
    {
        GuildService.Instance.OnGuildListResult -= UpdateGuildList;
    }
    void UpdateGuildList(List<NGuildInfo>guild)
    {
        ClearFriendList();
        InitItems(guild);
    }


    private void InitItems(List<NGuildInfo> guild)
    {
        foreach (var item in guild)
        {
            GameObject go = Instantiate(itemPrefab, this.listMain.transform);
            UIGuildItem ui = go.GetComponent<UIGuildItem>();

            ui.SetGuildInfo(item);
            this.listMain.AddItem(ui);
        }

    }
    public void OnGuildMemberSelected(ListView.ListViewItem item)
    {
        this.selectedItem = item as UIGuildItem;
        this.uiInfo.Info = this.selectedItem.Info;
    }
    private void ClearFriendList()
    {
        this.listMain.RemoveAll();
    }
    public void OnClickJoin()
    {
        if (selectedItem == null)
        {
            MessageBox.Show("请选择要加入的工会");
            return;
        }
        MessageBox.Show(string.Format("确定要加入工会【{0}】吗？", selectedItem.Info.GuildName), "申请加入工会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendGuildJoinRequest(this.selectedItem.Info.Id);
        };

    }
    private void Update()
    {

    }
}

