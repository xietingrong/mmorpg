using Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managers
{
    interface IEntityNotify
    {
        void OnEntityRemoved();
        void OnEntityChanged(Entity entity);
        void OnEntityEvent(EntityEvent @event,int param);
    }
    class EntityManager:Singleton<EntityManager>
    {
     
        public Dictionary<int,Entity> entities = new Dictionary<int,Entity>();

        public Dictionary<int, IEntityNotify> notifiers = new Dictionary<int, IEntityNotify>();
        public void RegisterEntityChangeNotify(int entityId, IEntityNotify entity)
        {
            this.notifiers[entityId] = entity;
        }

        public void AddEntity(Entity entity)
        {
            this.entities[entity.entityId] = entity;
        }
        public void RemoveEntity(NEntity entity)
        {
            this.entities.Remove(entity.Id);
            if(notifiers.ContainsKey(entity.Id))
            {
                notifiers[entity.Id].OnEntityRemoved();
                notifiers.Remove(entity.Id);
            }
        }

        internal void OnEntitySync(NEntitySync data)
        {
            Entity entity = null;
            this.entities.TryGetValue(data.Id, out entity);
            if(entity !=null)
            {
                if (data.Entity != null)
                {
                    entity.EntityData = data.Entity;
                }
                if (notifiers.ContainsKey(data.Id))
                {
                    notifiers[entity.entityId].OnEntityChanged(entity);
                    notifiers[entity.entityId].OnEntityEvent(data.Event, data.Param);
                }

            }

        }

        internal Entity GetEntity(int entityId)
        {
            Entity entity = null;
            entities.TryGetValue(entityId, out entity);
            return entity;
        }
    }
}
