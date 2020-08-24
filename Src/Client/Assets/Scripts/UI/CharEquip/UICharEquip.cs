using Common.Battle;
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
    public Text hp;
    public Slider hpbar;
    public Text mp;
    public Slider mpbar;
    public Text[] attrs;

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
        this.money.text = User.Instance.CurrentCharacterInfo.Gold.ToString();

        InitAttributes();
    }

    private void InitAttributes()
    {
        var charattr = User.Instance.CurrentCharacter.Attributes;
        this.hp.text = string.Format("{0}/{1}", charattr.HP, charattr.MaxHP);
        this.mp.text = string.Format("{0}/{1}", charattr.MP, charattr.MaxMp);
        this.hpbar.maxValue = charattr.MaxHP;
        this.hpbar.value = charattr.HP;
        this.mpbar.maxValue = charattr.MaxMp;
        this.mpbar.value = charattr.MP;
        for (int i = (int)AttributeType.STR; i <= (int)AttributeType.DEX; i++)
        {
            if (i == (int)AttributeType.CRI)
                this.attrs[i - 2].text = string.Format("{0:f2}%", charattr.Final.Data[i] * 100);
            else
                this.attrs[i - 2].text =((int) charattr.Final.Data[i]).ToString();
        }
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

