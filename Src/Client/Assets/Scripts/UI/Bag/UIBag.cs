using Managers;
using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIBag:UIWindow
{
    public Text money;
    public Transform[] pages;
    public GameObject bagItem;
    List<Image> slots;
     void Start()
    {
        if(slots ==null)
        {
            slots = new List<Image>();
            for(int page =0; page< this.pages.Length;page++)
            {
                slots.AddRange(this.pages[page].GetComponentsInChildren<Image>(true));
            }
            StartCoroutine(InitBags());

        }
    }

    IEnumerator InitBags()
    {
        for (int i = 0; i < BagManager.Instance.items.Length; i++)
        {
            var item = BagManager.Instance.items[i];
            if(item.ItemId >0)
            {
                GameObject go = Instantiate(bagItem, slots[i].transform);
                var ui = go.GetComponent<UIIconItem>();
                var def = ItemManager.Instance.Items[item.ItemId].Define;
                ui.SetMainIcon(def.Icon, item.Count.ToString());
            }
        }
        for (int i = BagManager.Instance.items.Length; i <slots.Count; i++)
        {
            slots[i].color = Color.gray;
        }
        yield return null;
    }
    public void SetTitle(string title)
    {
        this.money.text = User.Instance.CurrentCharacterInfo.Id.ToString();
    }
    void Clear()
    {
        for (int i = 0; i <slots.Count; i++)
        {
            if(slots[i].transform.childCount > 0)
            {
                Destroy(slots[i].transform.GetChild(0).gameObject);
            }
        }
    }
    public void OnReset()
    {
        BagManager.Instance.Reset();
        this.Clear();
        StartCoroutine(InitBags());
    }
}

