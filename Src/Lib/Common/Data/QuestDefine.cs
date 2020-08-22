using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Common.Data
{
    public enum QuestType
    {
        [Description("主线")]
        Main,
        [Description("支线")]
        Branch
    }
    public enum QuestTarget
    {
        None,
        Kill,
        Item
    }
    public class QuestDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int LimitLevel { get; set; }
        public CharacterClass LimitClass { get; set; }
        public int PreQuest { get; set; }
        public QuestType Type { get; set; }
        public int AcceptNpc { get; set; }
        public int SubmitNpc { get; set; }
        public string Overview { get; set; }

        public string Dialog { get; set; }
        public string DialogAccept { get; set; }
        public string DialogDeny { get; set; }
        public string DialogIncomplete { get; set; }
        public string DialogFinish { get; set; }
        public QuestTarget Target1 { get; set; }
        public int Target1ID { get; set; }
        public int Target1Num { get; set; }

        public QuestTarget Target2 { get; set; }
        public int Target2ID { get; set; }
        public int Target2Num { get; set; }
        public QuestTarget Target3 { get; set; }
        public int Target3ID { get; set; }
        public int Target3Num { get; set; }
        public int RewardGold { get; set; }
        public int RewardExp{ get; set; }
        public int RewardItem1 { get; set; }
        public int RewardItem1Count{ get; set; }
        public int RewardItem2 { get; set; }
        public int RewardItem2Count { get; set; }

        public int RewardItem3 { get; set; }
        public int RewardItem3Count { get; set; }
    }
}
