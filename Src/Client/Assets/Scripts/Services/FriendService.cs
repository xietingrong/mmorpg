using Managers;
using Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Services
{
    class FriendService : Singleton<FriendService>, IDisposable
    {
        public UnityAction OnFriendUpdate;
        public void Init()
        {

        }

        public FriendService()
        { 
            MessageDistributer.Instance.Subscribe<FriendAddRequest>(this.OnFriendAddRequest);
            MessageDistributer.Instance.Subscribe<FriendAddResponse>(this.OnFriendAddResponse);
            MessageDistributer.Instance.Subscribe<FriendListResponse>(this.OnFriendList);
            MessageDistributer.Instance.Subscribe<FriendRemoveResponse>(this.OnFriendRemove);
        }

        private void OnFriendRemove(object sender, FriendRemoveResponse message)
        {
            if (message.Result == Result.Success)
                MessageBox.Show("删除成功", "删除好友");
            else
                MessageBox.Show("删除失败", "删除好友",MessageBoxType.Error);
        }
        public void SendFriendRemoveResponse(int id,int friendId)
        {
            Debug.Log("SSendFriendRemoveResponse");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.friendRemove = new FriendRemoveRequest();
            message.Request.friendRemove.Id = id;
            message.Request.friendRemove.friendId = friendId;

            NetClient.Instance.SendMessage(message);
        }
        private void OnFriendList(object sender, FriendListResponse message)
        {
            Debug.Log("OnFriendList");
            FriendManager.Instance.allFriends = message.Friends;
            if (this.OnFriendUpdate != null)
                this.OnFriendUpdate();
        }


        private void OnFriendAddResponse(object sender, FriendAddResponse message)
        {
            if (message.Result == Result.Success)
                MessageBox.Show(message.Request.ToName+"接受了你的请求","添加好友成功");
            else
                MessageBox.Show(message.Errormsg, "添加好友失败");
        }
        public void SendFriendAddResponse(bool accept,FriendAddRequest request)
        {
            Debug.Log("SendFriendAddRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.friendAddRes = new FriendAddResponse();
            message.Request.friendAddRes.Result = accept ? Result.Success : Result.Failed;
            message.Request.friendAddRes.Errormsg = accept ? "对方同意" : "对方拒绝了你的请求";
            message.Request.friendAddRes.Request = request;
            NetClient.Instance.SendMessage(message);
        }


        private void OnFriendAddRequest(object sender, FriendAddRequest request)
        {
            var confirn = MessageBox.Show(String.Format("{0}请求加你为好友", request.FromName), "好友请求", MessageBoxType.Confirm, "接受", "拒绝");
            confirn.OnYes = () =>
            {
                this.SendFriendAddResponse(true, request);
            };
            confirn.OnNo = () =>
            {
                this.SendFriendAddResponse(false, request);
            };
        }
        public void SendFriendAddRequest(int friendId,string friendName)
        {
            Debug.Log("SendFriendAddRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.friendAddReq = new FriendAddRequest();
            message.Request.friendAddReq.FromId = User.Instance.CurrentCharacterInfo.Id;
            message.Request.friendAddReq.FromName = User.Instance.CurrentCharacterInfo.Name;
            message.Request.friendAddReq.ToId = friendId;
            message.Request.friendAddReq.ToName = friendName;
            NetClient.Instance.SendMessage(message);
        }

        public void Dispose()
        {

            MessageDistributer.Instance.Unsubscribe<FriendAddRequest>(this.OnFriendAddRequest);
            MessageDistributer.Instance.Unsubscribe<FriendAddResponse>(this.OnFriendAddResponse);
            MessageDistributer.Instance.Unsubscribe<FriendListResponse>(this.OnFriendList);
            MessageDistributer.Instance.Unsubscribe<FriendRemoveResponse>(this.OnFriendRemove);
        }
    }
}
