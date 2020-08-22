using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class UIGuildMemberItem : ListView.ListViewItem
{
    public NGuildMemberInfo Info;
    public void SetGuildMemberInfo(NGuildMemberInfo info)
    {
        Info = info;
    }
}

