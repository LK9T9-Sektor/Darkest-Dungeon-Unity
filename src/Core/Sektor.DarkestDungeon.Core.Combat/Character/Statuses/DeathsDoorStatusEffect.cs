using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Death's door status.</summary>
    public class DeathsDoorStatusEffect : StatusEffect
    {
        /// <inheritdoc/>
        public override StatusType Type { get { return StatusType.DeathsDoor; } }

        /// <inheritdoc/>
        public override bool IsApplied { get { return AtDeathsDoor; } }

        /// <summary>Gets or sets a value indicating whether the character is at death's door.</summary>
        public bool AtDeathsDoor { get; set; }

        /// <inheritdoc/>
        public override void UpdateNextTurn()
        {
        }

        /// <inheritdoc/>
        public override void ResetStatus()
        {
            AtDeathsDoor = false;
        }
    }
}