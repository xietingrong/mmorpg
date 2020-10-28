using Managers;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Services
{
    class StoryService : Singleton<StoryService>, IDisposable
    {
        public StoryService()
        {
            MessageDistributer.Instance.Subscribe<StoryStartResponse>(this.OnStoryStart);
            MessageDistributer.Instance.Subscribe<StoryEndResponse>(this.OnStoryEnd);
        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<StoryStartResponse>(this.OnStoryStart);
            MessageDistributer.Instance.Unsubscribe<StoryEndResponse>(this.OnStoryEnd);
        }
        private void OnStoryStart(object sender, StoryStartResponse message)
        {
            Debug.LogFormat("OnStoryStart: stroyID:{0}", message.storyId);
            StoryManager.Instance.OnStoryStart(message.storyId);
        }

        public void SendStartStory(int storyId)
        {
            Debug.LogFormat("SendStartStory: stroyID:{0}", storyId);

            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.storyStart= new StoryStartRequest();
            message.Request.storyStart.storyId = storyId;
            NetClient.Instance.SendMessage(message);
        }
        private void OnStoryEnd(object sender, StoryEndResponse message)
        {
            Debug.LogFormat("OnStoryEnd: stroyID:{0}", message.storyId);
            if(message.Result ==Result.Success)
            {

            }
        }

        public void Init()
        {

        }
   
    }
}
