using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkillBridge.Message;

namespace Common.Data
{
    public class SpawnRuleDefine
    {
        public int ID { get; set; }
        public int MapID { get; set; }
        public int SpawnMonID { get; set; }
        public int SpawnLevel{ get;set; }
        public SPAWN_TYPE SpawnType { get; set; }
        public int SpawnPoint { get; set; }
        public int SpawnPoints { get; set; }
        public float SpawnPeriod { get; set; }
    }
}
