using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using NUnit.Framework;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.GameEvents;
using Sektor.DarkestDungeon.Wpf.Navigation;
using Sektor.DarkestDungeon.Wpf.ViewModels;
using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Offscreen layout regression for the estate replay screen.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class EstateReplayRenderTests
    {
        private const int WindowWidth = 1600;
        private const int WindowHeight = 900;

        /// <summary>Buildings, heroes and the timeline render with real card bindings.</summary>
        [Test]
        public void EstateLayout_RendersBuildingAndHeroCards()
        {
            var existing = Application.Current;
            var app = existing ?? new App();
            if (existing == null)
                ((App)app).InitializeComponent();

            string directory = CreateEstateCapture();
            try
            {
                var viewModel = new EstateReplayViewModel(
                    new FakeNavigationService(),
                    new GameEventLogReader(),
                    new FakeLogger(),
                    directory,
                    new ImmediateReplayPacer());

                var view = new EstateReplayView { DataContext = viewModel };
                var size = new Size(WindowWidth, WindowHeight);
                view.Measure(size);
                view.Arrange(new Rect(size));
                view.UpdateLayout();

                List<string> texts = FindVisualChildren<TextBlock>(view)
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrEmpty(text))
                    .ToList();

                Assert.That(texts, Has.Some.EqualTo("ЗДАНИЯ"));
                Assert.That(texts, Has.Some.EqualTo("ГЕРОИ"));
                Assert.That(texts, Has.Some.EqualTo("ТАЙМЛАЙН"));
                Assert.That(texts, Has.Some.EqualTo("АУДИО"));
                Assert.That(texts, Has.Some.EqualTo("Abbey"));
                Assert.That(texts, Has.Some.EqualTo("Reynauld"));
            }
            finally
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, recursive: true);
            }
        }

        private static string CreateEstateCapture()
        {
            string directory = Path.Combine(Path.GetTempPath(), "estate-render-" + Guid.NewGuid().ToString("N"));
            var bus = new GameEventBus();
            using (var logger = new JsonEventLogger(
                       bus,
                       directory,
                       notes: "estate render",
                       kind: GameEventRunKinds.Estate))
            {
                var resources = new List<EstateResourceAmount>
                {
                    new EstateResourceAmount("gold", 500),
                };
                var roster = new List<EstateHeroState>
                {
                    new EstateHeroState(
                        "hero_1", "crusader", "Reynauld", 1, 1, 33, 33, 10, "Available",
                        true, false, false, Array.Empty<string>(), Array.Empty<string>()),
                };
                var buildings = new List<EstateBuildingState>
                {
                    new EstateBuildingState("abbey", "Abbey", 0.5f),
                };

                bus.Publish(new EstateSnapshot(1, resources, roster, buildings, Array.Empty<EstateQuestState>(), null, null));
                bus.Publish(new AudioPlayed("event:/town/abbey", AudioEventKinds.Music, AudioActions.Start));
            }

            return directory;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
            where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                    yield return match;

                foreach (T descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
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
