using System;
using Network;
using UnityEngine;

using Common.Data;
using SkillBridge.Message;
using Models;
using Managers;
using Entities;

namespace Services
{
    class MapService : Singleton<MapService>, IDisposable
    {

        public int CurrentMapId = 0;
        public MapService()
        {
            MessageDistributer.Instance.Subscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);
            MessageDistributer.Instance.Subscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);

            MessageDistributer.Instance.Subscribe<MapEntitySyncResponse>(this.OnMapEntitySync);
            SceneManager.Instance.onSceneLoadDone += OnLoadDone;
        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);
            MessageDistributer.Instance.Unsubscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);
            SceneManager.Instance.onSceneLoadDone -= OnLoadDone;
        }

        public void Init()
        {

        }

        private void OnMapCharacterEnter(object sender, MapCharacterEnterResponse response)
        {
            Debug.LogFormat("OnMapCharacterEnter:Map:{0} Count:{1}", response.mapId, response.Characters.Count);
            foreach (var cha in response.Characters)
            {
                if (User.Instance.CurrentCharacterInfo == null || (cha.Type == CharacterType.Player && User.Instance.CurrentCharacterInfo.Id == cha.Id))
                {//当前角色切换地图
                    User.Instance.CurrentCharacterInfo = cha;
                    if(User.Instance.CurrentCharacter  == null)
                    {
                        User.Instance.CurrentCharacter = new Character(cha);
                        
                    }
                    else
                    {
                        User.Instance.CurrentCharacter.UpdateInfo(cha);
                    }
                    User.Instance.characterInited();
                    if (User.Instance.CurrentCharacter != null)
                        User.Instance.CurrentCharacter.ready = false;//---
                    CharacterManager.Instance.AddCharacter(User.Instance.CurrentCharacter);
                    if (CurrentMapId != response.mapId)
                    {
                        this.EnterMap(response.mapId);
                        this.CurrentMapId = response.mapId;
                    }
                    continue;
                }
                CharacterManager.Instance.AddCharacter( new Character(cha));
            }
           
        }

        private void OnMapCharacterLeave(object sender, MapCharacterLeaveResponse response)
        {
            Debug.LogFormat("OnMapCharacterLeave: CharID:{0}", response.entityId);
            if(response.entityId != User.Instance.CurrentCharacterInfo.EntityId)
                CharacterManager.Instance.RemoveCharacter(response.entityId);
            else
            {
                if (User.Instance.CurrentCharacterObject != null)//---
                {
                    User.Instance.CurrentCharacterObject.OnLeaveLevel();
                }
                CharacterManager.Instance.Clear();
               
            }
                
        }


        public void EnterMap(int mapId)
        {
            if (DataManager.Instance.Maps.ContainsKey(mapId))
            {
                loadingDone = false;

                MapDefine map = DataManager.Instance.Maps[mapId];
                User.Instance.CurrentMapData = map;
                SceneManager.Instance.LoadScene(map.Resource);
                SoundManager.Instance.PlayMusic(map.Music);
            }
            else
                Debug.LogErrorFormat("EnterMap: Map {0} not existed", mapId);
        }

        private bool loadingDone = true;
        private void OnLoadDone()//-----
        {
            if (User.Instance.CurrentCharacter != null)
                User.Instance.CurrentCharacter.ready = true;
            if (User.Instance.CurrentCharacterObject != null)//---
            {
                User.Instance.CurrentCharacterObject.OnEnterLevel();
            }
            loadingDone = true;


        }

        public void SendMapEntitySync(EntityEvent entityEvent, NEntity entity,int param)
        {
            Debug.LogFormat("MapEntityUpdateRequest :ID:{0} POS:{1} DIR:{2} SPD:{3} ", entity.Id, entity.Position.String(), entity.Direction.String(), entity.Speed);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.mapEntitySync = new MapEntitySyncRequest();
            message.Request.mapEntitySync.entitySync = new NEntitySync()
            {
                Id = entity.Id,
                Event = entityEvent,
                Entity = entity,
                Param = param
            };
            NetClient.Instance.SendMessage(message);
        }

        void OnMapEntitySync(object sender, MapEntitySyncResponse response)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("MapEntityUpdateResponse: Entitys:{0}", response.entitySyncs.Count);
            sb.AppendLine();
            foreach (var entity in response.entitySyncs)
            {
                EntityManager.Instance.OnEntitySync(entity);
                sb.AppendFormat("    [{0}]evt:{1} entity:{2}", entity.Id, entity.Event, entity.Entity.String());
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }


        public void SendMapTeleport(int teleporterID)
        {
            Debug.LogFormat("MapTeleportRequest :teleporterID:{0}", teleporterID);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.mapTeleport = new MapTeleportRequest();
            message.Request.mapTeleport.teleporterId = teleporterID;
            NetClient.Instance.SendMessage(message);
        }
    }
}
