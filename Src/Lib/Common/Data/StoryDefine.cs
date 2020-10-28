using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Data
{
    public class StoryDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SubType { get; set; }
        public int Mapid { get; set; }
        public int LimitTime { get; set; }
        public int PreQuest { get; set; }
        public int Quest { get; set; }
      
    }
}
