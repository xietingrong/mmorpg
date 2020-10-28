using Common;
using Common.Data;
using GameServer.Core;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Vector3Int = GameServer.Core.Vector3Int;

namespace GameServer.Managers
{

    class EntityManager : Singleton<EntityManager>
    {
        private int idx =0;
        //public List<Entity> AllEntities = new List<Entity>();
        public Dictionary<int, Entity> AllEntities = new Dictionary<int, Entity>();
        public Dictionary<int, List<Entity>> MapEntities = new Dictionary<int, List<Entity>>();

        public int GetMapIndex(int mapId, int instanceId)
        {
            return mapId * 1000 + instanceId;
        }
        public void AddEntity(int mapId, int instanceID, Entity entity)
        {
            //加入管理器生成唯一id
            entity.EntityData.Id = ++idx;
            AllEntities.Add(entity.EntityData.Id, entity);

            AddMapEntity(mapId, instanceID, entity);
        }

        public void AddMapEntity(int mapId, int instanceID, Entity entity)
        {
            List<Entity> entities = null;
            int index = this.GetMapIndex(mapId, instanceID);
            if (!MapEntities.TryGetValue(index, out entities))
            {
                entities = new List<Entity>();
                MapEntities[index] = entities;
            }
            entities.Add(entity);
        }

        public void RemoveEntity(int mapId, int instanceID, Entity entity)
        {
            this.AllEntities.Remove(entity.entityId);
            this.RemoveMapEntity(mapId, instanceID, entity);
        }
        internal void RemoveMapEntity(int mapId, int instanceID, Entity entity)
        {
            int index = this.GetMapIndex(mapId, instanceID);
            this.MapEntities[index].Remove(entity);
        }
        public Entity GetEntity(int entityId)
        {
            Entity result = null;
            this.AllEntities.TryGetValue(entityId, out result);
            return result;
        }
        internal Creature GetCreature(int entityId)
        {
            return GetEntity(entityId) as Creature;
        }
        public List<T> GetMapEnitities<T>(int mapId,Predicate<Entity> match) where T:Creature
        {
            List<T> result = new List<T>();
            foreach (var entity in this.MapEntities[mapId])
            {
                if (entity is T && match.Invoke(entity))
                    result.Add((T)entity);
            }
            return result;
        }
        public List<T> GetMapEnititiesInRange<T>(int mapId, Vector3Int pos,float range) where T : Creature
        {
            return this.GetMapEnitities<T>(mapId, (entity) =>
             {
                 T Creature = entity as T;
                 return Creature.Distance(pos) < range;
             });
        }
    }
}
