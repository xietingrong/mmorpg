using Common;
using GameServer.Entities;
using GameServer.Models;
using GameServer.Services;
using SkillBridge.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
   
    class ItemManager
    {
        Character Owner;

        public Dictionary<int, Item> Items = new Dictionary<int, Item>();

        public ItemManager(Character owner)
        {
            this.Owner = owner;
           foreach(var item in owner.Data.Items)
           {
                this.Items.Add(item.ItemID, new Item(item));
           }
        }
      
        public bool UseItem(int itemId, int count = 1)
        {
            Log.InfoFormat("[{0}]UseItem[{1}:{2}]", this.Owner.Data.ID, itemId, count);
            Item item = null;
            if(this.Items.TryGetValue(itemId,out item))
            {
                if(item.Count<count)
                {
                    return false;
                }
                //Tdd0 增加使用逻辑
                item.Remove(count);
                return true;
            }
            return false;
        }
        public bool HasItem(int itemId)
        {
            Item item = null;
            if (this.Items.TryGetValue(itemId, out item))
            {
                return item.Count > 0;
            }
            return false;
        }
        public Item GetItem(int itemId)
        {
            Item item = null;
            this.Items.TryGetValue(itemId, out item);
            Log.InfoFormat("[{0}]GetItem[{1}:{2}]", this.Owner.Data.ID, itemId, item);
            return item;
           
        }

        public bool AddItem(int itemId, int count = 1)
        {
            Item item = null;
            if (this.Items.TryGetValue(itemId, out item))
            {
                item.Add(count);
            }
            else
            {
                TCharacterItem dbItem = new TCharacterItem();
                dbItem.CharacterID = Owner.Data.ID;
                dbItem.Owner = Owner.Data;
                dbItem.ItemID = itemId;
                dbItem.ItemCount = count;
                Owner.Data.Items.Add(dbItem);
                item = new Item(dbItem);
                this.Items.Add(itemId, item);
            }
            Log.InfoFormat("[{0}]AddItem[{1}:{2}]", this.Owner.Data.ID, itemId, count);
            DBService.Instance.Save();
            return true;
        }
        public bool RemoveItem(int ItemId, int count = 1)
        {
          
            if (!this.Items.ContainsKey(ItemId))
            {
                return false;
            }
            Item item = this.Items[ItemId];
            if (item.Count < count)
                return false;
            item.Remove(count);
            Log.InfoFormat("[{0}]AddItem[{1}:{2}]", this.Owner.Data.ID, ItemId, count);
            DBService.Instance.Save();
            return true;
        }
        public void GetItemInfos(List<NItemInfo> list)
        {
            foreach (var item in this.Items)
            {
                list.Add(new NItemInfo() { Id = item.Value.ItemID, Count = item.Value.Count });
            }
        }
    }
}
