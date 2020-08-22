using Common;
using GameServer.Entities;
using GameServer.Services;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class EquipManager:Singleton<EquipManager>
    {
 
        public Result EquipItem(NetConnection<NetSession> sender, int slot, int itemId,bool isEquip)
        {
            Character charactor = sender.Session.Character;
            if (!charactor.ItemManager.Items.ContainsKey(itemId))
                return Result.Failed;
            UpdateEquip(charactor.Data.Equips, slot, itemId, isEquip);
            DBService.Instance.Save();
            return Result.Failed;
        }
        unsafe void UpdateEquip(byte[] equipData,int slot,int itemId,bool isEquip)
        {
            fixed(byte* pt = equipData)
            {
                int* slotid = (int*)(pt + slot * sizeof(int));
                if (isEquip)
                    *slotid = itemId;
                else
                    *slotid = 0;
            }
        }
    }
}
