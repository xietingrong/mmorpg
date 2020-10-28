using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Data;
using Services;

namespace Managers
{
    class StoryManager : Singleton<StoryManager>
    {
        public void Init()
        {
            NpcManager.Instance.RegisterNpcEvent(Common.Data.NpcFunction.InvokeInsrance, OnOpenStory);
        }

        private bool OnOpenStory(NpcDefine npc)
        {
            this.ShowStoyUI(npc.Param);
            return true;
        }

        private void ShowStoyUI(int storyId)
        {
            StoryDefine story;
            if(DataManager.Instance.Storys.TryGetValue(storyId, out story))
            {
                UIStory uiStory = UIManager.Instance.Show<UIStory>();
                if(uiStory!= null)
                {
                    uiStory.SetStory(story);
                }
            }
        }
        public bool StartStory(int storyId)
        {
            StoryService.Instance.SendStartStory(storyId);
            return true;
        }

        internal void OnStoryStart(int storyId)
        {
           
        }
    }
}
