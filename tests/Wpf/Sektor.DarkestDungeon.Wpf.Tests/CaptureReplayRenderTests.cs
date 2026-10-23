using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using NUnit.Framework;

using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;
using Sektor.DarkestDungeon.Wpf.ViewModels;
using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Offscreen layout regression for the capture replay screen.</summary>
    [TestFixture]
    public class CaptureReplayRenderTests
    {
        private const int WindowWidth = 1600;
        private const int WindowHeight = 900;

        /// <summary>The replay screen lays out the formation, bottom event log and replay controls.</summary>
        [Test]
        public void CaptureReplayLayout_RendersFormationEventLogAndControls()
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    RenderAndAssert();
                }
                catch (Exception e)
                {
                    error = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (error != null)
                throw new AssertionException("Render failed: " + error);
        }

        private static void RenderAndAssert()
        {
            var existing = Application.Current;
            var app = existing ?? new App();
            if (existing == null)
                ((App)app).InitializeComponent();

            var pacer = new QueuedReplayPacer();
            var viewModel = new CaptureReplayViewModel(
                new FakeNavigationService(),
                new GameEventLogReader(),
                new CaptureAnimationCatalog(Path.Combine(TestContext.CurrentContext.TestDirectory, "Content", "Spine")),
                new FakeLogger(),
                Path.Combine(TestContext.CurrentContext.TestDirectory, "Content", "Captures", "Reference"),
                pacer);

            int guard = 0;
            while (!viewModel.IsAwaitingAnimation && pacer.RunNext())
                Assert.That(++guard, Is.LessThan(100), "The replay did not reach the first attack.");

            var view = new CaptureReplayView { DataContext = viewModel };
            var size = new Size(WindowWidth, WindowHeight);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();

            Assert.That(viewModel.TotalEvents, Is.EqualTo(38));
            Assert.That(viewModel.IsAwaitingAnimation, Is.True);

            List<TextBlock> timelineLabels = FindVisualChildren<TextBlock>(view)
                .Where(block => block.DataContext is CaptureTimelineEntryViewModel)
                .ToList();
            Assert.That(timelineLabels, Has.Count.EqualTo(38));

            List<SpineHeroControl> spineControls = FindVisualChildren<SpineHeroControl>(view).ToList();
            Assert.That(spineControls, Has.Count.EqualTo(3));

            var bitmap = new RenderTargetBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(view);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            string pngPath = Path.Combine(Path.GetTempPath(), "capture-replay-layout.png");
            using (var stream = File.Create(pngPath))
                encoder.Save(stream);
        }

        /// <summary>The replay draws an arrow from the attacker card to its target during an attack.</summary>
        [Test]
        public void CaptureReplayAttack_ShowsArrowFromAttackerToTarget()
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    RenderArrow();
                }
                catch (Exception e)
                {
                    error = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (error != null)
                throw new AssertionException("Render failed: " + error);
        }

        private static void RenderArrow()
        {
            var existing = Application.Current;
            var app = existing ?? new App();
            if (existing == null)
                ((App)app).InitializeComponent();

            var pacer = new QueuedReplayPacer();
            var viewModel = new CaptureReplayViewModel(
                new FakeNavigationService(),
                new GameEventLogReader(),
                new CaptureAnimationCatalog(Path.Combine(TestContext.CurrentContext.TestDirectory, "Content", "Spine")),
                new FakeLogger(),
                Path.Combine(TestContext.CurrentContext.TestDirectory, "Content", "Captures", "Reference"),
                pacer);

            var view = new CaptureReplayView { DataContext = viewModel };
            var size = new Size(WindowWidth, WindowHeight);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();

            int guard = 0;
            while (!viewModel.IsAwaitingAnimation && pacer.RunNext())
                Assert.That(++guard, Is.LessThan(100), "The replay did not reach the first attack.");

            view.UpdateLayout();

            BattleArrowView? arrow = FindVisualChildren<BattleArrowView>(view).FirstOrDefault();
            Assert.That(arrow, Is.Not.Null, "The replay field hosts the attack arrow layer.");

            var line = (System.Windows.Shapes.Polyline)arrow!.FindName("ArrowLine");
            var head = (System.Windows.Shapes.Polygon)arrow.FindName("ArrowHead");
            Assert.That(line, Is.Not.Null);
            Assert.That(head, Is.Not.Null);
            Assert.That(line.Visibility, Is.EqualTo(Visibility.Visible),
                "An attack draws the arrow from the attacker to the target.");
            Assert.That(head.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(line.Points.Count, Is.EqualTo(4),
                "The arrow goes up from the attacker, across above the cards and down onto the target.");
            Assert.That(head.Points.Count, Is.EqualTo(3));

            var attacker = viewModel.PartyFormation.Concat(viewModel.Enemies).First(unit => unit.IsAttacker);
            var target = viewModel.PartyFormation.Concat(viewModel.Enemies).First(unit => unit.IsTarget);
            PointCollection drawn = line.Points;
            arrow.Show(attacker, target);
            Assert.That(line.Points, Is.SameAs(drawn),
                "Unchanged geometry must keep the same collection, otherwise LayoutUpdated loops.");

            for (int pass = 0; pass < 5; pass++)
                view.UpdateLayout();
            Assert.That(line.Points, Is.SameAs(drawn),
                "Repeated layout passes must not rebuild the arrow geometry.");

            var bitmap = new RenderTargetBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(view);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = File.Create(Path.Combine(Path.GetTempPath(), "capture-replay-attack.png")))
                encoder.Save(stream);
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

        private sealed class QueuedReplayPacer : IReplayPacer
        {
            private readonly Queue<Action> pending = new Queue<Action>();

            public void Schedule(TimeSpan delay, Action callback) => pending.Enqueue(callback);

            public void Cancel() => pending.Clear();

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
            public void Log(string message)
            {
            }

            public void Warn(string message)
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
    }
}
