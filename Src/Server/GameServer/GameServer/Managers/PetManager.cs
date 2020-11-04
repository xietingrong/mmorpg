using Common.Data;
using GameServer.Core;
using GameServer.Entities;
using GameServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameServer.Managers
{
    class PetManager
    {
        Character Owner;

        public List<Monster> Monsters = new List<Monster>();
        public PetManager(Character owner)
        {
            this.Owner = owner;
        }
        public void PetInit()
        {
            if (Owner.map.Define.Type != MapType.Arena)
            {
                int index = 0;
                Monsters.Clear();
                foreach (var item in Owner.Data.TPets)
                {
                    Core.Vector3Int position = new Core.Vector3Int();
                    position.x = Owner.Position.x;
                    position.y = Owner.Position.y - 200 - 50 * index;
                    position.z = Owner.Position.z;
                    Core.Vector3Int dir = new Core.Vector3Int();
                    dir.x = Owner.Direction.x;
                    dir.y = Owner.Direction.y;
                    dir.z = Owner.Direction.z;
                    if (index == 0)
                    {
                        Monster monster = Owner.map.MonsterManager.Create(item.Tid, item.Level, position, dir, 0);
                        monster.OnOwner(this.Owner, -200 - 50 * index);
                        Monsters.Add(monster);
                    }
                    index++;

                }
            }

        }
        public void RemovePet()
        {
            foreach(var value in Monsters)
            {
                value.map.MonsterManager.RemoveMonster(value.MonsterId);
            }
        }
    }
}
