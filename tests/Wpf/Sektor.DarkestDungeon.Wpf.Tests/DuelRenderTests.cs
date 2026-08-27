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

        private static DuelController CreateDuel()
        {
            var duel = new DuelController();
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();
            return duel;
        }

        private static DuelBattleViewModel CreateView(DuelController duel)
        {
            return new DuelBattleViewModel(duel, new NullRivalLink(), () => { });
        }

        [Test]
        public void Snapshot_ReflectsControllerUnitsAndStatus()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            Assert.That(view.Heroes.Count, Is.EqualTo(4));
            Assert.That(view.Monsters.Count, Is.EqualTo(4));
            Assert.That(view.Status, Does.Contain("Round"));
            Assert.That(view.Heroes[0].HpCurrent, Is.GreaterThan(0));
            Assert.That(view.Heroes[0].HpMax, Is.GreaterThan(0));
        }

        [Test]
        public void ExecuteSkill_UpdatesSnapshotHp()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);
            int initial = view.Monsters[0].HpCurrent;

            if (duel.IsLocalTurn)
            {
                var unit = duel.CurrentUnit;
                var skill = unit!.Character.CurrentCombatSkills![0];
                var target = duel.GetAvailableTargets(unit, skill)[0];
                RandomSolver.SetRandomSeed(7);
                duel.ExecuteLocalSkill(skill.Id, target.CombatInfo.CombatId);
                view.Refresh();

                Assert.That(view.Monsters.FirstOrDefault(m => m.CombatId == target.CombatInfo.CombatId)!.HpCurrent,
                    Is.LessThan(initial));
            }
            else
            {
                Assert.Pass("First unit is a monster turn; snapshot still valid.");
            }
        }

        [Test]
        public void Hover_SetsTooltipAndSelection()
        {
            var view = CreateView(CreateDuel());
            var card = view.Heroes[0];

            view.HoverCommand.Execute(card);

            Assert.That(view.TooltipTarget, Is.SameAs(card));
            Assert.That(card.IsSelected, Is.True);

            view.UnhoverCommand.Execute(null);

            Assert.That(view.TooltipTarget, Is.Null);
            Assert.That(card.IsSelected, Is.False);
        }

        [Test]
        public void OpenStats_FillsSheetAndShows()
        {
            var view = CreateView(CreateDuel());
            var card = view.Heroes[0];

            view.OpenStatsCommand.Execute(card);

            Assert.That(view.IsStatsVisible, Is.True);
            Assert.That(view.StatsTarget.HeroName, Is.EqualTo(card.Name));
            Assert.That(view.StatsTarget.HitPoints, Is.EqualTo(card.HpCurrent + " / " + card.HpMax));

            view.CloseStatsCommand.Execute(null);

            Assert.That(view.IsStatsVisible, Is.False);
        }

        [Test]
        public void Round_ReflectsController()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            Assert.That(view.Events.Round, Is.EqualTo(duel.BattleGround!.Round.RoundNumber));
        }

        [Test]
        public void Actor_ReflectsCurrentUnit()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            Assert.That(view.RaidHud.Hero.Name, Is.EqualTo(duel.CurrentUnit?.Character.Name));
            Assert.That(view.Quest.Goal, Is.EqualTo(view.Status));
        }
    }
}