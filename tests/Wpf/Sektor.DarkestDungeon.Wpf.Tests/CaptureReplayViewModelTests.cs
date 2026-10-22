using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.GameEvents;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the recorded-capture replay projection, timeline pacing and final state.</summary>
    [TestFixture]
    public class CaptureReplayViewModelTests
    {
        /// <summary>The reference capture loads and stops at the first recorded attack.</summary>
        [Test]
        public void Constructor_LoadsReferenceCaptureAndWaitsForFirstAttack()
        {
            CaptureReplayViewModel viewModel = CreateViewModel(out FakeLogger logger);

            Assert.That(viewModel.TotalEvents, Is.EqualTo(38));
            Assert.That(viewModel.Timeline, Has.Count.EqualTo(38));
            Assert.That(viewModel.CaptureSubtitle, Does.Contain("auto-recorded raid run").And.Contain("38"));
            Assert.That(viewModel.SceneName, Is.EqualTo("Dungeon"));
            Assert.That(viewModel.RoomName, Is.EqualTo("3"));
            Assert.That(viewModel.Torch, Is.EqualTo(82));
            Assert.That(viewModel.Party.Select(unit => unit.Id), Is.EqualTo(new[] { "hero_1", "hero_2" }));
            Assert.That(viewModel.PartyFormation.Select(unit => unit.Id), Is.EqualTo(new[] { "hero_2", "hero_1" }));
            Assert.That(viewModel.Enemies.Select(unit => unit.Id), Is.EqualTo(new[] { "monster_3" }));

            CaptureUnitViewModel attacker = Unit(viewModel, "hero_2");
            Assert.That(attacker.IsAttacker, Is.True);
            Assert.That(attacker.ContentDirectory, Is.EqualTo(Path.Combine("root", "highwayman", "attack_slice")));
            Assert.That(attacker.Loop, Is.False);

            CaptureUnitViewModel target = Unit(viewModel, "monster_3");
            Assert.That(target.IsTarget, Is.True);
            Assert.That(target.IsCard, Is.True);
            Assert.That(target.Loop, Is.True);
            Assert.That(viewModel.CurrentEventNumber, Is.EqualTo(10));
            Assert.That(viewModel.IsAwaitingAnimation, Is.True);
            Assert.That(viewModel.IsFinished, Is.False);
            Assert.That(viewModel.IsPlaying, Is.True);
            Assert.That(viewModel.ReplayCommand.CanExecute(null), Is.False);
            Assert.That(viewModel.Timeline[9].IsCurrent, Is.True);
            Assert.That(viewModel.Timeline[9].Label, Does.Contain("hero_2 → monster_3"));

            Assert.That(logger.Messages, Has.Some.Contains("capture replay: loaded"));
            Assert.That(logger.Messages, Has.Some.Contains("capture replay: started"));
        }

        /// <summary>A schema 1.1 capture projects stress, statuses, rank moves, battle and audio events.</summary>
        [Test]
        public void CurrentSchemaCapture_ProjectsRanksStressStatusesBattleAndAudio()
        {
            string directory = CreateCurrentSchemaCapture();
            try
            {
                var viewModel = new CaptureReplayViewModel(
                    new FakeNavigationService(),
                    new GameEventLogReader(),
                    new FakeCaptureAnimationCatalog(),
                    new FakeLogger(),
                    directory,
                    new ImmediateReplayPacer());

                Assert.That(viewModel.IsFinished, Is.True);
                Assert.That(viewModel.TotalEvents, Is.EqualTo(10));
                Assert.That(viewModel.Round, Is.EqualTo(2));
                Assert.That(viewModel.Turn, Is.EqualTo("Monster"));
                Assert.That(viewModel.BattleWinner, Is.EqualTo("ПОБЕДА ГЕРОЕВ"));
                Assert.That(viewModel.Party.Select(unit => unit.Id), Is.EqualTo(new[] { "hero_2", "hero_1" }));
                Assert.That(viewModel.PartyFormation.Select(unit => unit.Id), Is.EqualTo(new[] { "hero_1", "hero_2" }));

                CaptureUnitViewModel hero = Unit(viewModel, "hero_2");
                Assert.That(hero.Rank, Is.EqualTo(1));
                Assert.That(hero.StressText, Is.EqualTo("25/100"));
                Assert.That(hero.StatusText, Does.Contain("Bleeding 2").And.Contain("damage"));
                Assert.That(Unit(viewModel, "hero_1").ConditionText, Is.EqualTo("СМЕРТНЫЙ ПОРОГ"));

                CaptureUnitViewModel monster = Unit(viewModel, "monster_3");
                Assert.That(monster.HpText, Is.EqualTo("4/12"));
                Assert.That(monster.StressText, Is.EqualTo("31"));

                Assert.That(viewModel.AudioLog, Has.Count.EqualTo(1));
                Assert.That(viewModel.AudioLog[0].Label, Does.Contain("event:/char/hero/highwayman/attack"));
                Assert.That(viewModel.Timeline, Has.Count.EqualTo(10));
                Assert.That(viewModel.Timeline[9].Label, Does.Contain("Бой окончен"));
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        /// <summary>Completing every attack plays the whole capture and applies the trailing snapshot.</summary>
        [Test]
        public void CompletingAllAttacks_ReachesTheTrailingSnapshotState()
        {
            CaptureReplayViewModel viewModel = CreateViewModel(out FakeLogger logger);

            CompleteAttacks(viewModel);

            Assert.That(viewModel.IsFinished, Is.True);
            Assert.That(viewModel.IsPlaying, Is.False);
            Assert.That(viewModel.IsAwaitingAnimation, Is.False);
            Assert.That(viewModel.CurrentEventNumber, Is.EqualTo(38));
            Assert.That(viewModel.Torch, Is.EqualTo(7));
            Assert.That(viewModel.Gold, Is.EqualTo(1000));
            Assert.That(viewModel.RoomName, Is.EqualTo("room2_1"));
            Assert.That(viewModel.RoomType, Is.EqualTo("BattleTresure"));
            Assert.That(viewModel.StatusText, Is.EqualTo("Готово: 38 событий"));

            Assert.That(viewModel.Party, Has.Count.EqualTo(2));
            Assert.That(Hp(viewModel, "hero_1"), Is.EqualTo("27/33"));
            Assert.That(Hp(viewModel, "hero_2"), Is.EqualTo("22/22"));

            Assert.That(viewModel.Enemies.Select(unit => unit.Id), Is.EqualTo(new[] { "monster_3", "monster_4" }));
            Assert.That(Hp(viewModel, "monster_3"), Is.EqualTo("35/35"));
            Assert.That(Hp(viewModel, "monster_4"), Is.EqualTo("12/12"));

            Assert.That(viewModel.Timeline[37].IsCurrent, Is.True);
            Assert.That(viewModel.Timeline[36].IsPast, Is.True);
            Assert.That(logger.Messages, Has.Some.Contains("capture replay: finished"));
        }

        /// <summary>Replaying resets the projected state and stops at the first attack again.</summary>
        [Test]
        public void ReplayCommand_RestartsFromTheFirstAttack()
        {
            CaptureReplayViewModel viewModel = CreateViewModel(out _);
            CompleteAttacks(viewModel);

            viewModel.ReplayCommand.Execute(null);

            Assert.That(viewModel.IsFinished, Is.False);
            Assert.That(viewModel.IsAwaitingAnimation, Is.True);
            Assert.That(viewModel.CurrentEventNumber, Is.EqualTo(10));
            Assert.That(viewModel.Torch, Is.EqualTo(82));
            Assert.That(viewModel.Party, Has.Count.EqualTo(2));
            Assert.That(viewModel.Enemies.Select(unit => unit.Id), Is.EqualTo(new[] { "monster_3" }));
            Assert.That(Unit(viewModel, "hero_2").IsAttacker, Is.True);
            Assert.That(Unit(viewModel, "monster_3").IsTarget, Is.True);
            Assert.That(viewModel.Timeline[9].IsCurrent, Is.True);
            Assert.That(viewModel.Timeline[0].IsCurrent, Is.False);
        }

        /// <summary>Pacing slows the event stream and holds every attack pose for a visible duration.</summary>
        [Test]
        public void Pacing_AppliesEventDelayAndMinimumAttackHold()
        {
            var pacer = new RecordingReplayPacer();
            CaptureReplayViewModel viewModel = CreateViewModel(out _, new FakeCaptureAnimationCatalog(), pacer);

            Assert.That(pacer.LastDelay, Is.EqualTo(TimeSpan.FromMilliseconds(100)));

            int guard = 0;
            while (!viewModel.IsAwaitingAnimation && pacer.RunNext())
                Assert.That(++guard, Is.LessThan(100), "The replay did not reach the first attack.");

            Assert.That(viewModel.IsAwaitingAnimation, Is.True);
            viewModel.AnimationCompletedCommand.Execute(null);
            Assert.That(pacer.LastDelay, Is.GreaterThan(TimeSpan.FromSeconds(1)));
        }

        /// <summary>Pausing stops the pending event, and resuming schedules it again.</summary>
        [Test]
        public void Pause_StopsAndResumesEventPacing()
        {
            var pacer = new RecordingReplayPacer();
            CaptureReplayViewModel viewModel = CreateViewModel(out _, new FakeCaptureAnimationCatalog(), pacer);
            int before = viewModel.CurrentEventNumber;

            viewModel.TogglePauseCommand.Execute(null);

            Assert.That(viewModel.IsPaused, Is.True);
            Assert.That(pacer.RunNext(), Is.False);
            Assert.That(viewModel.CurrentEventNumber, Is.EqualTo(before));

            viewModel.TogglePauseCommand.Execute(null);

            Assert.That(viewModel.IsPaused, Is.False);
            Assert.That(pacer.RunNext(), Is.True);
            Assert.That(viewModel.CurrentEventNumber, Is.GreaterThan(before));
        }

        /// <summary>An animation without linked content falls back to the idle set instead of failing.</summary>
        [Test]
        public void UnknownAnimation_FallsBackToIdle()
        {
            CaptureReplayViewModel viewModel = CreateViewModel(out _, new IdleOnlyCatalog());

            CaptureUnitViewModel attacker = Unit(viewModel, "hero_2");
            Assert.That(attacker.ContentDirectory, Is.EqualTo(Path.Combine("root", "highwayman", "Idle")));
            Assert.That(attacker.Loop, Is.False);
            Assert.That(Unit(viewModel, "monster_3").IsCard, Is.True);

            CompleteAttacks(viewModel);

            Assert.That(viewModel.IsFinished, Is.True);
            Assert.That(viewModel.Torch, Is.EqualTo(7));
        }

        /// <summary>A missing capture directory surfaces an error instead of throwing.</summary>
        [Test]
        public void MissingCapture_ShowsErrorAndDisablesReplay()
        {
            var viewModel = new CaptureReplayViewModel(
                new FakeNavigationService(),
                new GameEventLogReader(),
                new FakeCaptureAnimationCatalog(),
                new FakeLogger(),
                Path.Combine(Path.GetTempPath(), "missing-capture-" + Guid.NewGuid().ToString("N")),
                new ImmediateReplayPacer());

            Assert.That(viewModel.ErrorText, Does.Contain("Manifest not found"));
            Assert.That(viewModel.StatusText, Is.EqualTo("Захват недоступен"));
            Assert.That(viewModel.TotalEvents, Is.EqualTo(0));
            Assert.That(viewModel.ReplayCommand.CanExecute(null), Is.False);
            Assert.That(viewModel.IsAwaitingAnimation, Is.False);
        }

        private static void CompleteAttacks(CaptureReplayViewModel viewModel)
        {
            int guard = 0;
            while (viewModel.IsAwaitingAnimation && guard++ < 100)
                viewModel.AnimationCompletedCommand.Execute(null);

            Assert.That(guard, Is.LessThan(100), "The replay did not reach the end of the capture.");
        }

        private static string Hp(CaptureReplayViewModel viewModel, string unitId)
        {
            return Unit(viewModel, unitId).HpText;
        }

        private static CaptureUnitViewModel Unit(CaptureReplayViewModel viewModel, string unitId)
        {
            CaptureUnitViewModel? unit = viewModel.Party
                .Concat(viewModel.Enemies)
                .SingleOrDefault(candidate => candidate.Id == unitId);
            Assert.That(unit, Is.Not.Null, $"Unit '{unitId}' is not shown in the replay.");
            return unit!;
        }

        private static CaptureReplayViewModel CreateViewModel(out FakeLogger logger)
            => CreateViewModel(out logger, new FakeCaptureAnimationCatalog());

        private static CaptureReplayViewModel CreateViewModel(out FakeLogger logger, ICaptureAnimationCatalog catalog)
            => CreateViewModel(out logger, catalog, new ImmediateReplayPacer());

        private static CaptureReplayViewModel CreateViewModel(
            out FakeLogger logger,
            ICaptureAnimationCatalog catalog,
            IReplayPacer pacer)
        {
            logger = new FakeLogger();
            return new CaptureReplayViewModel(
                new FakeNavigationService(),
                new GameEventLogReader(),
                catalog,
                logger,
                FindReferenceCapture(),
                pacer);
        }

        private static string CreateCurrentSchemaCapture()
        {
            string directory = Path.Combine(Path.GetTempPath(), "capture-current-" + Guid.NewGuid().ToString("N"));
            var bus = new GameEventBus();
            using (var logger = new JsonEventLogger(bus, directory, notes: "schema 1.1 capture"))
            {
                var statuses = new List<CombatantStatusState>
                {
                    new CombatantStatusState("Bleeding", true, 2, 1),
                };
                var buffs = new List<CombatantBuffState>
                {
                    new CombatantBuffState("buff_1", "damage", 0.15f, 3, "Combat", "skill", true),
                };
                var heroOne = new CombatantState(
                    "hero_1", "crusader", 33, 33, false, "crusader_portrait", "crusader",
                    Rank: 1, Stress: 4, MaxStress: 100, AtDeathsDoor: true);
                var heroTwo = new CombatantState(
                    "hero_2", "highwayman", 22, 22, false, "highwayman_portrait", "highwayman",
                    Rank: 2, Stress: 17, MaxStress: 100, Statuses: statuses, Buffs: buffs);
                var monster = new CombatantState(
                    "monster_3", "brigand_cutthroat", 12, 12, true, "brigand_portrait", "brigand_cutthroat",
                    Rank: 1);
                var heroes = new List<CombatantState> { heroOne, heroTwo };
                var combatants = new List<CombatantState> { heroOne, heroTwo, monster };

                bus.Publish(new SceneSnapshot(
                    "Dungeon",
                    Array.Empty<SceneNode>(),
                    new SceneGlobals(82, 500),
                    heroes,
                    new[] { "hero_1", "hero_2" }));
                bus.Publish(new StateSnapshot(
                    StateSnapshotContexts.BattleStart,
                    new SceneGlobals(82, 500),
                    combatants,
                    SubsystemNames.Combat,
                    Battle: new BattleState(1, "Hero", "hero_1", new[] { "hero_2", "monster_3" })));
                bus.Publish(new SkillResolved(
                    "hero_2",
                    "slice",
                    new[] { new SkillTargetResult("monster_3", "Crit", 8, true, false, true, 4, 31, 1) },
                    new[] { "damage" },
                    new GameInput(GameInputKinds.PlayerClick, Target: "monster_3", Skill: "slice"),
                    new GameRng(Seed: 42, Roll: 0.03f, Crit: true, Modifier: 0.15f)));
                bus.Publish(new UnitMoved("hero_2", 2, 1, "skill", "hero_1"));
                bus.Publish(new UnitMoved("hero_1", 1, 2, "skill", "hero_2"));
                bus.Publish(new HeroStatChanged("hero_2", "stress", 17, 25, "skill"));
                bus.Publish(new AudioPlayed(
                    "event:/char/hero/highwayman/attack",
                    AudioEventKinds.Sfx,
                    AudioActions.OneShot,
                    Parameters: new[] { new AudioParameter("material", 1) }));
                bus.Publish(new RoundStarted(2, new[] { "hero_1", "hero_2", "monster_3" }));
                bus.Publish(new TurnStarted("monster_3", 2, "Monster"));
                bus.Publish(new BattleEnded(BattleWinners.Heroes, 2));
            }

            return directory;
        }

        private static string FindReferenceCapture()
        {
            DirectoryInfo? directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "docs", "spec_game_events", "examples");
                if (Directory.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("docs/spec_game_events/examples was not found above the test directory.");
        }

        private sealed class FakeCaptureAnimationCatalog : ICaptureAnimationCatalog
        {
            public string? GetAnimationDirectory(AnimationRef? animation)
            {
                if (animation == null || !IsHero(animation.Skeleton))
                    return null;

                return Path.Combine("root", animation.Skeleton, animation.Animation);
            }

            public string? GetIdleDirectory(string skeleton)
                => IsHero(skeleton) ? Path.Combine("root", skeleton, "Idle") : null;

            private static bool IsHero(string skeleton)
                => skeleton == "crusader" || skeleton == "highwayman";
        }

        private sealed class IdleOnlyCatalog : ICaptureAnimationCatalog
        {
            public string? GetAnimationDirectory(AnimationRef? animation) => null;

            public string? GetIdleDirectory(string skeleton)
                => skeleton == "crusader" || skeleton == "highwayman"
                    ? Path.Combine("root", skeleton, "Idle")
                    : null;
        }

        private sealed class ImmediateReplayPacer : IReplayPacer
        {
            public void Schedule(TimeSpan delay, Action callback) => callback();

            public void Cancel()
            {
            }
        }

        private sealed class RecordingReplayPacer : IReplayPacer
        {
            private Queue<Action> pending = new Queue<Action>();

            public TimeSpan LastDelay { get; private set; }

            public void Schedule(TimeSpan delay, Action callback)
            {
                LastDelay = delay;
                pending.Enqueue(callback);
            }

            public void Cancel() => pending = new Queue<Action>();

            public bool RunNext()
            {
                if (pending.Count == 0)
                    return false;

                pending.Dequeue()();
                return true;
            }
        }

        private sealed class FakeLogger : ILogger
        {
            public List<string> Messages { get; } = new List<string>();

            public void Log(string message) => Messages.Add(message);

            public void Warn(string message) => Messages.Add("WARN " + message);
        }

        private sealed class FakeNavigationService : INavigationService
        {
            public void NavigateTo(object screen)
            {
            }

            public void GoHome()
            {
            }
        }
    }
}
