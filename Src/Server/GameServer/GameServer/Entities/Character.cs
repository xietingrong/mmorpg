using Common;
using Common.Battle;
using Common.Data;
using Common.Utils;
using GameServer.AI;
using GameServer.Battle;
using GameServer.Core;
using GameServer.Managers;
using GameServer.Models;
using log4net.Core;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Entities
{
    /// <summary>
    /// Character
    /// 玩家角色类
    /// </summary>
    class Character : Creature,IPostResponser
    {
        public TCharacter Data;


        public ItemManager ItemManager;
        public QuestManager QuestManager;
        public StatusManager StatusManager;
        public FriendManager FriendManager;
        public PetManager PetManager;
        public Team Team;
        public double TeamUpdateTS;

        public Guild Guild;
        public double GuildUpdateTS;

        public Chat Chat;

        public Character(CharacterType type,TCharacter cha):
            base(type, cha.TID, cha.Level,new Core.Vector3Int(cha.MapPosX, cha.MapPosY, cha.MapPosZ),new Core.Vector3Int(100,0,0))
        {
            this.AI = new AIAgent(this);
            this.Data = cha;
            this.Id = cha.ID;
            this.Info.Id = cha.ID;
            this.Info.Exp = cha.Exp;
            this.Info.Class = (CharacterClass)cha.Class;
            this.Info.mapId = cha.MapID;
            this.Info.Gold = cha.Gold;
            this.Info.Ride = 0;
            this.Info.Name = cha.Name;

            this.ItemManager = new ItemManager(this);
            this.ItemManager.GetItemInfos(this.Info.Items);
            this.Info.Bag = new NBagInfo();
            this.Info.Bag.Unlocked = this.Data.Bag.Unlocked;
            this.Info.Bag.Items = this.Data.Bag.Items;
            this.Info.Equips = this.Data.Equips;
            this.QuestManager = new QuestManager(this);
            this.QuestManager.GetQuestInfos(this.Info.Quests);
            this.StatusManager = new StatusManager(this);
            this.FriendManager = new FriendManager(this);
            this.FriendManager.GetFriendInfos(this.Info.Friends);
            this.PetManager = new PetManager(this);
            
            this.Guild = GuildManager.Instance.GetGuild(this.Data.GuildId);

            this.Chat = new Chat(this);
            this.Info.attrDynamic = new NAttributeDynamic();
            this.Info.attrDynamic.Hp = cha.HP;
            this.Info.attrDynamic.Mp = cha.MP;
        }

        public long Gold
        {
            get { return this.Data.Gold; }
            set
            {
                if (this.Data.Gold == value)
                    return;
                 this.StatusManager.AddGoldChange((int)(value - this.Data.Gold));
                this.Data.Gold = value;
             }
         }

        public int Ride
        {
            get { return this.Info.Ride; }
            set
            {
                if (this.Info.Ride == value)
                    return;
                this.Info.Ride = value;
            }
        }
        public  long Exp
        {
            get { return this.Info.Exp; }
            set
            {
                if (this.Info.Exp == value)
                    return;
                this.StatusManager.AddExpChange((int)(value - this.Data.Exp));
                this.Data.Exp = value;
            }
        }
        public int Level
        {
            get { return this.Info.Level; }
            set
            {
                if (this.Info.Level == value)
                    return;
                this.StatusManager.AddLevelUp((int)(value - this.Data.Level));
                this.Data.Level = value;
            }
        }

        public void AddExp(int exp)
        {
            this.Exp += exp;
            this.CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            //经验公式：EXP =Power(lv,3)*10+lv*40+50;
            long needExp = (long)Math.Pow(this.Level, 3) * 10 + this.Level * 40 + 50;
            if(this.Exp >needExp)
            {
                this.LevelUp();
            }
        }

        private void LevelUp()
        {
            this.Level += 1;
            Log.InfoFormat("Charcter[{0}:{1}] levelUp:{2}", this.Id, this.Info.Name, this.Level);
            CheckLevelUp();
        }

        public void PostProcess(NetMessageResponse message)
        {
           // Log.InfoFormat("PostProcess > Character: characterID:{0}:{1}", this.Id, this.Info.Name);
            this.FriendManager.PostProcess(message);

            if (this.Team != null)
            {
                Log.InfoFormat("PostProcess > Team: characterID:{0}:{1}  {2}<{3}", this.Id, this.Info.Name, TeamUpdateTS, this.Team.timestamp);
                if (TeamUpdateTS < this.Team.timestamp)
                {
                    TeamUpdateTS = Team.timestamp;
                    this.Team.PostProcess(message);
                }
            }

            if (this.Guild != null)
            {
                Log.InfoFormat("PostProcess > Guild: characterID:{0}:{1}  {2}<{3}", this.Id, this.Info.Name, GuildUpdateTS, this.Guild.timestamp);
                if (this.Info.Guild == null)
                {
                    this.Info.Guild = this.Guild.GuildInfo(this);
                    if (message.mapCharacterEnter != null)
                        GuildUpdateTS = Guild.timestamp;
                }
                if (GuildUpdateTS < this.Guild.timestamp && message.mapCharacterEnter == null)
                {
                    GuildUpdateTS = Guild.timestamp;
                    this.Guild.PostProcess(this, message);
                }
            }

            if (this.StatusManager.HasStatus)
            {
                this.StatusManager.PostProcess(message);
            }

            this.Chat.PostProcess(message);
        }

        /// <summary>
        /// 角色离开时调用
        /// </summary>
        public void Clear()
        {
            this.PetManager.RemovePet() ;
            this.FriendManager.OfflineNotify();
        }

        public NCharacterInfo GetBasicInfo()
        {
            return new NCharacterInfo()
            {
                Id = this.Id,
                Name = this.Info.Name,
                Class = this.Info.Class,
                Level = this.Info.Level
            };
        }
        public override void Update()
        {
            base.Update();

            UpdateMovement();

            foreach (var item in PetManager.Monsters)
            {
                item.Update();
            }
            if (this.BattleState == Common.Battle.CharState.InBattle)
            {
                IsAiUse = true;
            }

        }
        private Vector3 movepPostion;
        public override void UpdateMovement()
        {
           
        }

        protected override void OnDamage(NDamageInfo damage, Creature source)
        {
            if (this.AI != null)
            {
                this.AI.OnDamage(damage, source);
            }
        }

        public override Skill FindSkill(BattleContext context, SkillType type)
        {
            Skill cancast = null;
            foreach (var skill in this.SkillMgr.skills)
            {
                if ((skill.Define.Type & type) != skill.Define.Type)
                    continue;

                var result = skill.CanCast(context);
                if (result == SkillResult.Casting)
                    return null;
                if (result == SkillResult.Ok)
                {
                    cancast = skill;
                }
            }
            return cancast;
        }
    }
}
