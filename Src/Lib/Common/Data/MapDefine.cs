using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Common.Data
{
    public class MapDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Resource { get; set; }
        public string Music { get; set; }
        public string MiniMap { get; set; }
        public MapType Type { get; set; }
    }
    public enum MapType
    {
        Normal,
        Arena,
        Story,
       
    }
}
