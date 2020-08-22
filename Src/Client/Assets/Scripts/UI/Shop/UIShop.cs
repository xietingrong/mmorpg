using Common.Data;
using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : UIWindow
{
    public Text title;
    public Text money;
    public GameObject shopItem;
    ShopDefine shop;
    public Transform[] itemRoot;
    void Start()
    {
        StartCoroutine(InitItems());
    }

    IEnumerator InitItems()
    {
        int count = 0;
        int page = 0;
        foreach (var kv in DataManager.Instance.ShopItems[shop.ID])
        {
            if(kv.Value.Status >0)
            {
                GameObject go = Instantiate(shopItem, itemRoot[page]);
                UIShopItem ui = go.GetComponent<UIShopItem>();
              
                ui.SetShopItem(kv.Key,kv.Value,this);
                count++;
                if(count >=10)
                {
                    count = 0;
                    page++;
                    itemRoot[page].gameObject.SetActive(true);
                }
            }
        }
           yield return null;
    }
    public void SetShop(ShopDefine shop)
    {
        this.shop = shop;
        this.title.text = shop.Name;
        this.money.text = User.Instance.CurrentCharacterInfo.Gold.ToString();
    }
    private UIShopItem selectShopItem;
    public void SelectShopItem(UIShopItem item)
    {
        if(selectShopItem != null)
        {
            selectShopItem.Selected = true;
        }
        selectShopItem = item;
    }
    public void OnClickBuy()
    {

    }
}


