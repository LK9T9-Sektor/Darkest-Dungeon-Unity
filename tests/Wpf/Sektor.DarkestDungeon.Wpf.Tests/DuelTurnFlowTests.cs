namespace Sektor.DarkestDungeon.Wpf.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Wpf.Combat;

    [TestFixture]
    public class DuelTurnFlowTests
    {
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

        private static void PlayTurn(DuelController host, DuelController client, int seed)
        {
            if (host.IsLocalTurn)
            {
                var unit = host.CurrentUnit;
                var skill = FirstUsableSkill(host, unit);
                if (skill == null)
                    return;
                var target = host.GetAvailableTargets(unit, skill)[0];
                RandomSolver.SetRandomSeed(seed);
                string? payload = host.ExecuteLocalSkill(skill.Id, target.CombatInfo.CombatId);
                RandomSolver.SetRandomSeed(seed);
                client.ApplyRemoteSkill(payload!);
            }
            else if (client.IsLocalTurn)
            {
                var unit = client.CurrentUnit;
                var skill = FirstUsableSkill(client, unit);
                if (skill == null)
                    return;
                var target = client.GetAvailableTargets(unit, skill)[0];
                RandomSolver.SetRandomSeed(seed);
                string? payload = client.ExecuteLocalSkill(skill.Id, target.CombatInfo.CombatId);
                RandomSolver.SetRandomSeed(seed);
                host.ApplyRemoteSkill(payload!);
            }
        }

        private static Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.CombatSkill? FirstUsableSkill(
            DuelController duel, Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle.ICombatUnit unit)
        {
            return unit.Character.CurrentCombatSkills!
                .FirstOrDefault(s => duel.IsSkillUsable(unit, s) && duel.GetAvailableTargets(unit, s).Count > 0);
        }

        [Test]
        public void TurnFlow_BothSides_RemainInLockstep()
        {
            int sessionSeed = 123456;

            var host = new DuelController();
            host.StartDuel(Picks("crusader"), Picks("highwayman"), sessionSeed, isHost: true);
            var client = new DuelController();
            client.StartDuel(Picks("crusader"), Picks("highwayman"), sessionSeed, isHost: false);

            RandomSolver.SetRandomSeed(sessionSeed);
            host.StartBattle();
            RandomSolver.SetRandomSeed(sessionSeed);
            client.StartBattle();

            Assert.That(host.Phase, Is.EqualTo(client.Phase));
            Assert.That(host.CurrentUnit!.CombatInfo.CombatId, Is.EqualTo(client.CurrentUnit!.CombatInfo.CombatId));

            int seed = sessionSeed;
            for (int turn = 0; turn < 12 && !host.IsFinished; turn++)
            {
                PlayTurn(host, client, seed);
                seed += 31;

                Assert.That(host.Phase, Is.EqualTo(client.Phase));
                for (int i = 0; i < host.HeroParty.Units.Count; i++)
                    Assert.That(host.HeroParty.Units[i].Character.HealthRatio,
                        Is.EqualTo(client.HeroParty.Units[i].Character.HealthRatio).Within(0.0001f));
                for (int i = 0; i < host.MonsterParty.Units.Count; i++)
                    Assert.That(host.MonsterParty.Units[i].Character.HealthRatio,
                        Is.EqualTo(client.MonsterParty.Units[i].Character.HealthRatio).Within(0.0001f));
            }

            Assert.That(host.IsFinished, Is.EqualTo(client.IsFinished));
        }
    }
}