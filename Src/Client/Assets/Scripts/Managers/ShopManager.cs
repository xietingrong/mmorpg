using Common.Data;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managers
{
    public class ShopManager : Singleton<ShopManager>
    {
       public void Init()
        {
            NpcManager.Instance.RegisterNpcEvent(NpcFunction.InvokeShop, OnOpenShop);
        }

        private bool OnOpenShop(NpcDefine npc)
        {
            this.ShowShop(npc.Param);
            return true;
        }

        private void ShowShop(int shopId)
        {
            ShopDefine shop;
            if(DataManager.Instance.Shops.TryGetValue(shopId,out shop))
            {
                UIShop uiShop = UIManager.Instance.Show<UIShop>();
                if(uiShop!= null)
                {
                    uiShop.SetShop(shop);
                }
            }
        }
        public bool BuyItem(int shopId,int shopItemId)
        {
            ItemService.Instance.SendBuyItem(shopId, shopItemId);
            return true;
        }
    }
}
