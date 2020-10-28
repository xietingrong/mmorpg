using Managers;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIFriends:UIWindow
{
    public GameObject itemPrefab;
    public ListView listMain;
    public Transform itemRoot;
    public UIFriendItem seletedItem;
    void Start()
    {
        FriendService.Instance.OnFriendUpdate = RefreshUI;
        this.listMain.onItemSelected += this.OnFriendSelected;
        RefreshUI();
    }

    private void RefreshUI()
    {
        ClearFriendList();
        InitFriendItems();
    }

    private void InitFriendItems()
    {
        foreach(var item in FriendManager.Instance.allFriends)
        {
            GameObject go = Instantiate(itemPrefab, this.listMain.transform);
            UIFriendItem ui = go.GetComponent<UIFriendItem>();

            ui.SetFriendInfo(item);
            this.listMain.AddItem(ui);
        }
    }

    private void ClearFriendList()
    {
        this.listMain.RemoveAll();
    }

    private void Update()
    {
        
    }
    public void OnFriendSelected(ListView.ListViewItem item)
    {
        this.seletedItem = item as UIFriendItem;
        
    }
    public void OnClickFriendAdd()
    {
        InputBox.Show("输入要添加的好友名称或ID", "添加好友").OnSubmit += OnFriendAddSubmit;
    }

    private bool OnFriendAddSubmit(string input, out string tips)
    {
        tips = "";
        int friendId = 0;
        string friendName = "";
        if (!int.TryParse(input, out friendId))
            friendName = input;
        if (friendId == User.Instance.CurrentCharacterInfo.Id|| friendName == User.Instance.CurrentCharacterInfo.Name)
         {
            tips = "开玩笑吗？不能添加自己哟";
            return false;
        }
        FriendService.Instance.SendFriendAddRequest(friendId, friendName);
        return true;
    }
    public void OnClickChat()
    {
        MessageBox.Show("暂未开放");
    }
    public void OnClickFriendItemInvite()
    {
        if (seletedItem == null)
        {
            MessageBox.Show("请选择要邀请的好友");
        }
        if (seletedItem.info.Status ==0)
        {
            MessageBox.Show("请选择在线的好友");
        }
        MessageBox.Show(string.Format("确定要邀请好友【{0}】加入队伍吗？",seletedItem.info.friendInfo.Name),"邀请好友组队", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            TeamService.Instance.SendTeamInviteRequest(this.seletedItem.info.friendInfo.Id, this.seletedItem.info.friendInfo.Name);
        };
    }
    public void OnClickChange()
    {
        if (seletedItem == null)
        {
            MessageBox.Show("请选择要挑战的好友");
        }
        if (seletedItem.info.Status == 0)
        {
            MessageBox.Show("请选择在线的好友");
        }
        MessageBox.Show(string.Format("确定要好友【{0}】进行竞技场挑战吗？", seletedItem.info.friendInfo.Name), "竞技场挑战", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            ArenaService.Instance.SendArenaChangeRequest(this.seletedItem.info.friendInfo.Id, this.seletedItem.info.friendInfo.Name);
        };
    }
     public void OnClickFriendRemove()
    {
        if (seletedItem == null)
        {
            MessageBox.Show("请选择要删除的好友");
            return;
        }
        MessageBox.Show(string.Format("确定要继续删除好友[{0}]吗？", seletedItem.info.friendInfo.Name), "删除好友", MessageBoxType.Confirm, "删除", "取消").OnYes = () =>
          {
              FriendService.Instance.SendFriendAddRequest(this.seletedItem.info.Id, this.seletedItem.info.friendInfo.Name);
          };
    
    }
}

