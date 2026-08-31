using System;
using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Pre-computes the overlay cells forming the rank-aware hover attack arrow.</summary>
    /// <remarks>
    /// The battlefield is covered by an 8x4 cell grid (column * 4 + row, indices 0..31). Columns
    /// 1..4 are the left (hero) ranks 1..4, columns 5..8 the right (monster) ranks 1..4. The arrow
    /// always starts just past the acting unit's column and ends at the hovered target's column
    /// (inclusive), so its span depends on both ranks: a left rank-4 actor lights columns 5..N,
    /// e.g. up to column 5 for a right rank-1 target (indices 16..19, "to 20"), column 6 for a
    /// rank-2 target ("to 24") and so on. Every (team, source rank, target rank) combination is
    /// pre-computed once into a lookup table; per column the lit rows form the pseudo-3D taper
    /// (thin 2-cell edges, full height in the middle of the field).
    /// </remarks>
    public static class DuelArrowCells
    {
        /// <summary>Gets the number of vertical rows per arrow column.</summary>
        public const int RowsPerColumn = 4;

        /// <summary>Gets the number of rank columns (4 per team).</summary>
        public const int RankColumns = 8;

        /// <summary>Gets the total number of overlay cells.</summary>
        public const int CellCount = RankColumns * RowsPerColumn;

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
                        Tables[team][source - 1][target - 1] = Build(source, target, team == 0);
                }
            }
        }

        /// <summary>Returns the pre-computed lit cell indices for the actor team and ranks.</summary>
        /// <param name="sourceTeam">The team of the acting unit (Heroes are the left team).</param>
        /// <param name="sourceRank">The acting unit's rank (1-4).</param>
        /// <param name="targetRank">The hovered target's rank on the opposite side (1-4).</param>
        /// <returns>The lit cell indices for the arrow span.</returns>
        public static IReadOnlyList<int> MaskFor(Team sourceTeam, int sourceRank, int targetRank)
        {
            bool isLeft = sourceTeam == Team.Heroes;
            return Tables[isLeft ? 0 : 1][sourceRank - 1][targetRank - 1];
        }

        /// <summary>Gets the cell index for the given 1-based column and 0-based row.</summary>
        /// <param name="column">The 1-based rank column (1-8).</param>
        /// <param name="row">The row (0-3).</param>
        /// <returns>The cell index.</returns>
        public static int Index(int column, int row)
        {
            return (column - 1) * RowsPerColumn + row;
        }

        private static int[] Build(int sourceRank, int targetRank, bool isLeft)
        {
            int sourceColumn = isLeft ? sourceRank : Ranks + sourceRank;
            int targetColumn = isLeft ? Ranks + targetRank : targetRank;
            int from;
            int to;
            if (isLeft)
            {
                from = sourceColumn + 1;
                to = targetColumn;
            }
            else
            {
                from = targetColumn;
                to = sourceColumn - 1;
            }

            var cells = new List<int>();
            for (int column = Math.Max(1, from); column <= Math.Min(RankColumns, to); column++)
            {
                foreach (int row in RowsFor(column))
                    cells.Add(Index(column, row));
            }

            return cells.ToArray();
        }

        private static IEnumerable<int> RowsFor(int column)
        {
            if (column == 1 || column == RankColumns)
                return new[] { 1, 2 };
            if (column == 2 || column == RankColumns - 1)
                return new[] { 0, 1, 2 };
            return new[] { 0, 1, 2, 3 };
        }
    }
}