using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkillBridge.Message;

namespace Common.Data
{
    public class TeleporterDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int MapID { get; set;}
        public int LinkTo { get; set; }
        public string Descript { get; set; }
        public NVector3 Position { get; set; }
        public NVector3 Direction { get; set; }
    }
}
