using Managers;
using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIRide:UIWindow
{
    public Text title;

    public GameObject itemPrefab;
    public ListView listMain;
    private UiRideItem selectedItem;
    public Text descript;
    private void Start()
    {
        ReFreshUI();
        this.listMain.onItemSelected += OnItemSelected;
    }

    private void OnItemSelected(ListView.ListViewItem item)
    {
        this.selectedItem = item as UiRideItem;
        this.descript.text = this.selectedItem.item.Define.Description;
    }

    private void ReFreshUI()
    {
        ClearItems();
        InitItems();
    
    }

    private void OnDestroy()
    {
       
    }
    public void InitItems()
    {
        foreach (var kv in ItemManager.Instance.Items)
        {
            if (kv.Value.Define.Type == ItemType.Ride
                && (kv.Value.Define.LimitClass ==CharacterClass.None.ToString() || kv.Value.Define.LimitClass == User.Instance.CurrentCharacterInfo.Class.ToString())
                )
            {
              
                GameObject go = Instantiate(itemPrefab, this.listMain.transform);
                UiRideItem ui = go.GetComponent<UiRideItem>();

                ui.SetRideItem(kv.Value, this, false);
                this.listMain.AddItem(ui);
            }
        }
    }
    void ClearItems()
    {
        this.listMain.RemoveAll();
    }
    public void DoRide()
    {
        if(this.selectedItem == null)
        {
            MessageBox.Show("请选择要召唤的坐骑", "提示");
            return;
        }
        User.Instance.Ride(this.selectedItem.item.Id);
    }
    
}

