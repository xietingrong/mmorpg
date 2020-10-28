using Common;
using Common.Battle;
using GameServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    class EffectManager
    {
        private Creature Owner;
        private Dictionary<BuffEffect, int> Effects = new Dictionary<BuffEffect, int>();
        public EffectManager(Creature owner)
        {
            this.Owner = owner;
        }
        internal bool HasEffect(BuffEffect effect)
        {
            if(this.Effects.TryGetValue(effect,out int val))
            {
                return val > 1;
            }
            return false;
        }
        internal void AddBuffEffect(BuffEffect effect)
        {
            Log.InfoFormat("[{0}].AddBuffEffect", this.Owner.Name, effect);
            if(!this.Effects.ContainsKey(effect))
            {
                this.Effects[effect] = 1;
            }
            else
            {
                this.Effects[effect]++ ;
            }
           
        }

        internal void RemoveEffect(BuffEffect effect)
        {
            Log.InfoFormat("[{0}].RemoveEffect", this.Owner.Name, effect);
            if(this.Effects[effect] >0)
            {
                this.Effects[effect]--;
            }
        }
    }
}
