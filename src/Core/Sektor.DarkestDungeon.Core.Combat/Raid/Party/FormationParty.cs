using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Raid.Party
{
    /// <summary>A formation party: an ordered list of combat units with marked ranks.</summary>
    public class FormationParty : IFormationParty
    {
        /// <inheritdoc/>
        public List<ICombatUnit> Units { get; }

        /// <inheritdoc/>
        public List<int> MarkedRanks { get; }

        /// <summary>Initializes a new instance of the <see cref="FormationParty"/> class.</summary>
        public FormationParty()
        {
            Units = new List<ICombatUnit>();
            MarkedRanks = new List<int>();
        }

        /// <summary>Adds a unit and assigns it the next rank.</summary>
        /// <param name="unit">The unit to add.</param>
        public void AddUnit(FormationUnit unit)
        {
            unit.Rank = Units.Count + 1;
            unit.Party = this;
            Units.Add(unit);
        }

        /// <summary>Removes a unit and re-assigns ranks.</summary>
        /// <param name="unit">The unit to remove.</param>
        public void RemoveUnit(FormationUnit unit)
        {
            Units.Remove(unit);
            for (int i = 0; i < Units.Count; i++)
                ((FormationUnit)Units[i]).Rank = i + 1;
        }
    }
}