using Common.Data;
using GameServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    class BuffManager
    {
        private Creature Owner;
        private List<Buff> Buffs = new List<Buff>();
        private int idx = 1;
        private int BuffId
        {
            get
            {
                return idx++;
            }
        }
        public BuffManager(Creature owner)
        {
            this.Owner = owner;
        }
        internal void AddBuff(BattleContext context, BuffDefine define)
        {
            Buff buff = new Buff(this.BuffId,this.Owner,define,context);
            this.Buffs.Add(buff);
        }

        internal void Update()
        {
            for (int i = 0; i < this.Buffs.Count; i++)
            {
                if(!this.Buffs[i].Stoped)
                    this.Buffs[i].Update();
            }
            this.Buffs.RemoveAll((b) => b.Stoped);
        }
    }
}
