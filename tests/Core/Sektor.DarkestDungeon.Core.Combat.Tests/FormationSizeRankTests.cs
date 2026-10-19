using System.Collections.Generic;
using NUnit.Framework;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Core.Combat.Tests
{
    /// <summary>
    /// Verifies size-aware formation ranks: a monster with size N occupies N ranks and the next unit
    /// starts after it, mirroring the legacy Unity rank assignment (cumulative size).
    /// </summary>
    [TestFixture]
    public class FormationSizeRankTests
    {
        private static Monster MakeMonster(string id, int size)
        {
            var monsterClass = new MonsterClass
            {
                StringId = id,
                TypeId = id,
                Size = size,
            };
            monsterClass.Attributes[AttributeType.HitPoints] = 100f;
            return new Monster(monsterClass);
        }

        private static FormationUnit Unit(Monster monster)
        {
            return new FormationUnit(monster, Team.Monsters);
        }

        [Test]
        public void AddUnit_SizeTwoThenSizeOne_RanksAreCumulative()
        {
            var party = new FormationParty();
            var big = Unit(MakeMonster("big", 2));
            var small = Unit(MakeMonster("small", 1));

            party.AddUnit(big);
            party.AddUnit(small);

            Assert.That(big.Rank, Is.EqualTo(1), "The size-2 monster occupies ranks 1-2.");
            Assert.That(small.Rank, Is.EqualTo(3), "The next unit starts after the occupied ranks.");
        }

        [Test]
        public void AddUnit_SizeThreeThenSizeOne_RanksAreCumulative()
        {
            var party = new FormationParty();
            var big = Unit(MakeMonster("big", 3));
            var small = Unit(MakeMonster("small", 1));

            party.AddUnit(big);
            party.AddUnit(small);

            Assert.That(big.Rank, Is.EqualTo(1));
            Assert.That(small.Rank, Is.EqualTo(4));
        }

        [Test]
        public void RemoveUnit_SizeTwoMiddle_RanksReassignCumulatively()
        {
            var party = new FormationParty();
            var first = Unit(MakeMonster("first", 1));
            var big = Unit(MakeMonster("big", 2));
            var last = Unit(MakeMonster("last", 1));
            party.AddUnit(first);
            party.AddUnit(big);
            party.AddUnit(last);

            Assert.That(new[] { first.Rank, big.Rank, last.Rank }, Is.EqualTo(new[] { 1, 2, 4 }));

            party.RemoveUnit(big);

            Assert.That(first.Rank, Is.EqualTo(1));
            Assert.That(last.Rank, Is.EqualTo(2), "Survivors reflow forward after the size-2 unit dies.");
        }

        [Test]
        public void RecalculateRanks_AfterShuffle_IsCumulative()
        {
            var party = new FormationParty();
            var big = Unit(MakeMonster("big", 2));
            var small = Unit(MakeMonster("small", 1));
            party.AddUnit(big);
            party.AddUnit(small);

            party.Units.Reverse();
            party.RecalculateRanks();

            Assert.That(small.Rank, Is.EqualTo(1));
            Assert.That(big.Rank, Is.EqualTo(2), "After the swap the size-2 monster starts at the front's occupied end.");
        }
    }
}