using Common.Data;
using GameServer.Core;
using GameServer.Entities;
using GameServer.Managers;
using GameServer.Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    class Battle
    {
        public Map Map;
        Dictionary<int, Creature> AllUnits = new Dictionary<int, Creature>();
        Queue<NSkillCastInfo> Actions = new Queue<NSkillCastInfo>();
        List<Creature> DeahPool = new List<Creature>();

        List<NSkillCastInfo> CastSkills = new List<NSkillCastInfo>();
        List<NSkillHitInfo> Hits = new List<NSkillHitInfo>();

        List<NBuffInfo> BuffActions= new List<NBuffInfo>();
        public Battle(Map map)
        {
            this.Map = map;
        }
        internal void ProcessBattleMessage(NetConnection<NetSession> sender, SkillCastRequest request)
        {
            Character character = sender.Session.Character;
            if(request.castInfo!= null)
            {
                if (character.entityId != request.castInfo.casterId)
                    return;
                this.Actions.Enqueue(request.castInfo);
            }
        }
        internal void Update()
        {
            this.CastSkills.Clear();
            this.Hits.Clear();
            this.BuffActions.Clear();

            if (this.Actions.Count > 0)
            {
                NSkillCastInfo skillCast = this.Actions.Dequeue();
                this.ExecuteAction(skillCast);
            }
            PetUpdate();
            this.UpdateUnits();

            this.BroadcastHitsMessage();

        }
        private void PetUpdate()
        {
            if (this.Map.Define.Type != MapType.Arena)
            { 
                foreach (var value in this.Map.MapCharacters)
                {
                    value.Value.character.Update();
                }
            }
        }

        private void BroadcastHitsMessage()
        {
            if (this.Hits.Count == 0 && this.BuffActions.Count == 0 && this.CastSkills.Count ==0) return;
            NetMessageResponse message = new NetMessageResponse();
            if (this.CastSkills.Count > 0)
            {
                message.skillCast = new SkillCastResponse();
                message.skillCast.castInfoes.AddRange(this.CastSkills);
                message.skillCast.Result = Result.Success;
                message.skillCast.Errormsg = "";
            }
            if (this.Hits.Count >0)
            {
                message.skillHits = new SkillHitResponse();
                message.skillHits.Hits.AddRange(this.Hits);
                message.skillHits.Result = Result.Success;
                message.skillHits.Errormsg = "";
            }
            if (this.BuffActions.Count > 0)
            {
                message.buffRes = new BuffResponse();
                message.buffRes.Buffs.AddRange(this.BuffActions);
                message.buffRes.Result = Result.Success;
                message.buffRes.Errormsg = "";
            }
            this.Map.BrodacastBattleResponse(message);
        }
        public List<Creature> AllArea = new List<Creature>();
        void UpdateCanAttk()
        {
            foreach (var value in AllArea)
            {
                value.Update();

            }
            //List<Creature> list = new List<Creature>();
            //foreach (var value in AllArea)
            //{
            //    if (!value.IsAttOver)
            //    {
            //        list.Add(value);
            //    }
            //}
            //List<Creature> list2 = new List<Creature>();
            //foreach (var value in list)
            //{
            //    if (value.IsAtt)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        list2.Add(value);
            //    }
            //}
            //list2.Sort((a, b) =>
            //{
            //    return a.Attributes.SPD > b.Attributes.SPD ? 1 : -1;
            //});

            //if (list2.Count < 1)
            // //重置战斗
            //{
            //    foreach (var value in AllArea)
            //    {
            //        value.IsAttOver = false;
            //        value.IsAtt = false;
            //    }
            //}
           
        }
        public List<Creature> UpdateCanAttacKlist(Creature owner)
        {
            List<Creature> list = new List<Creature>();
            foreach (var value in AllArea)
            {
                if (value.ZhengYing != owner.ZhengYing)
                {
                    list.Add(value);
                }
            }
            return list;
        }
        private void UpdateUnits()
        {
            this.DeahPool.Clear();
            if( this.Map.Define.Type == MapType.Arena)
            {


                UpdateCanAttk();

                foreach (var kv in this.AllUnits)
                {
                    if (kv.Value.isDeath)
                        this.DeahPool.Add(kv.Value);
                }
            }
            else 
            {
                foreach (var kv in this.AllUnits)//大地图战斗逻辑
                {
                    kv.Value.Update();
                    if (kv.Value.isDeath)
                        this.DeahPool.Add(kv.Value);
                }
            }
           
            foreach(var unit in this.DeahPool)
            {
                this.LeaveBattle(unit);
            }
        }
        
        private void ExecuteAction(NSkillCastInfo cast)
        {
            BattleContext context = new BattleContext(this);
            context.Caster = EntityManager.Instance.GetCreature(cast.casterId);
            context.Target = EntityManager.Instance.GetCreature(cast.targetId);
            context.CastSkill = cast;
            // context.Position = cast.Postion;

            if (context.Caster != null)
                this.JoinBattle(context.Caster);
            if (context.Target != null)
                this.JoinBattle(context.Target);

            context.Caster.CastSkill(context, cast.skillId);//执行到帧update执行发送消息

        }
        public void JoinBattle(Creature unit)
        {
            this.AllUnits[unit.entityId] = unit;

            if (this.Map.Define.Type == MapType.Arena)
                AllArea.Add(unit);
        }
        public void LeaveBattle(Creature unit)
        {         
             AllArea.Remove(this.AllUnits[unit.entityId]);
            this.AllUnits.Remove(unit.entityId);
        }
       

        internal List<Creature> FindUnitsInRange(Vector3Int pos, float range)
        {
            List<Creature> result = new List<Creature>();
            foreach (var unit in this.AllUnits)
            {
                var distance = unit.Value.Distance(pos);
                if (distance < range)
                {
                    result.Add(unit.Value);
                }
            }
            return result;
        }
        internal List<Creature> FindUnitsInMapRange(Vector3Int pos, float range)
        {
           
            return EntityManager.Instance.GetMapEnititiesInRange<Creature>(this.Map.GetMapIndex(),pos,range);
        }
        public void AddCastSkillInfo(NSkillCastInfo cast)
        {
            this.CastSkills.Add(cast);
        }
        public void AddHitInfo(NSkillHitInfo hit)
        {
            this.Hits.Add(hit);
        }
        internal void AddBuffAction(NBuffInfo buff)
        {
            this.BuffActions.Add(buff);
        }
      
    }

   
}
