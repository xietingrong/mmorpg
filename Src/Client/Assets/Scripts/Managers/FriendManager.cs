using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managers
{
    class FriendManager : Singleton<FriendManager>
    {
        public List<NFriendInfo> allFriends;
        public void Init(List<NFriendInfo> friends)
        {
            this.allFriends = friends;
        }
    }
}
