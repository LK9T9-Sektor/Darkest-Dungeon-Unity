using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Stun status.</summary>
    public class StunStatusEffect : StatusEffect, IStunStatusEffect
    {
        /// <inheritdoc/>
        public override StatusType Type { get { return StatusType.Stun; } }

        /// <inheritdoc/>
        public override bool IsApplied { get { return StunApplied; } }

        /// <inheritdoc/>
        public bool StunApplied { get; set; }

        /// <inheritdoc/>
        public override void UpdateNextTurn()
        {
        }

        /// <inheritdoc/>
        public override void ResetStatus()
        {
            StunApplied = false;
        }
    }
}