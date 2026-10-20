using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Clients.EventLog.Tests
{
    /// <summary>Verifies schema 1.2 estate runs and their manifest kind.</summary>
    [TestFixture]
    public class EstateCaptureReaderTests
    {
        private string directory;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "eventlog-estate-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (directory != null && Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }

        /// <summary>The estate snapshot and action events round-trip through the logger and reader.</summary>
        [Test]
        public void Read_EstateCapture_RestoresManifestKindAndEvents()
        {
            var bus = new GameEventBus();
            using (var logger = new JsonEventLogger(
                       bus,
                       directory,
                       notes: "estate capture",
                       kind: GameEventRunKinds.Estate))
            {
                var resources = new List<EstateResourceAmount>
                {
                    new EstateResourceAmount("gold", 500),
                    new EstateResourceAmount("crest", 4),
                };
                var roster = new List<EstateHeroState>
                {
                    new EstateHeroState(
                        "hero_1", "crusader", "Reynauld", 1, 1, 33, 33, 10, "Available",
                        true, false, false, new[] { "q1" }, Array.Empty<string>()),
                };
                var buildings = new List<EstateBuildingState>
                {
                    new EstateBuildingState("abbey", "Abbey", 0.5f),
                };
                var quests = new List<EstateQuestState>
                {
                    new EstateQuestState("quest_1", "weald", "explore", 1, 2, "goal_1", false),
                };

                bus.Publish(new EstateSnapshot(2, resources, roster, buildings, quests, "town_event", "Good"));
                bus.Publish(new EstateHeroRecruited("hero_1", "crusader", "Reynauld", 1, 1));
                bus.Publish(new EstateBuildingUpgraded("abbey", "abbey.meditation", "upgrade_1", 0.55f));
                bus.Publish(new EstateQuestSelected("quest_1", "weald", "explore", 1, 2));
                bus.Publish(new EstateProvisionsPurchased("food", 2, 50, false));
                bus.Publish(new EstateHeirloomExchanged("crest", 2, "gold", 100));
                bus.Publish(new EstateTownEventTriggered("town_event", "Good"));
            }

            Result<GameEventCapture> result = new GameEventLogReader().Read(directory);

            Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
            Assert.That(result.Value.Manifest.SchemaVersion, Is.EqualTo(JsonEventLogger.CurrentSchemaVersion));
            Assert.That(result.Value.Manifest.Kind, Is.EqualTo(GameEventRunKinds.Estate));
            Assert.That(result.Value.Events, Has.Count.EqualTo(7));

            var snapshot = (EstateSnapshot)result.Value.Events[0];
            Assert.That(snapshot.Week, Is.EqualTo(2));
            Assert.That(snapshot.Resources[0].Id, Is.EqualTo("gold"));
            Assert.That(snapshot.Resources[0].Amount, Is.EqualTo(500));
            Assert.That(snapshot.Roster[0].Name, Is.EqualTo("Reynauld"));
            Assert.That(snapshot.Buildings[0].UpgradeRatio, Is.EqualTo(0.5f));
            Assert.That(snapshot.Quests[0].Dungeon, Is.EqualTo("weald"));

            Assert.That(result.Value.Events[2], Is.TypeOf<EstateBuildingUpgraded>());
            Assert.That(result.Value.Events[6], Is.TypeOf<EstateTownEventTriggered>());
        }
    }
}
