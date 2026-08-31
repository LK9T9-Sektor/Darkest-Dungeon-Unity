using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Computes the pre-built 4x4 overlay cells forming the hover attack arrow.</summary>
    /// <remarks>
    /// The battlefield is covered by a fixed 4x4 cell grid (row-major indices, 0..15) whose edge
    /// cells are narrow and whose middle cells are wide, so lighting the band below yields the
    /// pseudo-3D effect: small rectangles at both ends, a maximum at the center of the field.
    /// The band is symmetric, so both teams light the same cells; positions are mirrored only in
    /// the mapping direction (left team starts at column 3 toward column 0, the right team the
    /// other way around), which is kept here for future rank-relative modes.
    /// </remarks>
    public static class DuelArrowCells
    {
        /// <summary>Gets the number of overlay cells per side (4x4).</summary>
        public const int GridSize = 4;

        /// <summary>Gets the total number of overlay cells.</summary>
        public const int CellCount = GridSize * GridSize;

        /// <summary>Returns the row-major indices of the lit arrow cells for the given actor team.</summary>
        /// <param name="actorTeam">The team of the acting unit.</param>
        /// <returns>The lit cell indices (12 of 16).</returns>
        public static IReadOnlyList<int> MaskFor(Team actorTeam)
        {
            var cells = new List<int>(CellCount);
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    if (IsLit(row, col))
                        cells.Add(Index(row, col));
                }
            }

            return cells;
        }

        /// <summary>Gets the row-major index of the cell at the given grid coordinates.</summary>
        /// <param name="row">The row (0-based).</param>
        /// <param name="col">The column (0-based).</param>
        /// <returns>The index.</returns>
        public static int Index(int row, int col)
        {
            return row * GridSize + col;
        }

        /// <summary>Whether the cell at the given grid coordinates belongs to the arrow band.</summary>
        /// <param name="row">The row (0-based).</param>
        /// <param name="col">The column (0-based).</param>
        /// <returns>True when lit.</returns>
        private static bool IsLit(int row, int col)
        {
            bool middleRows = row >= 1 && row <= 2;
            if (col == 0 || col == GridSize - 1)
                return middleRows;
            return row >= 0 && row < GridSize;
        }
    }
}