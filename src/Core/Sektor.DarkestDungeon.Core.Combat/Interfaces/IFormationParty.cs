using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Interfaces
{
    /// <summary>Abstraction of a formation party (hero or monster side).</summary>
    public interface IFormationParty
    {
        /// <summary>Gets the list of units in this party.</summary>
        List<ICombatUnit> Units { get; }

        /// <summary>Gets the ranks marked for targeting by skills.</summary>
        List<int> MarkedRanks { get; }
    }
}
