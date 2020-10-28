using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Battle;
using Entities;
using UnityEngine;

namespace Battle
{
    public class EffectManager
    {
        private Creature Owner;
        private Dictionary<BuffEffect, int> Effects = new Dictionary<BuffEffect, int>();
        public EffectManager(Creature owner)
        {
            this.Owner = owner;
        }
        internal bool HasEffect(BuffEffect effect)
        {
            int val;
            if (this.Effects.TryGetValue(effect, out  val))
            {
                return val > 1;
            }
            return false;
        }
        internal void AddBuffEffect(BuffEffect effect)
        {
            Debug.LogFormat("[{0}].AddBuffEffect", this.Owner.Name, effect);
            if (!this.Effects.ContainsKey(effect))
            {
                this.Effects[effect] = 1;
            }
            else
            {
                this.Effects[effect]++;
            }

        }

        internal void RemoveBuffEffect(BuffEffect effect)
        {
            Debug.LogFormat("[{0}].RemoveEffect", this.Owner.Name, effect);
            if (this.Effects[effect] > 0)
            {
                this.Effects[effect]--;
            }
        }
    }
}
