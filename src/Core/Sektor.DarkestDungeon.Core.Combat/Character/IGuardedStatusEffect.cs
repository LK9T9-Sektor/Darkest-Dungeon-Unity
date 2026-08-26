using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of the guarded status effect (target side).</summary>
    public interface IGuardedStatusEffect : IStatusEffect
    {
        /// <summary>Resets the status.</summary>
        void ResetStatus();

        /// <summary>Gets or sets the guarding unit.</summary>
        ICombatUnit Guard { get; set; }

        /// <summary>Gets or sets the guard duration.</summary>
        int GuardDuration { get; set; }
    }
}