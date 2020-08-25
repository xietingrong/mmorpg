using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace Managers
{
    public enum NpcQuestStatus
    {
        None =0,//无任务
        Complete,//拥有已完成可提交任务
        Available,//拥有可接受任务
        Incomolete,//拥有未完成任务
    }
    class QuestManager : Singleton<QuestManager>
    {
        //所有有效任务
        public List<NQuestInfo> questInfos;
        public Dictionary<int, Quest> allQuests = new Dictionary<int, Quest>();
        public Dictionary<int,Dictionary<NpcQuestStatus,List<Quest>>>npcQuests = new   Dictionary<int, Dictionary<NpcQuestStatus, List<Quest>>>();
        public UnityAction<Quest> onQuestStatusChanged;

        public UnityAction OnQuestChange;

        public void Init(List<NQuestInfo> quests)
        {
            this.questInfos = quests;
            allQuests.Clear();
            this.npcQuests.Clear();
            InitQuests();
        }

        private void InitQuests()
        {
            //初始化已有任务
            foreach (var info in this.questInfos)
            {
                Quest quest = new Quest(info);
                this.AddNpcQuest(quest.Define.AcceptNpc, quest);
                this.AddNpcQuest(quest.Define.SubmitNpc, quest);
                this.allQuests[quest.Info.QuestId] = quest;
            }
            //初始化可用任务
            foreach (var kv in DataManager.Instance.Quests)
            {
                if (kv.Value.LimitClass != CharacterClass.None && kv.Value.LimitClass != User.Instance.CurrentCharacterInfo.Class)
                    continue;//不符合职业
                if (kv.Value.LimitLevel > User.Instance.CurrentCharacterInfo.Level)
                    continue;//不符合等级
                if (this.allQuests.ContainsKey(kv.Key))
                    continue;//任务已存在
                if(kv.Value.PreQuest>0)
                {
                    Quest preQuest;
                    if (this.allQuests.TryGetValue(kv.Value.PreQuest, out preQuest))//获取前置任务
                    {
                        if (preQuest.Info == null)
                            continue;//前置任务未接取
                        if (preQuest.Info.Status != QuestStatus.Finished)
                            continue;//前置任务未完成
                    }
                    else
                        continue;//前置任务未完成
                 
                }
                Quest quest = new Quest(kv.Value);
                this.AddNpcQuest(quest.Define.AcceptNpc, quest);
                this.AddNpcQuest(quest.Define.SubmitNpc, quest);
                this.allQuests[quest.Define.ID] = quest;
            }
        }

        private void AddNpcQuest(int npcId, Quest quest)
        {
            if (!this.npcQuests.ContainsKey(npcId))
                this.npcQuests[npcId] = new Dictionary<NpcQuestStatus, List<Quest>>();
            List<Quest> availables;
            List<Quest> complates;
            List<Quest> incomplates;
            if(!this.npcQuests[npcId].TryGetValue(NpcQuestStatus.Available,out availables))
            {
                availables = new List<Quest>();
                this.npcQuests[npcId][NpcQuestStatus.Available] = availables;
            }
            if (!this.npcQuests[npcId].TryGetValue(NpcQuestStatus.Complete, out complates))
            {
                complates = new List<Quest>();
                this.npcQuests[npcId][NpcQuestStatus.Complete] = complates;
            }
            if (!this.npcQuests[npcId].TryGetValue(NpcQuestStatus.Incomolete, out incomplates))
            {
                incomplates = new List<Quest>();
                this.npcQuests[npcId][NpcQuestStatus.Incomolete] = incomplates;
            }
            if(quest.Info != null)
            {
                if(npcId ==quest.Define.AcceptNpc && !this.npcQuests[npcId][NpcQuestStatus.Available].Contains(quest))
                {
                    this.npcQuests[npcId][NpcQuestStatus.Available].Add(quest);
                }
                else
                {
                    if ( quest.Define.SubmitNpc == npcId && quest.Info.Status == QuestStatus.Complated )
                    {
                        if(!this.npcQuests[npcId][NpcQuestStatus.Complete].Contains(quest))
                        {
                            this.npcQuests[npcId][NpcQuestStatus.Complete].Add(quest);
                        }
                        
                    }
                    if (quest.Define.SubmitNpc == npcId && quest.Info.Status == QuestStatus.InProgress)
                    {
                        if (!this.npcQuests[npcId][NpcQuestStatus.Incomolete].Contains(quest))
                        {
                            this.npcQuests[npcId][NpcQuestStatus.Incomolete].Add(quest);
                        }

                    }
                }
            }
        }
        public NpcQuestStatus GetQuestStatusByNpc(int npcId)
        {
            Dictionary<NpcQuestStatus, List<Quest>> status = new Dictionary<NpcQuestStatus, List<Quest>>();
            if(this.npcQuests.TryGetValue(npcId,out status))
            {
                if (status[NpcQuestStatus.Complete].Count > 0)
                    return NpcQuestStatus.Complete;
                if (status[NpcQuestStatus.Available].Count > 0)
                    return NpcQuestStatus.Available;
                if (status[NpcQuestStatus.Incomolete].Count > 0)
                    return NpcQuestStatus.Incomolete;
            }
            return NpcQuestStatus.None;
        }
        public bool OpenNpcQuest(int npcId)
        {
            Dictionary<NpcQuestStatus, List<Quest>> status = new Dictionary<NpcQuestStatus, List<Quest>>();

            if (this.npcQuests.TryGetValue(npcId, out status))
            {
                if (status[NpcQuestStatus.Complete].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Complete].First());
                if (status[NpcQuestStatus.Available].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Available].First());
                if (status[NpcQuestStatus.Incomolete].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Incomolete].First());
            }
            return false;
        }

        private bool ShowQuestDialog(Quest quest)
        {
           if(quest.Info == null || quest.Info.Status ==QuestStatus.Complated)
            {
                UIQuestDialog dlg = UIManager.Instance.Show<UIQuestDialog>();
                dlg.SetQuest(quest);
                dlg.OnClose += OnQuestDialogClose;
                return false;
            }
           if(quest.Info!= null || quest.Info.Status == QuestStatus.Complated)
            {
                if (!string.IsNullOrEmpty(quest.Define.DialogIncomplete))
                    MessageBox.Show(quest.Define.DialogIncomplete);
            }
                return true;
        }

        private void OnQuestDialogClose(UIWindow sender, UIWindow.WindowResult result)
        {
            UIQuestDialog dlg = (UIQuestDialog)sender;
            if(result == UIWindow.WindowResult.Yes)
            {
                MessageBox.Show(dlg.quest.Define.DialogAccept);
            }
            else if(result == UIWindow.WindowResult.No)
            {
                MessageBox.Show(dlg.quest.Define.DialogDeny);
            }
        }
        public void OnQuestAccepted(NQuestInfo quest)
        {

        }
        public void onQuestSubmited(NQuestInfo quest)
        {

        }
    }
  
}
