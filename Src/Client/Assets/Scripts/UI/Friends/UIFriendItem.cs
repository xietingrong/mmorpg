using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIFriendItem : ListView.ListViewItem
{
  
    public Text nickname;
    public Text @class;
    public Text level;
    public Text status;
    public Image background;
    public Sprite nomalBg;
    public Sprite SelectedBg;
    public override void onSelected(bool selected)
    {
        this.background.overrideSprite = selected ? SelectedBg : nomalBg;
    }
    public NFriendInfo info;
    private void Start()
    {

    }
    bool isEquiped = false;
    public void SetFriendInfo(NFriendInfo item)
    {
        this.info = item;
        if (this.nickname != null) this.nickname.text = this.info.friendInfo.Name;
        if (this.@class != null) this.@class.text = this.info.friendInfo.Class.ToString();
        if (this.level != null) this.level.text = this.info.friendInfo.Level.ToString();
        if (this.status != null) this.status.text = this.info.Status ==1?"在线":"离线";
    }
}





