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

        /// <summary>Adds a unit and assigns it the next free rank (after the last unit's occupied ranks).</summary>
        /// <param name="unit">The unit to add.</param>
        public void AddUnit(FormationUnit unit)
        {
            if (Units.Count == 0)
                unit.Rank = 1;
            else
            {
                var last = (FormationUnit)Units[Units.Count - 1];
                unit.Rank = last.Rank + last.Size;
            }

            unit.Party = this;
            Units.Add(unit);
        }

        /// <summary>Removes a unit and re-assigns ranks cumulatively by unit size (like Unity).</summary>
        /// <param name="unit">The unit to remove.</param>
        public void RemoveUnit(FormationUnit unit)
        {
            Units.Remove(unit);
            RecalculateRanks();
        }

        /// <summary>Re-assigns ranks cumulatively so each unit starts after the previous one's occupied ranks.</summary>
        public void RecalculateRanks()
        {
            int nextRank = 1;
            foreach (ICombatUnit unit in Units)
            {
                ((FormationUnit)unit).Rank = nextRank;
                nextRank += unit.Size;
            }
        }
    }
}