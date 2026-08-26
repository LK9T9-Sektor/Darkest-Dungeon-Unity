namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics
{
    using NSubstitute;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
    using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

    [TestFixture]
    public class RoundTests
    {
        [Test]
        public void PreHeroTurn_SetsHeroStateAndClearsLastSkill()
        {
            var unit = Substitute.For<ICombatUnit>();
            var info = Substitute.For<IFormationUnitInfo>();
            unit.CombatInfo.Returns(info);
            var battleGround = Substitute.For<IBattleGround>();

            var round = new Round();
            round.PreHeroTurn(unit, battleGround);

            Assert.That(round.TurnType, Is.EqualTo(TurnType.HeroTurn));
            Assert.That(round.TurnStatus, Is.EqualTo(TurnStatus.PreTurn));
            Assert.That(round.HeroAction, Is.EqualTo(HeroTurnAction.Waiting));
            Assert.That(round.SelectedUnit, Is.SameAs(unit));

            info.Received(1).UpdateNextTurn();
        }

        [Test]
        public void PreMonsterTurn_SetsMonsterState()
        {
            var unit = Substitute.For<ICombatUnit>();
            var info = Substitute.For<IFormationUnitInfo>();
            unit.CombatInfo.Returns(info);
            var battleGround = Substitute.For<IBattleGround>();

            var round = new Round();
            round.PreMonsterTurn(unit, battleGround);

            Assert.That(round.TurnType, Is.EqualTo(TurnType.MonsterTurn));
            Assert.That(round.SelectedUnit, Is.SameAs(unit));
        }

        [Test]
        public void PostTurn_ClearsSelection()
        {
            var unit = Substitute.For<ICombatUnit>();
            var info = Substitute.For<IFormationUnitInfo>();
            unit.CombatInfo.Returns(info);

            var round = new Round();
            round.PreHeroTurn(unit, Substitute.For<IBattleGround>());
            round.PostHeroTurn();

            Assert.That(round.SelectedUnit, Is.Null);
            Assert.That(round.SelectedTarget, Is.Null);
        }
    }
}