using Managers;
using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UICharEquip : UIWindow
{
    public Text title;
    public Text money;
    public GameObject itemPrefab;
    public GameObject itemEquipedPrefab;
    public Transform itemListRoot;
    public List<Transform> slots;
    private void Start()
    {
        ReFreshUI();
        EquipManager.Instance.OnEquipChanged += ReFreshUI;
    }

    private void ReFreshUI()
    {
        ClearAllEquipList();
        InitAllEquipItems();
        ClearEquipList();
        InitEquipedItem();
        this.money.text = User.Instance.CurrentCharacter.Gold.ToString();
    }

    private void OnDestroy()
    {
        EquipManager.Instance.OnEquipChanged -= ReFreshUI;
    }
    public void InitAllEquipItems()
    {
        foreach(var kv in ItemManager.Instance.Items)
        {
            if(kv.Value.Define.Type == ItemType.Equip)
            {
                if (EquipManager.Instance.Contains(kv.Key))
                    continue;
                GameObject go = Instantiate(itemPrefab, itemListRoot);
                UIEquipItem ui = go.GetComponent<UIEquipItem>();

                ui.SetEquipItem(kv.Key, kv.Value, this,false);
            }
        }
    }
    void ClearAllEquipList()
    {
        foreach(var item in itemListRoot.GetComponentsInChildren<UIEquipItem>())
        {
            Destroy(item.gameObject);
        }
    }
    void ClearEquipList()
    {
        foreach (var item in slots)
        {
            if(item.childCount >0)
                 Destroy(item.GetChild(0).gameObject);
        }
    }
    public void InitEquipedItem()
    {
        for (int i = 0; i < (int)EquipSlot.SlotMax;i++)
        {
            var item = EquipManager.Instance.Equips[i];
            if(item!= null)
            {
                GameObject go = Instantiate(itemPrefab, itemListRoot);
                UIEquipItem ui = go.GetComponent<UIEquipItem>();

                ui.SetEquipItem(i, item, this, true);
            }
             
        }
    }
    public void DoEquip(Item item)
    {
        EquipManager.Instance.EquipItem(item);
    }
    public void UnEquip(Item item)
    {
        EquipManager.Instance.UnEquipItem(item);
    }

}

