using Common.Data;
using Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

public class UIStory:UIWindow
{
    public Text title;
    public Text descript;
    StoryDefine stroy;
     void Start()
    {
        
    }
    public void SetStory(StoryDefine stroy)
    {
        this.stroy = stroy;
        this.title.text = stroy.Name;
        this.descript.text = stroy.Description;
    }
    public void OnClickStart()
    {
        if(!StoryManager.Instance.StartStory(this.stroy.ID))
        {

        }
    }
}

