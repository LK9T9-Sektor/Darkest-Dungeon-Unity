using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Guarded status (target side) — which unit guards this target.</summary>
    public class GuardedStatusEffect : StatusEffect, IGuardedStatusEffect
    {
        /// <inheritdoc/>
        public override StatusType Type { get { return StatusType.Guarded; } }

        /// <inheritdoc/>
        public override bool IsApplied { get { return Guard != null && GuardDuration > 0; } }

        /// <inheritdoc/>
        public int GuardDuration { get; set; }

        /// <inheritdoc/>
        public ICombatUnit Guard { get; set; }

        /// <inheritdoc/>
        public override void UpdateNextTurn()
        {
        }

        /// <inheritdoc/>
        public override void ResetStatus()
        {
            GuardDuration = 0;
            if (Guard == null)
                return;

            var removingGuard = (IGuardStatusEffect)Guard.Character.GetStatusEffect(StatusType.Guard);
            for (int i = removingGuard.Targets.Count - 1; i >= 0; i--)
            {
                var guardTarget = (IGuardedStatusEffect)removingGuard.Targets[i].Character.GetStatusEffect(StatusType.Guarded);
                if (guardTarget.Guard == Guard)
                {
                    guardTarget.Guard = null;
                    guardTarget.GuardDuration = 0;
                    removingGuard.Targets.RemoveAt(i);
                }
            }

            Guard = null;
        }
    }
}