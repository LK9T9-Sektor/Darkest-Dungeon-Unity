using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.GameEvents;
using Sektor.DarkestDungeon.Wpf.Navigation;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the estate capture projection, timeline and audio journal.</summary>
    [TestFixture]
    public class EstateReplayViewModelTests
    {
        /// <summary>A synthetic estate run projects resources, roster, buildings, quests and events.</summary>
        [Test]
        public void EstateCapture_ProjectsEconomyRosterBuildingsAndActions()
        {
            string directory = CreateEstateCapture();
            try
            {
                var viewModel = new EstateReplayViewModel(
                    new FakeNavigationService(),
                    new GameEventLogReader(),
                    new FakeLogger(),
                    directory,
                    new ImmediateReplayPacer());

                Assert.That(viewModel.IsFinished, Is.True);
                Assert.That(viewModel.TotalEvents, Is.EqualTo(8));
                Assert.That(viewModel.Week, Is.EqualTo(2));
                Assert.That(viewModel.Resources, Has.Count.EqualTo(2));
                Assert.That(viewModel.Resources[0].Label, Does.Contain("gold").And.Contain("750"));
                Assert.That(viewModel.Roster, Has.Count.EqualTo(1));
                Assert.That(viewModel.Roster[0].Name, Is.EqualTo("Reynauld"));
                Assert.That(viewModel.Roster[0].QuirksText, Is.EqualTo("q1"));
                Assert.That(viewModel.Buildings, Has.Count.EqualTo(1));
                Assert.That(viewModel.Buildings[0].Name, Is.EqualTo("Abbey"));
                Assert.That(viewModel.Quests, Has.Count.EqualTo(1));
                Assert.That(viewModel.Quests[0].Title, Does.Contain("weald"));
                Assert.That(viewModel.AudioLog, Has.Count.EqualTo(1));
                Assert.That(viewModel.Timeline, Has.Count.EqualTo(8));
                Assert.That(viewModel.Timeline[7].IsCurrent, Is.True);
                Assert.That(viewModel.StatusText, Does.Contain("8"));
            }
            finally
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, recursive: true);
            }
        }

        private static string CreateEstateCapture()
        {
            string directory = Path.Combine(Path.GetTempPath(), "estate-replay-" + Guid.NewGuid().ToString("N"));
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
                bus.Publish(new EstateBuildingUpgraded("abbey", "abbey.meditation", "upgrade_1", 0.75f));
                bus.Publish(new EstateQuestSelected("quest_1", "weald", "explore", 1, 2));
                bus.Publish(new EstateProvisionsPurchased("food", 2, 50, false));
                bus.Publish(new EstateHeirloomExchanged("crest", 2, "gold", 100));
                bus.Publish(new AudioPlayed(
                    "event:/town/abbey",
                    AudioEventKinds.Music,
                    AudioActions.Start));

                resources[0] = new EstateResourceAmount("gold", 750);
                bus.Publish(new EstateSnapshot(2, resources, roster, buildings, quests, null, null));
            }

            return directory;
        }

        private sealed class ImmediateReplayPacer : IReplayPacer
        {
            public void Schedule(TimeSpan delay, Action callback) => callback();

            public void Cancel()
            {
            }
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

        private sealed class FakeLogger : ILogger
        {
            public void Log(string message)
            {
            }

            public void Warn(string message)
            {
            }
        }
    }
}
