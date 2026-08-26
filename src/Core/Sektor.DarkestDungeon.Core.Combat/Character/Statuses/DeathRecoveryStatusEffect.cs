using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Death recovery status (post death's door recovery).</summary>
    public class DeathRecoveryStatusEffect : StatusEffect
    {
        /// <inheritdoc/>
        public override StatusType Type { get { return StatusType.DeathRecovery; } }

        /// <inheritdoc/>
        public override bool IsApplied { get { return AtDeathRecovery; } }

        /// <summary>Gets or sets a value indicating whether the character is recovering.</summary>
        public bool AtDeathRecovery { get; set; }

        /// <inheritdoc/>
        public override void UpdateNextTurn()
        {
        }

        /// <inheritdoc/>
        public override void ResetStatus()
        {
            AtDeathRecovery = false;
        }
    }
}