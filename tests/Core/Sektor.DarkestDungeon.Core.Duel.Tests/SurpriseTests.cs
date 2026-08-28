namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

    [TestFixture]
    public class SurpriseTests
    {
        [Test]
        public void MonstersSurprised_ActsLastAndMarksTheMonsterParty()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 1, isHost: true);
            RandomSolver.SetRandomSeed(1);
            duel.StartBattle();

            Assert.That(duel.BattleGround.SurpriseStatus, Is.EqualTo(SurpriseStatus.MonstersSurprised));
            Assert.That(duel.MonsterParty.Units.All(unit => unit.CombatInfo.IsSurprised), Is.True,
                "The surprised monsters should be flagged.");
            Assert.That(duel.HeroParty.Units.All(unit => !unit.CombatInfo.IsSurprised), Is.True);

            var order = duel.BattleGround.Round.OrderedUnits;
            int firstMonster = order.FindIndex(unit => unit.Team == Team.Monsters);
            int lastHero = order.FindLastIndex(unit => unit.Team == Team.Heroes);
            Assert.That(firstMonster, Is.GreaterThan(lastHero),
                "Surprised monsters should act after all heroes in the first round.");
        }

        [Test]
        public void HeroesSurprised_ActsLastAndMarksTheHeroParty()
        {
            var content = new TestDuelContent();
            var duel = new DuelController(content);
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 11, isHost: true);
            RandomSolver.SetRandomSeed(11);
            duel.StartBattle();

            Assert.That(duel.BattleGround.SurpriseStatus, Is.EqualTo(SurpriseStatus.HeroesSurprised));
            Assert.That(duel.HeroParty.Units.All(unit => unit.CombatInfo.IsSurprised), Is.True,
                "The surprised heroes should be flagged.");

            var order = duel.BattleGround.Round.OrderedUnits;
            int firstHero = order.FindIndex(unit => unit.Team == Team.Heroes);
            int lastMonster = order.FindLastIndex(unit => unit.Team == Team.Monsters);
            Assert.That(firstHero, Is.GreaterThan(lastMonster),
                "Surprised heroes should act after all monsters in the first round.");
        }

        private static DuelHeroPick[] Picks(string classId)
        {
            return new[]
            {
                new DuelHeroPick(classId, 1),
                new DuelHeroPick(classId, 2),
                new DuelHeroPick(classId, 3),
                new DuelHeroPick(classId, 4),
            };
        }
    }
}