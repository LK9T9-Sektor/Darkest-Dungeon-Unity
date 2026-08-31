using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Tests for the pre-built 4x4 hover-arrow cell mask.</summary>
    [TestFixture]
    public class DuelArrowCellsTests
    {
        /// <summary>The arrow band must cover 12 of the 16 cells and every index must be in range.</summary>
        [Test]
        public void MaskFor_CoversTwelveCellsAndValidIndices()
        {
            foreach (var team in new[] { Team.Heroes, Team.Monsters })
            {
                var mask = DuelArrowCells.MaskFor(team);
                Assert.That(mask, Is.Not.Null);
                Assert.That(mask.Count, Is.EqualTo(12), "The tapered band lights 12 of 16 cells.");
                Assert.That(mask.Distinct().Count(), Is.EqualTo(mask.Count), "No duplicate cell indices.");
                Assert.That(mask.All(i => i >= 0 && i < DuelArrowCells.CellCount), Is.True, "Indices must be in range.");
            }
        }

        /// <summary>The band is symmetric, so both teams light the same cells (the source/target
        /// ends are only a mirrored reading of the same geometry).</summary>
        [Test]
        public void MaskFor_IsSymmetricAcrossTeams()
        {
            Assert.That(DuelArrowCells.MaskFor(Team.Heroes), Is.EquivalentTo(DuelArrowCells.MaskFor(Team.Monsters)));
        }

        /// <summary>Edge cells are thin (only the middle rows) while the inner columns cover every
        /// row, giving the pseudo-3D "small at the ends, max at the center" shape.</summary>
        [Test]
        public void MaskFor_TapersOnlyEdges()
        {
            var mask = DuelArrowCells.MaskFor(Team.Heroes);

            int edgeIndex = DuelArrowCells.Index(0, 0);
            int centerIndex = DuelArrowCells.Index(0, 1);
            Assert.That(mask, Does.Not.Contain(edgeIndex), "Top-left corner cell stays dark.");
            Assert.That(mask, Does.Contain(centerIndex), "Middle column spans the full height.");

            Assert.That(mask, Does.Contain(DuelArrowCells.Index(1, 0)), "Edge column keeps its middle rows.");
            Assert.That(mask, Does.Contain(DuelArrowCells.Index(2, 3)), "Edge column keeps its middle rows.");
            Assert.That(mask, Does.Not.Contain(DuelArrowCells.Index(3, 0)), "Bottom-left corner stays dark.");
        }
    }
}