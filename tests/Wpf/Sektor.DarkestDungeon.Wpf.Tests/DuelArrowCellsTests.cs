using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Tests for the pre-computed rank-aware hover-arrow slot table.</summary>
    [TestFixture]
    public class DuelArrowCellsTests
    {
        /// <summary>Visual slots run 0..7 = hero ranks 4,3,2,1 then monster ranks 1,2,3,4, so the
        /// two rank-1 front units meet at the field center (slots 3 and 4).</summary>
        [Test]
        public void SlotFor_InvertsRanksIntoVisualOrder()
        {
            Assert.That(DuelArrowCells.SlotFor(Team.Heroes, 1), Is.EqualTo(3));
            Assert.That(DuelArrowCells.SlotFor(Team.Heroes, 2), Is.EqualTo(2));
            Assert.That(DuelArrowCells.SlotFor(Team.Heroes, 3), Is.EqualTo(1));
            Assert.That(DuelArrowCells.SlotFor(Team.Heroes, 4), Is.EqualTo(0));
            Assert.That(DuelArrowCells.SlotFor(Team.Monsters, 1), Is.EqualTo(4));
            Assert.That(DuelArrowCells.SlotFor(Team.Monsters, 2), Is.EqualTo(5));
            Assert.That(DuelArrowCells.SlotFor(Team.Monsters, 3), Is.EqualTo(6));
            Assert.That(DuelArrowCells.SlotFor(Team.Monsters, 4), Is.EqualTo(7));
        }

        /// <summary>A left rank-1 actor is one slot away from a right rank-1 target (both front
        /// ranks), so the arrow lights only the single center slot instead of the whole field.</summary>
        [Test]
        public void MaskFor_HeroRankOneToMonsterRankOne_LightsOnlyCenterSlot()
        {
            Assert.That(DuelArrowCells.MaskFor(Team.Heroes, 1, 1), Is.EqualTo(new[] { 4 }));
        }

        /// <summary>The mirror case: a right rank-1 actor aiming at a left rank-1 target lights
        /// only the single center slot on the hero side.</summary>
        [Test]
        public void MaskFor_MonsterRankOneToHeroRankOne_LightsOnlyCenterSlot()
        {
            Assert.That(DuelArrowCells.MaskFor(Team.Monsters, 1, 1), Is.EqualTo(new[] { 3 }));
        }

        /// <summary>A left rank-4 (rearmost) actor lights every slot past its own — slots 1..4/5/6/7
        /// depending on the right target's rank.</summary>
        [Test]
        public void MaskFor_LeftRankFourSpansFromSecondSlotToTarget()
        {
            Assert.That(DuelArrowCells.MaskFor(Team.Heroes, 4, 1), Is.EqualTo(new[] { 1, 2, 3, 4 }));
            Assert.That(DuelArrowCells.MaskFor(Team.Heroes, 4, 2), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
            Assert.That(DuelArrowCells.MaskFor(Team.Heroes, 4, 3), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
            Assert.That(DuelArrowCells.MaskFor(Team.Heroes, 4, 4), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7 }));
        }

        /// <summary>The far corners are a long spoke: hero rank 4 to monster rank 4 covers every
        /// slot between them, monster rank 4 to hero rank 4 mirrors it on the left side.</summary>
        [Test]
        public void MaskFor_FarCornersSpanTheWholeField()
        {
            Assert.That(DuelArrowCells.MaskFor(Team.Heroes, 4, 4), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7 }));
            Assert.That(DuelArrowCells.MaskFor(Team.Monsters, 4, 4), Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5, 6 }));
        }

        /// <summary>Every pre-computed entry is a non-empty contiguous run of slots, never lights the
        /// acting unit's own slot and stays within the 0..7 range; a hero actor only lights the
        /// right (monster) side and vice versa.</summary>
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
                        Assert.That(mask.Count, Is.GreaterThan(0), "The arrow must always light at least one slot.");
                        Assert.That(mask.Distinct().Count(), Is.EqualTo(mask.Count), "No duplicate slot indices.");
                        Assert.That(mask.All(i => i >= 0 && i < DuelArrowCells.CellCount), Is.True, "Indices in range.");
                        Assert.That(mask.Contains(DuelArrowCells.SlotFor(team, source)), Is.False,
                            "The actor's own slot is never lit.");

                        foreach (int step in mask.Zip(mask.Skip(1), (a, b) => b - a))
                            Assert.That(step, Is.EqualTo(1), "The mask is a contiguous run of slots.");

                        int sourceSlot = DuelArrowCells.SlotFor(team, source);
                        Assert.That(team == Team.Heroes ? mask.All(i => i > sourceSlot) : mask.All(i => i < sourceSlot),
                            Is.True, "The arrow only lights slots towards the target side of the actor.");
                    }
                }
            }
        }
    }
}