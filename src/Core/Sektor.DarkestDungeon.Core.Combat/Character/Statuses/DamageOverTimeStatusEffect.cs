using System.Collections.Generic;
using System.Linq;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Base of a damage-over-time status effect (bleed/poison).</summary>
    public abstract class DamageOverTimeStatusEffect : StatusEffect, IDotStatusEffect
    {
        /// <inheritdoc/>
        public override bool IsApplied { get { return doTs.Count > 0; } }

        private readonly List<DamageOverTimeInstanse> doTs = new List<DamageOverTimeInstanse>();

        /// <summary>Gets the current combined tick damage.</summary>
        public int CurrentTickDamage { get { return doTs.Count > 0 ? doTs.Sum(dot => dot.TickDamage) : 0; } }

        /// <summary>Gets the combined remaining damage.</summary>
        public int CombinedDamage { get { return doTs.Count > 0 ? doTs.Sum(dot => dot.TicksLeft * dot.TickDamage) : 0; } }

        /// <summary>Gets the expiration time (max remaining ticks).</summary>
        public int ExpirationTime { get { return doTs.Count > 0 ? doTs.Max(dot => dot.TicksLeft) : 0; } }

        /// <inheritdoc/>
        public override void UpdateNextTurn()
        {
            for (int i = doTs.Count - 1; i >= 0; i--)
                if (doTs[i].CheckExpiration())
                    doTs.RemoveAt(i);
        }

        /// <inheritdoc/>
        public override void ResetStatus()
        {
            RemoveDoT();
        }

        /// <summary>Adds a damage-over-time instance.</summary>
        /// <param name="tickDamage">The damage per tick.</param>
        /// <param name="ticks">The number of ticks.</param>
        public void AddInstanse(int tickDamage, int ticks)
        {
            var newDot = new DamageOverTimeInstanse
            {
                TickDamage = tickDamage,
                TicksAmount = ticks,
            };
            newDot.TicksLeft = newDot.TicksAmount;
            doTs.Add(newDot);
        }

        /// <summary>Removes all damage-over-time instances.</summary>
        public void RemoveDoT()
        {
            doTs.Clear();
        }
    }
}