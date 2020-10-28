using Common.Data;
using GameServer.Managers;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    class Story
    {
        internal int StoryId;
        internal int InstanceId;
        public Map map;
      
        private NetConnection<NetSession> Player;
        Map SourceMapRed;
        int startPoint = 12;
        public int Round { get ; internal set; }
        private float timer = 0f;

        public Story(Map map, int storyId, int instance, NetConnection<NetSession> owner)
        {
            this.map = map;
            StoryId = storyId;
            //this.InstanceId = instance;
            this.Player = owner;
        }

        public static int Key { get; internal set; }

       

        internal void PlayerEnter()
        {
            this.SourceMapRed = PlayerLeaveMap(this.Player);
            this.PlayerEnterArena();
        }

        private void PlayerEnterArena()
        {
            TeleporterDefine starPoint = DataManager.Instance.Teleporters[this.startPoint];
            this.Player.Session.Character.Position = starPoint.Position;
            this.Player.Session.Character.Direction = starPoint.Direction;
            this.map.AddCharacter(this.Player, this.Player.Session.Character);
            this.map.CharacterEnter(this.Player, this.Player.Session.Character);
            EntityManager.Instance.AddMapEntity(this.map.ID, this.map.InstanceId, this.Player.Session.Character);
        }

        private Map PlayerLeaveMap(NetConnection<NetSession> player)
        {
            var currentMap = MapManager.Instance[player.Session.Character.Info.mapId];
            currentMap.CharacterLeave(player.Session.Character);
            EntityManager.Instance.RemoveEntity(currentMap.ID, currentMap.InstanceId, player.Session.Character);
            return currentMap;
        }
        internal void Update()
        {

        }
        internal void End()
        {

        }
    }
}
