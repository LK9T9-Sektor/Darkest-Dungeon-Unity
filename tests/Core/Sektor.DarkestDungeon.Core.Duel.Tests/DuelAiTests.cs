namespace Sektor.DarkestDungeon.Core.Duel.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

    [TestFixture]
    public class DuelAiTests
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

        [Test]
        public void Ai_ChoosesActions_BothSidesRemainInLockstep()
        {
            int sessionSeed = 123456;
            var content = new TestDuelContent();
            var ai = new DuelAi();

            var host = new DuelController(content);
            host.StartDuel(Picks("crusader"), Picks("highwayman"), sessionSeed, isHost: true);
            var client = new DuelController(content);
            client.StartDuel(Picks("crusader"), Picks("highwayman"), sessionSeed, isHost: false);

            RandomSolver.SetRandomSeed(sessionSeed);
            host.StartBattle();
            RandomSolver.SetRandomSeed(sessionSeed);
            client.StartBattle();

            for (int turn = 0; turn < 8 && !host.IsFinished; turn++)
            {
                if (host.IsLocalTurn)
                {
                    PlayHostTurn(host, client, 1000 + turn);
                }
                else if (client.IsLocalTurn)
                {
                    string payload = ai.ChooseAction(client);
                    RandomSolver.SetRandomSeed(2000 + turn);
                    ExecuteLocally(client, payload);
                    RandomSolver.SetRandomSeed(2000 + turn);
                    host.ApplyRemoteSkill(payload);
                }

                Assert.That(host.Phase, Is.EqualTo(client.Phase));
                for (int i = 0; i < host.HeroParty.Units.Count; i++)
                    Assert.That(host.HeroParty.Units[i].Character.HealthRatio,
                        Is.EqualTo(client.HeroParty.Units[i].Character.HealthRatio).Within(0.0001f));
                for (int i = 0; i < host.MonsterParty.Units.Count; i++)
                    Assert.That(host.MonsterParty.Units[i].Character.HealthRatio,
                        Is.EqualTo(client.MonsterParty.Units[i].Character.HealthRatio).Within(0.0001f));
            }
        }

        private static void PlayHostTurn(DuelController host, DuelController client, int seed)
        {
            var unit = host.CurrentUnit;
            var skill = FirstUsableSkill(host, unit);
            if (skill == null)
            {
                host.ExecuteLocalPass();
                client.ApplyRemoteSkill(DuelPayload.PassAction());
                return;
            }

            var target = host.GetAvailableTargets(unit, skill)[0];
            RandomSolver.SetRandomSeed(seed);
            string payload = host.ExecuteLocalSkill(skill.Id, target.CombatInfo.CombatId);
            RandomSolver.SetRandomSeed(seed);
            client.ApplyRemoteSkill(payload);
        }

        private static void ExecuteLocally(DuelController duel, string payload)
        {
            var parts = payload.Split('|');
            if (parts[0] == DuelPayload.Pass)
            {
                duel.ExecuteLocalPass();
                return;
            }
            if (parts[0] == DuelPayload.Move)
            {
                duel.ExecuteLocalMove(int.Parse(parts[1]));
                return;
            }
            duel.ExecuteLocalSkill(parts[0], int.Parse(parts[1]));
        }

        private static CombatSkill FirstUsableSkill(
            DuelController duel, Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle.ICombatUnit unit)
        {
            return unit.Character.CurrentCombatSkills
                .FirstOrDefault(s => duel.IsSkillUsable(unit, s) && duel.GetAvailableTargets(unit, s).Count > 0);
        }
    }
}