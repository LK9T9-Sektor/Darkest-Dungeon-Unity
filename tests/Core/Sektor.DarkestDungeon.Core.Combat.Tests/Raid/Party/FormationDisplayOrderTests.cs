namespace Sektor.DarkestDungeon.Core.Combat.Tests.Raid.Party
{
    using System.Collections.Generic;
    using System.Linq;

    using NSubstitute;
    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
    using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

    [TestFixture]
    public class FormationDisplayOrderTests
    {
        private static ICombatUnit Unit(int rank)
        {
            var unit = Substitute.For<ICombatUnit>();
            unit.Rank.Returns(rank);
            return unit;
        }

        private static IFormationParty ScrambledParty()
        {
            var units = new List<ICombatUnit> { Unit(3), Unit(1), Unit(4), Unit(2) };
            var party = Substitute.For<IFormationParty>();
            party.Units.Returns(units);
            return party;
        }

        [Test]
        public void HeroSide_OrdersBackToFront()
        {
            var ranks = FormationDisplayOrder.HeroSide().OrderLeftToRight(ScrambledParty()).Select(unit => unit.Rank);

            CollectionAssert.AreEqual(new[] { 4, 3, 2, 1 }, ranks);
        }

        [Test]
        public void MonsterSide_OrdersFrontToBack()
        {
            var ranks = FormationDisplayOrder.MonsterSide().OrderLeftToRight(ScrambledParty()).Select(unit => unit.Rank);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, ranks);
        }
    }
}