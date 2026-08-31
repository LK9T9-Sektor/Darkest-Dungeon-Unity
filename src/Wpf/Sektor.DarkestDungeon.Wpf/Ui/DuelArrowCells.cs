using System;
using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Pre-computes the overlay slots forming the rank-aware hover attack arrow.</summary>
    /// <remarks>
    /// The battlefield is covered by 8 horizontal slots in visual order 0..7 = hero ranks 4,3,2,1
    /// on the left then monster ranks 1,2,3,4 on the right; a hero of rank r sits at slot 4 - r and
    /// a monster at slot 3 + r, so the two rank-1 "front" units are the neighbours of the field
    /// center (slots 3 and 4). The arrow always starts just past the acting unit's slot and ends at
    /// the hovered target's slot (inclusive), so its span depends on both ranks. Every (team, source
    /// rank, target rank) combination is pre-computed once into a lookup table and a hover only
    /// indexes it.
    /// </remarks>
    public static class DuelArrowCells
    {
        /// <summary>Gets the number of unit slots (4 per team).</summary>
        public const int CellCount = 8;

        private const int Ranks = 4;

        private static readonly int[][][][] Tables = new int[2][][][];

        static DuelArrowCells()
        {
            for (int team = 0; team < 2; team++)
            {
                Tables[team] = new int[Ranks][][];
                for (int source = 1; source <= Ranks; source++)
                {
                    Tables[team][source - 1] = new int[Ranks][];
                    for (int target = 1; target <= Ranks; target++)
                        Tables[team][source - 1][target - 1] = Build(team == 0 ? Team.Heroes : Team.Monsters, source, target);
                }
            }
        }

        /// <summary>Returns the visual slot (0-7) of the given team's rank.</summary>
        /// <param name="team">The team (heroes occupy the left slots).</param>
        /// <param name="rank">The rank (1-4).</param>
        /// <returns>Slot 0-3 for a hero (4,3,2,1) and 4-7 for a monster (1,2,3,4).</returns>
        public static int SlotFor(Team team, int rank)
        {
            return team == Team.Heroes ? Ranks - rank : Ranks - 1 + rank;
        }

        /// <summary>Returns the pre-computed lit slot indices for the actor team and ranks.</summary>
        /// <param name="sourceTeam">The team of the acting unit (heroes are the left team).</param>
        /// <param name="sourceRank">The acting unit's rank (1-4).</param>
        /// <param name="targetRank">The hovered target's rank on the opposite side (1-4).</param>
        /// <returns>The lit slots for the arrow span.</returns>
        public static IReadOnlyList<int> MaskFor(Team sourceTeam, int sourceRank, int targetRank)
        {
            int team = sourceTeam == Team.Heroes ? 0 : 1;
            return Tables[team][sourceRank - 1][targetRank - 1];
        }

        private static int[] Build(Team sourceTeam, int sourceRank, int targetRank)
        {
            int sourceSlot = SlotFor(sourceTeam, sourceRank);
            int targetSlot = SlotFor(Opposite(sourceTeam), targetRank);
            int from = sourceTeam == Team.Heroes ? sourceSlot + 1 : targetSlot;
            int to = sourceTeam == Team.Heroes ? targetSlot : sourceSlot - 1;

            var slots = new List<int>(to - from + 1);
            for (int slot = from; slot <= to; slot++)
                slots.Add(slot);
            return slots.ToArray();
        }

        private static Team Opposite(Team team)
        {
            return team == Team.Heroes ? Team.Monsters : Team.Heroes;
        }
    }
}