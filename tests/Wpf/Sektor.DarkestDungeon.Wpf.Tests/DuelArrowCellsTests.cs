using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Tests for the pre-computed rank-aware hover-arrow cell table.</summary>
    [TestFixture]
    public class DuelArrowCellsTests
    {
        /// <summary>The user-specified example: a left rank-4 actor never paints the first four
        /// columns (the arrow starts at column 5, "index 20" boundary) and its last lit column is
        /// the hovered target's: rank 1 = column 5, rank 2 = column 6, rank 3 = column 7, rank 4 =
        /// column 8 (the far edge).</summary>
        [Test]
        public void MaskFor_LeftRankFourSpansFromFifthColumnToTarget()
        {
            Assert.That(DuelArrowCells.MaskFor(Team.Heroes, 4, 1).All(i => i >= DuelArrowCells.Index(5, 0)),
                Is.True, "The first four columns stay dark for a left rank-4 actor.");
            Assert.That(Columns(DuelArrowCells.MaskFor(Team.Heroes, 4, 1)), Is.EqualTo(new[] { 5 }), "Right rank 1 ends at column 5.");
            Assert.That(Columns(DuelArrowCells.MaskFor(Team.Heroes, 4, 2)), Is.EqualTo(new[] { 5, 6 }), "Right rank 2 ends at column 6.");
            Assert.That(Columns(DuelArrowCells.MaskFor(Team.Heroes, 4, 3)), Is.EqualTo(new[] { 5, 6, 7 }), "Right rank 3 ends at column 7.");
            Assert.That(Columns(DuelArrowCells.MaskFor(Team.Heroes, 4, 4)), Is.EqualTo(new[] { 5, 6, 7, 8 }), "Right rank 4 ends at column 8.");
        }

        /// <summary>Left rank 1 (column 1) lights every column after its own up to the target:
        /// rank 1 target ends at column 5, rank 4 ends at the far edge.</summary>
        [Test]
        public void MaskFor_LeftRankOneSpansForwards()
        {
            Assert.That(Columns(DuelArrowCells.MaskFor(Team.Heroes, 1, 1)), Is.EqualTo(new[] { 2, 3, 4, 5 }));
            Assert.That(Columns(DuelArrowCells.MaskFor(Team.Heroes, 1, 4)), Is.EqualTo(new[] { 2, 3, 4, 5, 6, 7, 8 }));
        }

        /// <summary>The right side mirrors the left: a right rank-4 actor paints the columns
        /// between the left-ward target and its own column.</summary>
        [Test]
        public void MaskFor_RightRankFourSpansTowardsTarget()
        {
            Assert.That(Columns(DuelArrowCells.MaskFor(Team.Monsters, 4, 1)), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7 }));
            Assert.That(Columns(DuelArrowCells.MaskFor(Team.Monsters, 4, 4)), Is.EqualTo(new[] { 4, 5, 6, 7 }));
            Assert.That(Columns(DuelArrowCells.MaskFor(Team.Monsters, 1, 1)), Is.EqualTo(new[] { 1, 2, 3, 4 }));
            Assert.That(DuelArrowCells.MaskFor(Team.Monsters, 4, 1).All(i => i >= DuelArrowCells.Index(1, 0)),
                Is.True, "No cell beyond the actor's right column is lit.");
        }

        /// <summary>Every pre-computed entry is non-empty, uses only valid cell indices once each
        /// and never lights the acting unit's own columns.</summary>
        [Test]
        public void MaskFor_FullTableIsValid()
        {
            foreach (var team in new[] { Team.Heroes, Team.Monsters })
            {
                for (int source = 1; source <= 4; source++)
                {
                    for (int target = 1; target <= 4; target++)
                    {
                        IReadOnlyList<int> mask = DuelArrowCells.MaskFor(team, source, target);
                        Assert.That(mask, Is.Not.Null);
                        Assert.That(mask.Count, Is.GreaterThan(0), "The arrow must always light at least one cell.");
                        Assert.That(mask.Distinct().Count(), Is.EqualTo(mask.Count), "No duplicate cell indices.");
                        Assert.That(mask.All(i => i >= 0 && i < DuelArrowCells.CellCount), Is.True, "Indices in range.");

                        int sourceColumn = team == Team.Heroes ? source : 4 + source;
                        Assert.That(Columns(mask).Contains(sourceColumn), Is.False, "The actor's own column is never lit.");
                    }
                }
            }
        }

        /// <summary>The taper: the far edge column keeps only the middle rows while the mid-field
        /// columns span the full height, so the band is thin at the ends and max in the center.</summary>
        [Test]
        public void MaskFor_TapersEdgesToCenter()
        {
            IReadOnlyList<int> fullField = DuelArrowCells.MaskFor(Team.Heroes, 1, 4);
            Assert.That(LitRows(fullField, 8).Count, Is.EqualTo(2), "The far edge column keeps only the middle rows.");
            Assert.That(LitRows(fullField, 4).Count, Is.EqualTo(4), "The mid-field column is at full height.");
        }

        private static IReadOnlyList<int> LitRows(IReadOnlyList<int> mask, int column)
        {
            return mask.Where(i => (i / DuelArrowCells.RowsPerColumn) + 1 == column)
                .Select(i => i % DuelArrowCells.RowsPerColumn).ToArray();
        }

        private static IReadOnlyList<int> Columns(IReadOnlyList<int> mask)
        {
            return mask.Select(i => (i / DuelArrowCells.RowsPerColumn) + 1).Distinct().OrderBy(c => c).ToArray();
        }
    }
}