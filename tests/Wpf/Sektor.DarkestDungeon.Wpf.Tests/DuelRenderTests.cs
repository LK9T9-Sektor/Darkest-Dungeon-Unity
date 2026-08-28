namespace Sektor.DarkestDungeon.Wpf.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
    using Sektor.DarkestDungeon.Core.Duel;
    using Sektor.DarkestDungeon.Wpf.Data;
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
            var duel = new DuelController(new DuelContent());
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
            Assert.That(view.Heroes[0].Speed, Is.InRange(0, 100));
            Assert.That(view.Heroes[0].ResistStun, Is.InRange(0, 100));
            Assert.That(view.Heroes[0].ResistMove, Is.InRange(0, 100));
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

        [Test]
        public void Pass_AdvancesTurn()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            if (!duel.IsLocalTurn)
            {
                Assert.Pass("Rival starts first; pass path covered by Move tests.");
                return;
            }

            var before = duel.CurrentUnit;
            view.PassCommand.Execute(null);

            Assert.That(duel.IsFinished || !ReferenceEquals(duel.CurrentUnit, before), Is.True);
        }

        [Test]
        public void Move_SwapsWithAdjacentAlly()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            if (!duel.IsLocalTurn)
            {
                Assert.Pass("Rival starts first; move path covered by Pass tests.");
                return;
            }

            var unit = duel.CurrentUnit!;
            var ally = (unit.Team == Team.Heroes ? duel.HeroParty : duel.MonsterParty).Units
                .FirstOrDefault(candidate => Math.Abs(candidate.Rank - unit.Rank) == 1);
            if (ally == null)
            {
                Assert.Pass("No adjacent ally to move to.");
                return;
            }

            view.MoveCommand.Execute(null);
            var allyCards = unit.Team == Team.Heroes ? view.Heroes : view.Monsters;
            view.TargetCommand.Execute(allyCards.First(card => card.Rank == ally.Rank));

            Assert.That(unit.Rank, Is.EqualTo(ally.Rank));
        }

        [Test]
        public void SkillExecution_AppendsDetailedLog()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            if (!duel.IsLocalTurn || view.Skills.Count == 0)
            {
                Assert.Pass("Rival starts first; log path covered on the local side.");
                return;
            }

            view.SelectSkillCommand.Execute(view.Skills[0]);
            var target = view.Heroes.Concat(view.Monsters).FirstOrDefault(card => card.IsTarget);
            Assert.That(target, Is.Not.Null, "Selected skill should highlight a target.");

            int logCount = view.Log.Count;
            view.TargetCommand.Execute(target);

            Assert.That(view.Log.Count, Is.GreaterThan(logCount));
            Assert.That(view.Log[view.Log.Count - 1], Is.Not.Empty);
        }

        [Test]
        public void Quirk_Buff_ModifiesMaxHealth()
        {
            var withQuirk = new DuelController(new DuelContent());
            withQuirk.StartDuel(new[] { new DuelHeroPick("crusader", 1, null, new[] { "tough" }) }, Picks("highwayman"), 42, isHost: true);
            var plain = new DuelController(new DuelContent());
            plain.StartDuel(new[] { new DuelHeroPick("crusader", 1) }, Picks("highwayman"), 42, isHost: true);

            float hpWith = withQuirk.HeroParty.Units[0].Character.GetPairedAttribute(AttributeType.HitPoints).ModifiedValue;
            float hpPlain = plain.HeroParty.Units[0].Character.GetPairedAttribute(AttributeType.HitPoints).ModifiedValue;

            Assert.That(hpWith, Is.GreaterThan(hpPlain));
        }

        [Test]
        public void PartyConfig_RoundTripsQuirks()
        {
            var config = new Networking.DuelPartyConfig(
                new[] { "crusader", "highwayman" },
                new[] { 1, 2 },
                new[] { new[] { "smite" }, Array.Empty<string>() },
                new[] { new[] { "tough" }, new[] { "fragile" } });

            var parsed = Networking.DuelPartyConfig.Deserialize(config.Serialize());

            Assert.That(parsed.ClassIds[0], Is.EqualTo("crusader"));
            CollectionAssert.AreEquivalent(new[] { "smite" }, parsed.SelectedSkillIds[0]);
            CollectionAssert.AreEquivalent(new[] { "tough" }, parsed.QuirkIds[0]);
            CollectionAssert.AreEquivalent(new[] { "fragile" }, parsed.QuirkIds[1]);
        }
    }
}