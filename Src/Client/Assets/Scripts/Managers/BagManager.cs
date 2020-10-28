using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Managers
{
    class BagManager : Singleton<BagManager>
    {
        public int Unlocked;
        public BagItem[] items;
        NBagInfo Info;
       

        unsafe public void Init(NBagInfo info)
        {
            this.Info = info;
            this.Unlocked = info.Unlocked;
            items = new BagItem[this.Unlocked];
            if(info.Items!= null && info.Items.Length>= this.Unlocked)
            {
                Anylyze(info.Items);
            }
            else
            {
                int v = sizeof(BagItem);
                Info.Items = new byte[v * this.Unlocked];
                Reset();
            }

        }

        

        public void Reset()
        {
            int i = 0;
            foreach (var kv in ItemManager.Instance.Items)
            {
                if (kv.Value.Count<= kv.Value.Define.StackLimit )
                {
                    this.items[i].ItemId = (ushort)kv.Key;
                    this.items[i].Count = (ushort)kv.Value.Count;
                }
                else
                {
                    int count = kv.Value.Count;
                    while(count > kv.Value.Define.StackLimit)
                    {
                        this.items[i].ItemId = (ushort)kv.Key;
                        this.items[i].Count = (ushort)kv.Value.Count;
                        i++;
                        count -= kv.Value.Count;
                    }
                    this.items[i].ItemId = (ushort)kv.Key;
                    this.items[i].Count = (ushort)count;
                }
                i++;
            }
        }
        unsafe void Anylyze(byte[] data)
        {
            fixed(byte *pt =data)
            {
                for (int i=0;i < this.Unlocked;i++)
                {
                    BagItem* item = (BagItem*)(pt + i * sizeof(BagItem));
                    items[i] = *item;
                }
            }
        }
        unsafe public NBagInfo  GetNBagInfo()
        {
            fixed (byte* pt = Info.Items)
            {
                for (int i = 0; i < this.Unlocked; i++)
                {
                    BagItem* item = (BagItem*)(pt + i * sizeof(BagItem));
                    *item = items[i];
                }
            }
            return this.Info;
        }
        public void AddItem(int itemId, int count)
        {
            ushort addCount = (ushort)count;
            for (int i = 0; i < this.Unlocked; i++)
            {
                if(this.items[i].ItemId == itemId)
                {
                    ushort canAdd = (ushort)(DataManager.Instance.Items[itemId].StackLimit - this.items[i].Count);
                    if(canAdd >= addCount)
                    {
                        this.items[i].Count += addCount;
                        canAdd = 0;
                        break;
                    }
                    else
                    {
                        this.items[i].Count += canAdd;
                        addCount -= canAdd;
                    }
                }
            }
            if(addCount >0)
            {
                for (int i = 0; i < this.items.Length; i++)
                {
                    if(this.items[i].ItemId == 0)
                    {
                        this.items[i].ItemId = (ushort)itemId;
                        this.items[i].Count = addCount;
                        break;
                    }
                }
            }
        }
        public void RemoveItem(int itemId, int count)
        {

        }
    }
}
