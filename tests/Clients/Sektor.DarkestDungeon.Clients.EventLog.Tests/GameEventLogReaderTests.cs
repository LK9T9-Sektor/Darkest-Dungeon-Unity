using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Clients.EventLog.Tests
{
    /// <summary>Verifies the JSONL reader: reference capture, allowlist binder and contract validation.</summary>
    [TestFixture]
    public class GameEventLogReaderTests
    {
        private string runDirectory;

        [SetUp]
        public void SetUp()
        {
            runDirectory = Path.Combine(Path.GetTempPath(), "eventlog-reader-test-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (runDirectory != null && Directory.Exists(runDirectory))
                Directory.Delete(runDirectory, recursive: true);
        }

        /// <summary>The checked-in reference capture is read completely and ordered by sequence.</summary>
        [Test]
        public void Read_ReferenceCapture_ReturnsAllEventsInSequence()
        {
            var reader = new GameEventLogReader();

            Result<GameEventCapture> result = reader.Read(FindReferenceCapture());

            Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
            Assert.That(result.Value.Manifest.SchemaVersion, Is.EqualTo("1.0.0"));
            Assert.That(result.Value.Manifest.Files, Has.Count.EqualTo(3));
            Assert.That(result.Value.Events, Has.Count.EqualTo(38));
            Assert.That(result.Value.Events.Select(gameEvent => gameEvent.Sequence), Is.Ordered.And.EqualTo(Enumerable.Range(1, 38)));
            Assert.That(result.Value.Events.OfType<AttackExecuted>(), Has.Exactly(6).Items);
        }

        /// <summary>The current schema restores the 1.1 combat and audio payloads through the allowlist binder.</summary>
        [Test]
        public void Read_CurrentSchemaCapture_RestoresCombatAndAudioPayloads()
        {
            CreateCurrentSchemaRun();

            Result<GameEventCapture> result = new GameEventLogReader().Read(runDirectory);

            Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
            Assert.That(result.Value.Manifest.SchemaVersion, Is.EqualTo(JsonEventLogger.CurrentSchemaVersion));
            Assert.That(result.Value.Events, Has.Count.EqualTo(10));

            var snapshot = (StateSnapshot)result.Value.Events.Single(gameEvent => gameEvent is StateSnapshot);
            Assert.That(snapshot.Battle, Is.Not.Null);
            Assert.That(snapshot.Battle!.Round, Is.EqualTo(1));
            Assert.That(snapshot.Battle.Turn, Is.EqualTo("Hero"));
            Assert.That(snapshot.Combatants[0].Rank, Is.EqualTo(2));
            Assert.That(snapshot.Combatants[0].Stress, Is.EqualTo(17));
            Assert.That(snapshot.Combatants[0].Statuses, Has.Count.EqualTo(1));
            Assert.That(snapshot.Combatants[0].Statuses![0].Id, Is.EqualTo("Bleeding"));
            Assert.That(snapshot.Combatants[0].Buffs, Has.Count.EqualTo(1));
            Assert.That(snapshot.Combatants[0].Buffs![0].Attribute, Is.EqualTo("damage"));

            var skill = result.Value.Events.OfType<SkillResolved>().Single();
            Assert.That(skill.AttackerId, Is.EqualTo("hero_2"));
            Assert.That(skill.Targets, Has.Count.EqualTo(1));
            Assert.That(skill.Targets[0].HpAfter, Is.EqualTo(4));
            Assert.That(skill.Targets[0].StressAfter, Is.EqualTo(31));
            Assert.That(skill.Rng!.Crit, Is.True);

            var moved = result.Value.Events.OfType<UnitMoved>().Single();
            Assert.That(moved.ToRank, Is.EqualTo(1));

            var audio = result.Value.Events.OfType<AudioPlayed>().Single();
            Assert.That(audio.Kind, Is.EqualTo(AudioEventKinds.Sfx));
            Assert.That(audio.Action, Is.EqualTo(AudioActions.OneShot));
            Assert.That(audio.Parameters, Has.Count.EqualTo(1));
            Assert.That(audio.Parameters![0].Name, Is.EqualTo("material"));

            var ended = result.Value.Events.OfType<BattleEnded>().Single();
            Assert.That(ended.Winner, Is.EqualTo(BattleWinners.Heroes));
            Assert.That(ended.Rounds, Is.EqualTo(2));
        }

        /// <summary>A missing sequence number is rejected with a result error, not an exception.</summary>
        [Test]
        public void Read_SequenceGap_ReturnsFailure()
        {
            CreateRun();
            string path = Path.Combine(runDirectory, "torch_session.jsonl");
            string[] lines = File.ReadAllLines(path);
            lines[1] = lines[1].Replace("\"Sequence\":2", "\"Sequence\":5");
            File.WriteAllLines(path, lines);

            Result<GameEventCapture> result = new GameEventLogReader().Read(runDirectory);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Sequence gap or duplicate"));
        }

        /// <summary>A duplicated sequence number is rejected.</summary>
        [Test]
        public void Read_DuplicateSequence_ReturnsFailure()
        {
            CreateRun();
            string path = Path.Combine(runDirectory, "torch_session.jsonl");
            string[] lines = File.ReadAllLines(path);
            lines[1] = lines[1].Replace("\"Sequence\":2", "\"Sequence\":1");
            File.WriteAllLines(path, lines);

            Result<GameEventCapture> result = new GameEventLogReader().Read(runDirectory);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Sequence gap or duplicate"));
        }

        /// <summary>An event whose subsystem does not match its file is rejected.</summary>
        [Test]
        public void Read_SubsystemMismatch_ReturnsFailure()
        {
            CreateRun();
            string path = Path.Combine(runDirectory, "torch_session.jsonl");
            File.WriteAllText(path, File.ReadAllText(path).Replace("\"Subsystem\":\"torch\"", "\"Subsystem\":\"combat\""));

            Result<GameEventCapture> result = new GameEventLogReader().Read(runDirectory);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("subsystem 'combat'"));
        }

        /// <summary>A type outside the event contract is rejected by the allowlist binder.</summary>
        [Test]
        public void Read_UnknownType_ReturnsFailureWithoutException()
        {
            CreateRun();
            string path = Path.Combine(runDirectory, "torch_session.jsonl");
            File.WriteAllText(
                path,
                "{\"$type\":\"System.IO.FileInfo, System.IO.FileSystem\",\"Subsystem\":\"torch\",\"Sequence\":1}"
                + Environment.NewLine);

            Result<GameEventCapture> result = new GameEventLogReader().Read(runDirectory);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("not part of the game event contract"));
        }

        /// <summary>A recording of another schema version is rejected.</summary>
        [Test]
        public void Read_UnsupportedSchemaVersion_ReturnsFailure()
        {
            CreateRun();
            string path = Path.Combine(runDirectory, "manifest.json");
            File.WriteAllText(
                path,
                File.ReadAllText(path).Replace(JsonEventLogger.CurrentSchemaVersion, "9.9.9"));

            Result<GameEventCapture> result = new GameEventLogReader().Read(runDirectory);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Unsupported schema version"));
        }

        /// <summary>A missing manifest is reported as a result failure.</summary>
        [Test]
        public void Read_MissingManifest_ReturnsFailure()
        {
            Result<GameEventCapture> result = new GameEventLogReader().Read(runDirectory);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Manifest not found"));
        }

        private void CreateRun()
        {
            var bus = new GameEventBus();
            using (var logger = new JsonEventLogger(bus, runDirectory))
            {
                bus.Publish(new TorchChanged(100, 99, "corridorStep"));
                bus.Publish(new TorchChanged(99, 98, "corridorStep"));
            }
        }

        private void CreateCurrentSchemaRun()
        {
            var bus = new GameEventBus();
            using (var logger = new JsonEventLogger(bus, runDirectory))
            {
                var statuses = new List<CombatantStatusState>
                {
                    new CombatantStatusState("Bleeding", true, 2, 1),
                };
                var buffs = new List<CombatantBuffState>
                {
                    new CombatantBuffState("buff_1", "damage", 0.15f, 3, "Combat", "skill", true),
                };
                var heroes = new List<CombatantState>
                {
                    new CombatantState(
                        "hero_2", "highwayman", 22, 22, false, "highwayman_portrait", "highwayman",
                        Rank: 2, Stress: 17, MaxStress: 100, Statuses: statuses, Buffs: buffs),
                };
                var combatants = new List<CombatantState>
                {
                    heroes[0],
                    new CombatantState(
                        "monster_3", "brigand_cutthroat", 12, 12, true, "brigand_portrait", "brigand_cutthroat",
                        Rank: 1, Size: 1),
                };

                bus.Publish(new SceneSnapshot(
                    "Dungeon",
                    Array.Empty<SceneNode>(),
                    new SceneGlobals(90, 500),
                    heroes,
                    new[] { "hero_2" }));
                bus.Publish(new StateSnapshot(
                    StateSnapshotContexts.BattleStart,
                    new SceneGlobals(90, 500),
                    combatants,
                    SubsystemNames.Combat,
                    Battle: new BattleState(1, "Hero", "hero_2", new[] { "monster_3" }, "none", "active")));
                bus.Publish(new SkillResolved(
                    "hero_2",
                    "slice",
                    new[] { new SkillTargetResult("monster_3", "Crit", 8, true, false, true, 4, 31, 1) },
                    new[] { "damage" },
                    new GameInput(GameInputKinds.PlayerClick, Target: "monster_3", Skill: "slice"),
                    new GameRng(Seed: 42, Roll: 0.03f, Crit: true, Modifier: 0.15f)));
                bus.Publish(new UnitMoved("hero_2", 2, 1, "skill", "hero_1"));
                bus.Publish(new TurnStarted("hero_2", 1, "Hero", new[] { "monster_3" }));
                bus.Publish(new TurnEnded("hero_2", 1));
                bus.Publish(new RoundStarted(2, new[] { "hero_2", "monster_3" }));
                bus.Publish(new RoundEnded(1));
                bus.Publish(new AudioPlayed(
                    "event:/char/hero/highwayman/attack",
                    AudioEventKinds.Sfx,
                    AudioActions.OneShot,
                    Volume: 0.8f,
                    Position: new AudioPosition(1, 0, 2),
                    Parameters: new[] { new AudioParameter("material", 1) }));
                bus.Publish(new BattleEnded(BattleWinners.Heroes, 2));
            }
        }

        private static string FindReferenceCapture()
        {
            DirectoryInfo directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "docs", "spec_game_events", "examples");
                if (Directory.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("docs/spec_game_events/examples was not found above the test directory.");
        }
    }
}
