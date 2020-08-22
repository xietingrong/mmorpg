using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class UIGuildItem : ListView.ListViewItem
{

    public NGuildInfo Info;
    public void SetGuildInfo(NGuildInfo item)
    {
        Info = item;
    }
}

