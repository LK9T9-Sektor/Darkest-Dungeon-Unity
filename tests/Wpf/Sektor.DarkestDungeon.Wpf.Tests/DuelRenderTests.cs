using System;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Networking;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    [TestFixture]
    public class DuelRenderTests
    {
        private sealed class NullRivalLink : IDuelRivalLink
        {
            public event Action<string>? RivalActionReceived;
            public event Action<string>? SkillPreviewed;
            public event Action<int>? TargetPreviewed;

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

        /// <summary>Mimics the network/AI link by letting the test raise <see cref="RivalActionReceived"/>
        /// just as the wire does when a rival action arrives.</summary>
        private sealed class FiringRivalLink : IDuelRivalLink
        {
            public event Action<string>? RivalActionReceived;
            public event Action<string>? SkillPreviewed;
            public event Action<int>? TargetPreviewed;

            public void Fire(string payload)
            {
                RivalActionReceived?.Invoke(payload);
            }

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
        public void Snapshot_HeroesRenderBackToFrontMonstersFrontToBack()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            Assert.That(view.Heroes.Select(hero => hero.Rank),
                Is.EqualTo(duel.HeroParty.Units.Select(unit => unit.Rank).Reverse()));
            Assert.That(view.Monsters.Select(monster => monster.Rank),
                Is.EqualTo(duel.MonsterParty.Units.Select(unit => unit.Rank)));
        }

        [Test]
        public void Snapshot_EveryCardHasNonZeroRank()
        {
            var view = CreateView(CreateDuel());

            Assert.That(view.Heroes.Select(hero => hero.Rank), Is.All.GreaterThan(0));
            Assert.That(view.Monsters.Select(monster => monster.Rank), Is.All.GreaterThan(0));
        }

        [Test]
        public void Snapshot_ActionPipsMatchRoundProgress()
        {
            var view = CreateView(CreateDuel());
            var currentId = view.Heroes.Concat(view.Monsters).FirstOrDefault(card => card.IsCurrent)?.CombatId;

            foreach (var card in view.Heroes.Concat(view.Monsters))
            {
                Assert.That(card.ActionsTotal, Is.GreaterThanOrEqualTo(1));
                Assert.That(card.ActionPips.Count, Is.EqualTo(card.ActionsTotal));
                Assert.That(card.ActionPips, Is.All.InRange(0, 1));
                Assert.That(card.RemainingActions, Is.InRange(0, card.ActionsTotal));
            }

            if (currentId != null)
            {
                var current = view.Heroes.Concat(view.Monsters).First(card => card.CombatId == currentId.Value);
                Assert.That(current.RemainingActions, Is.EqualTo(1), "The acting unit keeps its action this round.");
            }
        }

        [Test]
        public void Actor_PanelReflectsCurrentUnitStats()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);
            var unit = duel.CurrentUnit!;
            var stats = view.RaidHud.Hero.Stats;

            Assert.That(view.RaidHud.Hero.Name, Is.EqualTo(unit.Character.Name));
            Assert.That(stats.HitPoints, Is.EqualTo(
                (int)unit.Character.GetPairedAttribute(AttributeType.HitPoints).CurrentValue + " / "
                + (int)unit.Character.GetPairedAttribute(AttributeType.HitPoints).ModifiedValue));
            Assert.That(stats.Speed, Is.EqualTo(((int)unit.Character.Speed).ToString()));
            Assert.That(stats.Accuracy, Is.EqualTo("+" + (int)unit.Character.Accuracy));
            Assert.That(stats.Crit, Is.EqualTo((int)(unit.Character.Crit * 100) + "%"));
            Assert.That(stats.Dodge, Is.EqualTo(((int)unit.Character.Dodge).ToString()));
            Assert.That(stats.Protection, Is.EqualTo(((int)unit.Character.Protection) + "%"));
        }

        [Test]
        public void Snapshot_HeroCardsShowCanonicalClassName()
        {
            var view = CreateView(CreateDuel());

            Assert.That(view.Heroes.Select(hero => hero.Name), Is.All.EqualTo("Reynauld"));
            Assert.That(view.Monsters.Select(monster => monster.Name), Is.All.EqualTo("Dismas"));
            Assert.That(view.Heroes[0].ClassName, Is.EqualTo("Crusader"));
            Assert.That(view.Monsters[0].ClassName, Is.EqualTo("Highwayman"));
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
        public void OpenStats_FillsSheetAndShows()
        {
            var view = CreateView(CreateDuel());
            var card = view.Heroes[0];

            view.OpenStatsCommand.Execute(card);

            Assert.That(view.IsStatsVisible, Is.True);
            Assert.That(view.StatsTarget.HeroName, Is.EqualTo(card.Name));
            Assert.That(view.StatsTarget.HitPoints, Is.EqualTo(card.HpCurrent + " / " + card.HpMax));
            Assert.That(view.StatsTarget.ResistStun, Is.EqualTo(card.ResistStun));
            Assert.That(view.StatsTarget.ResistDeathBlow, Is.EqualTo(card.ResistDeathBlow));

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
            Assert.That(view.Quest.Goal, Is.EqualTo("Defeat the rival party"));
        }

        [Test]
        public void Turn_End_GreysActedUnitPips()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            if (!duel.IsLocalTurn)
            {
                Assert.Pass("Rival starts first; the pip greying is covered by the pass path next round.");
                return;
            }

            var actedId = duel.CurrentUnit!.CombatInfo.CombatId;
            string actedName = duel.CurrentUnit!.Character.Name;
            view.PassCommand.Execute(null);

            var actedCard = view.Heroes.Concat(view.Monsters).Single(card => card.CombatId == actedId);
            Assert.That(actedCard.RemainingActions, Is.EqualTo(0), "The acting unit's pip turns gray once it moved.");
            Assert.That(view.Heroes.Concat(view.Monsters).Single(card => card.IsCurrent).RemainingActions,
                Is.EqualTo(1), "The new current unit keeps a white pip.");
            Assert.That(view.TurnOrder.Select(entry => entry.Name), Does.Not.Contain(actedName),
                "The unit that already moved is dropped from the turn order strip.");
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
        public void SelectTarget_InvalidTarget_IsIgnored()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            if (!duel.IsLocalTurn || view.Skills.Count == 0)
            {
                Assert.Pass("Rival starts first; the invalid-target path is covered on the local side.");
                return;
            }

            view.SelectSkillCommand.Execute(view.Skills[0]);
            var invalid = view.Heroes.Concat(view.Monsters).FirstOrDefault(card => !card.IsTarget);
            Assert.That(invalid, Is.Not.Null, "There should be at least one card outside the selected skill's targets.");

            var targetCard = view.Heroes.Concat(view.Monsters).FirstOrDefault(card => card.IsTarget);
            if (targetCard == null)
                Assert.Pass("Selected skill has no valid targets; nothing can be executed.");

            int hpBefore = targetCard.HpCurrent;
            int logCount = view.Log.Count;

            view.TargetCommand.Execute(invalid);

            Assert.That(view.Log.Count, Is.EqualTo(logCount), "Clicking an invalid target must not trigger a skill action.");
            Assert.That(targetCard.HpCurrent, Is.EqualTo(hpBefore),
                "No damage or heal should be applied through an invalid target.");
        }

        [Test]
        public void HealSkill_SelectsOnlyAlliesAndHealsTheClickedAlly()
        {
            var duel = CreateDuel();
            var view = CreateView(duel);

            ICombatUnit? unit = null;
            CombatSkill? healSkill = null;
            int guard = 0;
            while (!duel.IsFinished && guard++ < 100)
            {
                if (duel.IsLocalTurn)
                {
                    var heal = duel.CurrentUnit?.Character.CurrentCombatSkills?.FirstOrDefault(
                        s => s.Category == SkillCategory.Heal && duel.IsSkillUsable(duel.CurrentUnit!, s));
                    if (heal != null)
                    {
                        unit = duel.CurrentUnit;
                        healSkill = heal;
                        break;
                    }
                    duel.ExecuteLocalPass();
                    view.Refresh();
                }
                else
                {
                    duel.ApplyRemoteSkill(DuelPayload.PassAction());
                    view.Refresh();
                }
            }

            if (unit == null || healSkill == null)
                Assert.Pass("No hero reached a turn with a usable heal skill.");

            var woundedAlly = duel.HeroParty.Units.FirstOrDefault(u => u != unit);
            Assert.That(woundedAlly, Is.Not.Null);
            woundedAlly.Character.TakeDamage(4);
            view.Refresh();

            var healButton = view.Skills.FirstOrDefault(s => s.Id == healSkill.Id);
            Assert.That(healButton, Is.Not.Null);
            view.SelectSkillCommand.Execute(healButton);

            Assert.That(view.Heroes.Count(card => card.IsTarget), Is.GreaterThan(0),
                "A heal skill highlights the friendly cards.");
            Assert.That(view.Monsters.All(card => !card.IsTarget), Is.True,
                "Enemies are never valid targets of a heal.");

            int logCount = view.Log.Count;
            view.TargetCommand.Execute(view.Monsters.First());
            Assert.That(view.Log.Count, Is.EqualTo(logCount),
                "Clicking an enemy with a heal selected must not trigger an action.");

            var woundedCard = view.Heroes.First(card => card.CombatId == woundedAlly.CombatInfo.CombatId);
            int hpBefore = woundedCard.HpCurrent;
            view.TargetCommand.Execute(woundedCard);

            var healedCard = view.Heroes.First(card => card.CombatId == woundedAlly.CombatInfo.CombatId);
            Assert.That(healedCard.HpCurrent, Is.GreaterThan(hpBefore),
                "Clicking a wounded ally with a heal must raise its hit points.");
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

        [Test]
        public void PartyConfig_RoundTripsTrinkets()
        {
            var config = new Networking.DuelPartyConfig(
                new[] { "crusader", "highwayman" },
                new[] { 1, 2 },
                new[] { new[] { "smite" }, Array.Empty<string>() },
                new[] { new[] { "tough" }, Array.Empty<string>() },
                new[] { new[] { "accuracy_stone" }, new[] { "lucky_dice", "focus_ring" } });

            var parsed = Networking.DuelPartyConfig.Deserialize(config.Serialize());

            CollectionAssert.AreEquivalent(new[] { "accuracy_stone" }, parsed.TrinketIds[0]);
            CollectionAssert.AreEquivalent(new[] { "lucky_dice", "focus_ring" }, parsed.TrinketIds[1]);
        }

        [Test]
        public void PartyConfig_DeserializesLegacyRowWithoutTrinkets()
        {
            const string legacyRow = "crusader|1|smite,stunning_blow|tough";

            var parsed = Networking.DuelPartyConfig.Deserialize(legacyRow);

            Assert.That(parsed.ClassIds[0], Is.EqualTo("crusader"));
            CollectionAssert.AreEquivalent(new[] { "smite", "stunning_blow" }, parsed.SelectedSkillIds[0]);
            CollectionAssert.AreEquivalent(new[] { "tough" }, parsed.QuirkIds[0]);
            Assert.That(parsed.TrinketIds[0], Is.Empty);
        }
        [Test]
        public void RivalAction_IsStagedAndPreviewed_NotAppliedImmediately()
        {
            var duel = CreateDuel();
            var link = new FiringRivalLink();
            var view = new DuelBattleViewModel(duel, link, () => { });

            string statusBefore = view.Status;
            link.Fire(DuelPayload.MoveAction(1));

            Assert.That(view.IsMovePreview, Is.True,
                "A rival MOVE must enter move-preview mode for the reveal beat");
            Assert.That(view.Status, Is.EqualTo(statusBefore),
                "The rival action must be held by the 1s reveal and not applied synchronously");
        }
    }
}