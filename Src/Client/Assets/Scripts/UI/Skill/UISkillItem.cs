using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UISkillItem : ListView.ListViewItem
{
    public Image icon;
    public Text title;
    public Text level;

    public Image background;
    public Sprite normalBg;
    public Sprite selectedBg;

    public override void onSelected(bool selected)
    {
        this.background.overrideSprite = selected ? selectedBg : normalBg;
    }


    public SkillDefine item;
    private void Start()
    {

    }
    public void SetItem(SkillDefine item,UISkill owner, bool equiped)
    {
        this.item = item;
     
        if (this.title != null) this.title.text = this.item.Name;
        if (this.level != null) this.level.text = item.UnlockLevel.ToString();
        if (this.icon != null) this.icon.overrideSprite = Resloader.Load<Sprite>(this.item.Icon);
    }
}


