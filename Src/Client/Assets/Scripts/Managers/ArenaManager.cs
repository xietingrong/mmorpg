using Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Managers
{
    class ArenaManager : Singleton<ArenaManager>
    {
        public int Round;
        private ArenaInfo   arenaInfo;
        public void EnterArena(ArenaInfo arenaInfo)
        {
            Debug.LogFormat("ArenaManager.EnterArena :{0}", arenaInfo.ArenaId);

            this.arenaInfo = arenaInfo;
        }
        public void ExitArena(ArenaInfo arenaInfo)
        {
            Debug.LogFormat("ArenaManager.ExitArena :{0}", arenaInfo.ArenaId);

           this.arenaInfo = null;
        }
       
        internal void SendReady()
        {
            Debug.LogFormat("ArenaManager.SendReady :{0}", arenaInfo.ArenaId);
            ArenaService.Instance.SendArenaReadyRequest(this.arenaInfo.ArenaId);
        }
        internal void OnReady(int round, ArenaInfo arenaInfo)
        {
            Debug.LogFormat("ArenaManager.SendRea :{0}", arenaInfo.ArenaId);
            this.Round = round;
            if(UIArenacs.Instance!= null)
            {
                UIArenacs.Instance.ShowCountDown();
            }
        }
        internal void OnRoundStart(int round,ArenaInfo arena)
        {
            Debug.LogFormat("ArenaManager.OnRoundStart :{0} round: {1}", arenaInfo.ArenaId,round);
            if (UIArenacs.Instance != null)
            {
                UIArenacs.Instance.ShowRoundStart(round, arena);
            }
        }
        internal void OnRoundEnd(int round, ArenaInfo arena)
        {
            Debug.LogFormat("ArenaManager.OnRoundEnd :{0} round: {1}", arenaInfo.ArenaId, round);
            if (UIArenacs.Instance != null)
            {
                UIArenacs.Instance.ShowRoundResult(round, arena);
            }
        }
    }
}
