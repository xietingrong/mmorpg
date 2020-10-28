using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkillBridge.Message;

namespace Common.Data
{
    public enum NpcType
    {
        None = 0,
        Functional = 1,
        Task = 2,
    }
    public enum NpcFunction
    {
        None = 0,
        InvokeShop =1,
        InvokeInsrance =2,
        InvokeStroy = 3,
    }
    public class NpcDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public NVector3 Postion { get; set;}
        public string Descript { get; set; }
        public NpcType Type { get; set; }
        public NpcFunction Function { get; set; }
        public int Param { get; set; }
    }
}
