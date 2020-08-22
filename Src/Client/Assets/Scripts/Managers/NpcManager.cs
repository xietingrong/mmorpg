using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Managers
{
    public class NpcManager : Singleton<NpcManager>
    {
        public delegate bool NpcActionHandler(NpcDefine npc);

        Dictionary<NpcFunction, NpcActionHandler> EventMap = new Dictionary<NpcFunction, NpcActionHandler>();
        Dictionary<int, Vector3> npcPostions = new Dictionary<int, Vector3>();

        public void RegisterNpcEvent(NpcFunction  function, NpcActionHandler action)
        {
            if (!EventMap.ContainsKey(function))
                EventMap[function] = action;
            else
                EventMap[function] += action;
        }
        public NpcDefine GetNpcDefine(int npcID)
        {
            return DataManager.Instance.Npcs[npcID];
        }
        public bool Interactive(int npcId)
        {
            if (DataManager.Instance.Npcs.ContainsKey(npcId))
            {
                var npc = DataManager.Instance.Npcs[npcId];
                return Interactive(npc);
            }
                return false;
        }
        public bool Interactive(NpcDefine npc)
        {
            if(DoTaskInteractive(npc))
            {
                return true;
            }
            if (npc.Type == NpcType.Functional)
            {
                return DoFunctionInteractive(npc);
            }
            return false;
        }

        private bool DoFunctionInteractive(NpcDefine npc)
        {
            if(npc.Type != NpcType.Functional)
               return false;
            if (!EventMap.ContainsKey(npc.Function))
                return false;
            return EventMap[npc.Function](npc);
        }

        private bool DoTaskInteractive(NpcDefine npc)
        {
            var status = QuestManager.Instance.GetQuestStatusByNpc(npc.ID);
            if (status == NpcQuestStatus.None)
                return false;
            return QuestManager.Instance.OpenNpcQuest(npc.ID);
        }
        public void UpdateNpcPostion(int npc,Vector3 pos)
        {
            this.npcPostions[npc] = pos;
        }
        internal Vector3 GetNpcPostion(int npc)
        {
            return this.npcPostions[npc];
        }
    }
}
