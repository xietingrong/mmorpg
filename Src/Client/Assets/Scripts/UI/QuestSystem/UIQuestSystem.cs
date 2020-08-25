using Common.Data;
using Managers;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIQuestSystem : UIWindow
{
    public UIText title;
    public GameObject itemPrefab;
    public TabView Tabs;
    public ListView listMain;
    public ListView listBranch;
    public UIQuestInfo questInfo;

    private bool ShowAvailableList = false;
    private void Start()
    {
        this.listMain.onItemSelected += this.OnQuestSelected;

        this.listBranch.onItemSelected += this.OnQuestSelected;
        this.Tabs.OnTabSelect += this.OnSelectTab;
        RefreshUI();
        QuestManager.Instance.OnQuestChange +=RefreshUI;
    }

    private void RefreshUI()
    {
        ClearAllQuestList();
        InitAllQustItems();
    }

    private void InitAllQustItems()
    {
       foreach(var kv in QuestManager.Instance.allQuests)
        {
            if(ShowAvailableList)
            {
                if (kv.Value.Info != null)
                    continue;
            }
            else
            {
                if (kv.Value.Info == null)
                    continue;
            }
            GameObject go = Instantiate(itemPrefab, kv.Value.Define.Type == QuestType.Main?listMain.transform:listBranch.transform);
            UIQuestItem ui = go.GetComponent<UIQuestItem>();
            ui.SetQUestInfo(kv.Value);
            if (kv.Value.Define.Type == QuestType.Main)
                this.listMain.AddItem(ui as ListView.ListViewItem);
            else
                this.listBranch.AddItem(ui as ListView.ListViewItem);
        }
    }

    private void ClearAllQuestList()
    {
        this.listMain.RemoveAll();
        this.listBranch.RemoveAll();
    }
 
    private void OnQuestSelected(ListView.ListViewItem item )
    {
        UIQuestItem qustItem = item as UIQuestItem;
    }
    private void OnDestroy()
    {
         QuestManager.Instance.OnQuestChange -= RefreshUI;
    }
    private void OnSelectTab(int idx)
    {
        ShowAvailableList =idx ==1;
        RefreshUI();
    }
 
   
}


