using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIShopItem  : MonoBehaviour,ISelectHandler
{
    private UIShop shop;
    public int ShopItemID { get; set; }
    private ShopItemDefine ShopItem { get; set; }
    private ItemDefine item;

    public Image icon;
    public Text title;
    public Text price;
    public Text count;
    public Image background;
    public Sprite NormalBg;
    public Sprite selectedBg;
    private bool selected;
    public bool Selected
    {
        get { return selected; }
        set
        {
            selected = value;
            this.background.overrideSprite = selected ? selectedBg : NormalBg;
        }
    }
    private void Start()
    {
        
    }
    public void SetShopItem(int id,ShopItemDefine shopItem,UIShop owner)
    {
        this.shop = owner;
        this.ShopItemID = id;
        this.ShopItem = shopItem;
        this.item = DataManager.Instance.Items[this.ShopItem.ItemID];

        this.title.text =this.item.Name;
        this.count.text = ShopItem.Count.ToString();
        this.price.text = ShopItem.Price.ToString();
        this.icon.overrideSprite = Resloader.Load<Sprite>(item.Icon);
    }
    public void OnSelect(BaseEventData eventData)
    {
        this.Selected = true;
        this.shop.SelectShopItem(this);
    }
}
