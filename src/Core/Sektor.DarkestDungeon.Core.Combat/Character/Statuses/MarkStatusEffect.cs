using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Marked (tagged) status.</summary>
    public class MarkStatusEffect : StatusEffect, IMarkStatusEffect
    {
        /// <inheritdoc/>
        public override StatusType Type { get { return StatusType.Marked; } }

        /// <inheritdoc/>
        public override bool IsApplied { get { return MarkDuration > 0; } }

        /// <inheritdoc/>
        public DurationType DurationType { get; set; }

        /// <inheritdoc/>
        public int MarkDuration { get; set; }

        /// <inheritdoc/>
        public override void UpdateNextTurn()
        {
            if (DurationType == DurationType.Combat)
                return;

            if (MarkDuration > 0)
                MarkDuration--;
        }

        /// <inheritdoc/>
        public override void ResetStatus()
        {
            MarkDuration = 0;
        }
    }
}