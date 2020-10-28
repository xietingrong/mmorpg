using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Battle;
using Common.Data;
using Entities;
using Services;
using SkillBridge.Message;
using UnityEngine;

namespace Models
{
    class User : Singleton<User>
    {
        SkillBridge.Message.NUserInfo userInfo;


        public SkillBridge.Message.NUserInfo Info
        {
            get { return userInfo; }
        }


        public void SetupUserInfo(SkillBridge.Message.NUserInfo info)
        {
            this.userInfo = info;
        }

        public Character CurrentCharacter { get; set; }
        public MapDefine CurrentMapData { get; set; }

        public SkillBridge.Message.NCharacterInfo CurrentCharacterInfo { get; set; }

        public PlayerInputController CurrentCharacterObject { get; set; }

        public NTeamInfo TeamInfo { get; set; }

        public void AddGold(int gold)
        {
            this.CurrentCharacterInfo.Gold += gold;
        }


        public int CurrentRide = 0;
        internal void Ride(int id)
        {
            if (CurrentRide != id)
            {
                CurrentRide = id;
                CurrentCharacterObject.SendEntityEvent(EntityEvent.Ride, CurrentRide);
            }
            else
            {
                CurrentRide = 0;
                CurrentCharacterObject.SendEntityEvent(EntityEvent.Ride, 0);
            }
        }
        public delegate void CharacterInitHandle();
        public event CharacterInitHandle OnCharacterInit;
        internal void characterInited()
        {
            if (OnCharacterInit != null)
                OnCharacterInit();
        }
    }
}
