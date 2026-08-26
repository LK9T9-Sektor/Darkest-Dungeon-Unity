using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Enums;

namespace Sektor.DarkestDungeon.Core.Combat.Interfaces
{
    /// <summary>Abstraction of a combat unit on the battlefield.</summary>
    public interface ICombatUnit
    {
        /// <summary>Gets the unit's position rank (1-4).</summary>
        int Rank { get; }

        /// <summary>Gets the unit's team affiliation.</summary>
        Team Team { get; }

        /// <summary>Gets the unit's character data.</summary>
        ICharacter Character { get; }

        /// <summary>Gets the unit's combat state information.</summary>
        IFormationUnitInfo CombatInfo { get; }

        /// <summary>Gets the unit's party.</summary>
        IFormationParty Party { get; }

        /// <summary>Gets the unit's size (1 for normal, 2 for large monsters).</summary>
        int Size { get; }

        /// <summary>Gets a value indicating whether this unit is a corpse.</summary>
        bool IsCorpse { get; }

        /// <summary>Gets a value indicating whether this unit can be targeted.</summary>
        bool IsTargetable { get; }

        /// <summary>Gets the effect event queue.</summary>
        List<IEffectEvent> EventQueue { get; }
    }
}
