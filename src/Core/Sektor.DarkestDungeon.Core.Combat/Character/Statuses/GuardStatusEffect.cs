using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Guard status (performer side) — the list of guarded targets.</summary>
    public class GuardStatusEffect : StatusEffect, IGuardStatusEffect
    {
        /// <inheritdoc/>
        public override StatusType Type { get { return StatusType.Guard; } }

        /// <inheritdoc/>
        public override bool IsApplied { get { return Targets.Count > 0; } }

        /// <inheritdoc/>
        public List<ICombatUnit> Targets { get; }

        /// <summary>Initializes a new instance of the <see cref="GuardStatusEffect"/> class.</summary>
        public GuardStatusEffect()
        {
            Targets = new List<ICombatUnit>();
        }

        /// <inheritdoc/>
        public override void UpdateNextTurn()
        {
            for (int i = Targets.Count - 1; i >= 0; i--)
            {
                var targetStatus = (IGuardedStatusEffect)Targets[i].Character.GetStatusEffect(StatusType.Guarded);
                if (--targetStatus.GuardDuration <= 0)
                {
                    targetStatus.Guard = null;
                    Targets.RemoveAt(i);
                }
            }
        }

        /// <inheritdoc/>
        public override void ResetStatus()
        {
            for (int i = Targets.Count - 1; i >= 0; i--)
            {
                var guardedTarget = (IGuardedStatusEffect)Targets[i].Character.GetStatusEffect(StatusType.Guarded);
                guardedTarget.Guard = null;
                guardedTarget.GuardDuration = 0;
            }
            Targets.Clear();
        }
    }
}