using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character.Statuses
{
    /// <summary>Abstract base of a character status effect.</summary>
    public abstract class StatusEffect : IStatusEffect, IResetableStatusEffect
    {
        /// <summary>Gets the status type.</summary>
        public abstract StatusType Type { get; }

        /// <inheritdoc/>
        public abstract bool IsApplied { get; }

        /// <summary>Updates the status at the next turn.</summary>
        public abstract void UpdateNextTurn();

        /// <inheritdoc/>
        public abstract void ResetStatus();
    }
}