using log4net.Appender;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UITeam : MonoBehaviour
{
    public Text teamTitle;
    public UITeamItem[] Members;
    public ListView list;
    private void Start()
    {
        if(User.Instance.TeamInfo ==null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        foreach(var item in Members)
        {
            this.list.AddItem(item);
        }
    }
    private void OnEnable()
    {
        UpdateTeamUI();
    }
    public void ShowTeam(bool show)
    {
        this.gameObject.SetActive(show);
        if(show)
        {
            UpdateTeamUI();
        }
    }
    private void UpdateTeamUI()
    {
        if (User.Instance.TeamInfo == null) return;
        this.teamTitle.text = string.Format("我的队伍({0}/5)", User.Instance.TeamInfo.Members.Count);
        for(var i=0;i < 5;i++)
        {
            if(i< User.Instance.TeamInfo.Members.Count)
            {
                this.Members[i].SetTeamInfo(i, User.Instance.TeamInfo.Members[i], User.Instance.TeamInfo.Members[i].Id == User.Instance.TeamInfo.Leader);
                this.Members[i].gameObject.SetActive(true);
            }
            else
            {
                this.Members[i].gameObject.SetActive(false);
            }
        }
    }
    public void OnClickLevel()
    {
        MessageBox.Show("确定要离开队伍吗", "推出队伍", MessageBoxType.Confirm, "确定离开", "拒绝").OnYes = () =>
        {
            TeamService.Instance.SendTeamLeaveRequest(User.Instance.TeamInfo.Id);
        };
    }
}

