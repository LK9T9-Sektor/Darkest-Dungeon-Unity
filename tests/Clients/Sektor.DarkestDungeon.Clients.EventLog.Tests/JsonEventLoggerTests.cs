using System;
using System.IO;

using Newtonsoft.Json;

using NUnit.Framework;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Clients.EventLog.Tests
{
    /// <summary>Verifies the JSONL event logger: per-subsystem files, polymorphic roundtrip and manifest.</summary>
    [TestFixture]
    public class JsonEventLoggerTests
    {
        private static readonly JsonSerializerSettings ReadSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        private string runDirectory;

        [SetUp]
        public void SetUp()
        {
            runDirectory = Path.Combine(Path.GetTempPath(), "eventlog-test-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (runDirectory != null && Directory.Exists(runDirectory))
                Directory.Delete(runDirectory, recursive: true);
        }

        /// <summary>Events land in per-subsystem files and read back polymorphically in publish order.</summary>
        [Test]
        public void Events_AreWrittenPerSubsystem_AndRoundTripPolymorphically()
        {
            var bus = new GameEventBus();
            string manifestPath;
            using (var logger = new JsonEventLogger(bus, runDirectory, notes: "unit roundtrip"))
            {
                manifestPath = logger.ManifestPath;
                Assert.That(File.Exists(manifestPath), Is.True, "manifest must exist from construction");

                bus.Publish(new TorchChanged(100, 99, "corridorStep"));
                bus.Publish(new AttackExecuted(
                    "crusader_01",
                    "brigand_01",
                    "smite",
                    new AnimationRef("Crusader", "smite"),
                    new AnimationRef("BrigandCutthroat", "death"),
                    new GameInput(GameInputKinds.PlayerClick, Target: "brigand_01", Skill: "smite"),
                    new GameRng(Seed: 42, Roll: 0.03f, Crit: true, Modifier: 0.15f)));
                bus.Publish(new TorchChanged(99, 98, "corridorStep"));
            }

            string torchLines = File.ReadAllText(Path.Combine(runDirectory, "torch_session.jsonl"));
            string[] torchRows = torchLines.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(torchRows, Has.Length.EqualTo(2));

            string combatLine = File.ReadAllText(Path.Combine(runDirectory, "combat_session.jsonl"));
            var attack = (AttackExecuted)JsonConvert.DeserializeObject<GameEvent>(combatLine, ReadSettings);
            Assert.That(attack.AttackerId, Is.EqualTo("crusader_01"));
            Assert.That(attack.SkillName, Is.EqualTo("smite"));
            Assert.That(attack.AttackerAnimation.Skeleton, Is.EqualTo("Crusader"));
            Assert.That(attack.Input.Skill, Is.EqualTo("smite"));
            Assert.That(attack.Rng.Crit, Is.True);
            Assert.That(attack.Sequence, Is.EqualTo(2));

            var firstTorch = (TorchChanged)JsonConvert.DeserializeObject<GameEvent>(torchRows[0], ReadSettings);
            var secondTorch = (TorchChanged)JsonConvert.DeserializeObject<GameEvent>(torchRows[1], ReadSettings);
            Assert.That(firstTorch.Sequence, Is.EqualTo(1));
            Assert.That(secondTorch.Sequence, Is.EqualTo(3));
            Assert.That(firstTorch.EventId, Is.Not.EqualTo(secondTorch.EventId));
        }

        /// <summary>The manifest carries schema version, seed, notes and the file list.</summary>
        [Test]
        public void Manifest_CarriesSchemaSeedNotesAndFiles()
        {
            var bus = new GameEventBus();
            using (var logger = new JsonEventLogger(bus, runDirectory, seed: 42, notes: "tutorial run"))
            {
                bus.Publish(new TorchChanged(100, 99, "corridorStep"));
            }

            var manifest = JsonConvert.DeserializeObject<GameEventManifest>(File.ReadAllText(runDirectory + Path.DirectorySeparatorChar + "manifest.json"));
            Assert.That(manifest, Is.Not.Null);
            Assert.That(manifest.SchemaVersion, Is.EqualTo(JsonEventLogger.CurrentSchemaVersion));
            Assert.That(manifest.Seed, Is.EqualTo(42));
            Assert.That(manifest.Notes, Is.EqualTo("tutorial run"));
            Assert.That(manifest.Files, Does.Contain("torch_session.jsonl"));
        }

        /// <summary>An event published with an empty subsystem falls back to the unspecified file instead of vanishing.</summary>
        [Test]
        public void EmptySubsystem_FallsBackToUnspecifiedFile()
        {
            var bus = new GameEventBus();
            using (var logger = new JsonEventLogger(bus, runDirectory))
            {
                bus.Publish(new ActionRejected("targetFullHp", string.Empty, new GameInput(GameInputKinds.PlayerClick)));
            }

            Assert.That(File.Exists(Path.Combine(runDirectory, "unspecified_session.jsonl")), Is.True);
        }
    }
}
