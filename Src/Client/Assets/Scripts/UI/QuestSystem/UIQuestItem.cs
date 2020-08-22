using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestItem : ListView.ListViewItem
{
    Quest quest;
    public Text title;
    public Image background;
    public Sprite nomalBg;
    public Sprite SelectedBg;
    public override void onSelected(bool selected)
    {
        this.background.overrideSprite = selected ? SelectedBg : nomalBg;
    }
    private void Start()
    {
        
    }
    bool isEquiped = false;
    public void SetQUestInfo(Quest item)
    {
        this.quest = item;
        if (this.title != null) this.title.text = this.quest.Define.Name;
    }
}
