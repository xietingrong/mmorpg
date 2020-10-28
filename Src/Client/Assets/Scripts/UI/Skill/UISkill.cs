using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
public class UISkill : UIWindow
{
    public GameObject itemPrefab;
    public Text descript;
    public ListView listMain;
    public UISkillItem selectedItem;

    private bool ShowAvailableList = false;
    private void Start()
    {
        this.listMain.onItemSelected += this.OnQuestSelected;
        RefreshUI();
    }

    private void RefreshUI()
    {
        ClearItem();
        InitItems();
    }

    private void InitItems()
    {
        var Skills = DataManager.Instance.Skills[(int)User.Instance.CurrentCharacterInfo.Class];
        foreach(var kv in Skills)
        {
            if(kv.Value.Type == Common.Battle.SkillType.Skill)
            {
                GameObject go = Instantiate(itemPrefab, this.listMain.transform);
                UISkillItem ui = go.GetComponent<UISkillItem>();
                ui.SetItem(kv.Value, this, false);
                this.listMain.AddItem(ui);
            }
        }
    }

    private void ClearItem()
    {
        this.listMain.RemoveAll();
        
    }

    private void OnQuestSelected(ListView.ListViewItem item)
    {
        this.selectedItem = item as UISkillItem;
        this.descript.text = this.selectedItem.item.Description;

    }
    private void OnDestroy()
    {
       
    }
}
