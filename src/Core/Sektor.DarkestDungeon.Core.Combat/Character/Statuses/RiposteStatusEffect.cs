using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Riposte status.</summary>
    public class RiposteStatusEffect : StatusEffect, IRiposteStatusEffect
    {
        /// <inheritdoc/>
        public override StatusType Type { get { return StatusType.Riposte; } }

        /// <inheritdoc/>
        public override bool IsApplied { get { return RiposteDuration > 0; } }

        /// <inheritdoc/>
        public DurationType DurationType { get; set; }

        /// <inheritdoc/>
        public int RiposteDuration { get; set; }

        /// <inheritdoc/>
        public override void UpdateNextTurn()
        {
            if (DurationType == DurationType.Combat)
                return;

            if (RiposteDuration > 0)
                RiposteDuration--;
        }

        /// <inheritdoc/>
        public override void ResetStatus()
        {
            RiposteDuration = 0;
        }
    }
}