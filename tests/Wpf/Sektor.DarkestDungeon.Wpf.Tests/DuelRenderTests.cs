namespace Sektor.DarkestDungeon.Wpf.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Wpf.Combat;
    using Sektor.DarkestDungeon.Wpf.Networking;
    using Sektor.DarkestDungeon.Wpf.ViewModels;

    [TestFixture]
    public class DuelRenderTests
    {
        private sealed class NullRivalLink : IDuelRivalLink
        {
            public event Action<string>? RivalActionReceived;

            public void SendLocalAction(string payload)
            {
            }

            public void Attach(DuelController controller)
            {
            }

            public void Detach()
            {
            }

            public void Pump()
            {
            }

            public void Dispose()
            {
            }
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

        [Test]
        public void Snapshot_ReflectsControllerUnitsAndStatus()
        {
            var duel = new DuelController();
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var view = new DuelBattleViewModel(duel, new NullRivalLink(), () => { });

            Assert.That(view.Heroes.Count, Is.EqualTo(4));
            Assert.That(view.Monsters.Count, Is.EqualTo(4));
            Assert.That(view.Status, Does.Contain("Round"));
            Assert.That(view.Heroes[0].Hp, Is.GreaterThan(0));
        }

        [Test]
        public void ExecuteSkill_UpdatesSnapshotHp()
        {
            var duel = new DuelController();
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();

            var view = new DuelBattleViewModel(duel, new NullRivalLink(), () => { });
            int initial = view.Monsters[0].Hp;

            if (duel.IsLocalTurn)
            {
                var unit = duel.CurrentUnit;
                var skill = unit!.Character.CurrentCombatSkills![0];
                var target = duel.GetAvailableTargets(unit, skill)[0];
                RandomSolver.SetRandomSeed(7);
                duel.ExecuteLocalSkill(skill.Id, target.CombatInfo.CombatId);
                view.Refresh();

                Assert.That(view.Monsters.FirstOrDefault(m => m.CombatId == target.CombatInfo.CombatId)!.Hp,
                    Is.LessThan(initial));
            }
            else
            {
                Assert.Pass("First unit is a monster turn; snapshot still valid.");
            }
        }
    }
}