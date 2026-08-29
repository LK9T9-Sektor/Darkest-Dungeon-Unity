using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Raid.Party
{
    /// <summary>Holds the marked ranks of a formation (targeting marks).</summary>
    public class FormationRanks
    {
        /// <summary>Gets the marked ranks.</summary>
        public List<int> MarkedRanks { get; }

        /// <summary>Initializes a new instance of the <see cref="FormationRanks"/> class.</summary>
        public FormationRanks()
        {
            MarkedRanks = new List<int>();
        }

        /// <summary>Marks a rank for targeting.</summary>
        /// <param name="rank">The rank to mark.</param>
        public void MarkRank(int rank)
        {
            MarkedRanks.Add(rank);
        }

        /// <summary>Clears all marks.</summary>
        public void ClearMarks()
        {
            MarkedRanks.Clear();
        }
    }
}