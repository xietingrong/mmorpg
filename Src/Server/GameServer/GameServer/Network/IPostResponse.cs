using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public interface IPostResponser
    {
        void PostProcess(NetMessageResponse message);
    }
}
