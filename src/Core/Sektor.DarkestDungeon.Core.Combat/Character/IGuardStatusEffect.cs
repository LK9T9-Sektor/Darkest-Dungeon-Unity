using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of the guard status effect (performer side).</summary>
    public interface IGuardStatusEffect : IStatusEffect
    {
        /// <summary>Resets the status.</summary>
        void ResetStatus();

        /// <summary>Gets the list of guarded target units.</summary>
        List<ICombatUnit> Targets { get; }
    }
}