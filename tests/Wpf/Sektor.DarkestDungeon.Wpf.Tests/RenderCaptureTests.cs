using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rectangle = System.Windows.Shapes.Rectangle;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Networking;
using Sektor.DarkestDungeon.Wpf.Ui;
using Sektor.DarkestDungeon.Wpf.ViewModels;
using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Offscreen rendering regression tests for the duel HUD layout (based on the visual
    /// tree bounds, not a screenshot: the CI model cannot inspect bitmaps).</summary>
    [TestFixture]
    public class RenderCaptureTests
    {
        private const int WindowWidth = 1600;
        private const int WindowHeight = 900;

        private sealed class DuelRenderTestsNullRivalLink : IDuelRivalLink
        {
#pragma warning disable CS0067
            public event Action<string>? RivalActionReceived;
#pragma warning restore CS0067

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

        private static DuelBattleViewModel CreateView(out DuelController duel)
        {
            duel = new DuelController(new DuelContent());
            duel.StartDuel(Picks("crusader"), Picks("highwayman"), 42, isHost: true);
            RandomSolver.SetRandomSeed(42);
            duel.StartBattle();
            return new DuelBattleViewModel(duel, new DuelRenderTestsNullRivalLink(), () => { });
        }

        /// <summary>Makes the rival pass until it is the local player's turn, so the skill buttons
        /// render (they are only populated during the local turn).</summary>
        private static void DriveToLocalTurn(DuelController duel, DuelBattleViewModel view)
        {
            int guard = 0;
            while (!duel.IsLocalTurn && !duel.IsFinished && guard++ < 50)
            {
                duel.ApplyRemoteSkill(DuelPayload.PassAction());
                view.Refresh();
            }
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

        private static Point Origin(Visual root, FrameworkElement element)
        {
            return element.TransformToAncestor(root).Transform(new Point(0, 0));
        }

        private static double CenterY(Visual root, FrameworkElement element)
        {
            return Origin(root, element).Y + element.ActualHeight / 2;
        }

        private static double CenterX(Visual root, FrameworkElement element)
        {
            return Origin(root, element).X + element.ActualWidth / 2;
        }

        private static void DumpLayout(Visual root, string path)
        {
            using (var writer = new StreamWriter(path))
                Walk(root, root, writer, 0);
        }

        private static void Walk(DependencyObject node, Visual root, StreamWriter writer, int depth)
        {
            string? tag = null;
            if (node is FrameworkElement fe)
            {
                switch (fe.DataContext)
                {
                    case DuelUnitViewModel:
                        tag = "CARD";
                        break;
                    case DuelTurnEntryViewModel when fe is Border:
                        tag = "TURNENTRY";
                        break;
                    case DuelSkillViewModel:
                        tag = "SKILL";
                        break;
                }

                if (fe is Button button)
                {
                    string? tooltip = button.ToolTip?.ToString();
                    if (tooltip == "Move to an adjacent rank")
                        tag = "MOVEBTN";
                    else if (tooltip == "Skip the turn")
                        tag = "PASSBTN";
                }

                if (tag != null)
                {
                    GeneralTransform transform = fe.TransformToAncestor(root);
                    Point origin = transform.Transform(new Point(0, 0));
                    writer.WriteLine(
                        $"{tag}{new string(' ', Math.Max(1, 12 - tag.Length))}" +
                        $"{fe.GetType().Name,-16}" +
                        $"X={origin.X,5:0} Y={origin.Y,5:0} W={fe.ActualWidth,5:0} H={fe.ActualHeight,5:0} " +
                        $"aspect={fe.ActualWidth / Math.Max(1, fe.ActualHeight),4:0.00} " +
                        $"dc={fe.DataContext}");
                }
            }

            int count = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < count; i++)
                Walk(VisualTreeHelper.GetChild(node, i), root, writer, depth + 1);
        }

        private static void RenderAndAssert()
        {
            var existing = Application.Current;
            var app = existing ?? new App();
            if (existing == null)
                ((App)app).InitializeComponent();

            DuelController duel;
            var view = CreateView(out duel);
            DriveToLocalTurn(duel, view);
            view.OpenStatsCommand.Execute(view.Heroes[0]);

            var duelView = new DuelBattleView { DataContext = view };
            var size = new Size(WindowWidth, WindowHeight);
            duelView.Measure(size);
            duelView.Arrange(new Rect(size));
            duelView.UpdateLayout();

            // Cards render tall and are never flattened (the regression this suite guards against is
            // the star portrait row collapsing to its content height when measured unbounded).
            List<DuelUnitCardView> cards = new List<DuelUnitCardView>(FindVisualChildren<DuelUnitCardView>(duelView));
            Assert.That(cards.Count, Is.EqualTo(8), "One card per unit.");
            foreach (var card in cards)
            {
                Assert.That(card.ActualWidth, Is.EqualTo(185).Within(1));
                Assert.That(card.ActualHeight, Is.EqualTo(330).Within(1));
            }

            // Skills, Move and Pass all sit in the same horizontal row (the StripPanel guard against
            // the WrapPanel wrapping the skills onto a second row).
            List<Button> buttons = new List<Button>(FindVisualChildren<Button>(duelView));
            List<Button> skillButtons = buttons.FindAll(button => button.DataContext is DuelSkillViewModel);
            Button? moveButton = buttons.Find(button => button.ToolTip?.ToString() == "Move to an adjacent rank");
            Button? passButton = buttons.Find(button => button.ToolTip?.ToString() == "Skip the turn");

            Assert.That(view.Skills.Count, Is.GreaterThan(0), "The local turn should populate skill buttons.");
            Assert.That(skillButtons.Count, Is.EqualTo(view.Skills.Count));
            Assert.That(moveButton, Is.Not.Null);
            Assert.That(passButton, Is.Not.Null);

            double skillsCenterY = CenterY(duelView, skillButtons[0]);
            double moveCenterY = CenterY(duelView, moveButton!);
            double passCenterY = CenterY(duelView, passButton!);
            Assert.That(moveCenterY, Is.EqualTo(skillsCenterY).Within(12), "MOVE is on the skills row.");
            Assert.That(passCenterY, Is.EqualTo(skillsCenterY).Within(12), "PASS is on the skills row.");

            // The right-click stat sheet is centered on the whole window with an opaque background.
            Grid overlay = (Grid)duelView.FindName("StatsOverlay");
            var overlayBorder = (Border)overlay.Children[0];
            var overlayBrush = (SolidColorBrush)overlayBorder.Background;
            Assert.That(overlay.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Center));
            Assert.That(overlay.VerticalAlignment, Is.EqualTo(VerticalAlignment.Center));
            Assert.That(overlayBrush.Color, Is.EqualTo(Color.FromRgb(0x0C, 0x0A, 0x08)));
            Assert.That(overlayBrush.Color.A, Is.EqualTo(255), "The overlay background must be opaque.");
            Assert.That(CenterX(duelView, overlay), Is.EqualTo(WindowWidth / 2.0).Within(4));
            Assert.That(CenterY(duelView, overlay), Is.EqualTo(WindowHeight / 2.0).Within(4));

            DumpLayout(duelView, Path.Combine(Path.GetTempPath(), "duel-layout.txt"));

            var bitmap = new RenderTargetBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(duelView);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            string pngPath = Path.Combine(Path.GetTempPath(), "duel-layout.png");
            using (var stream = File.Create(pngPath))
                encoder.Save(stream);
        }

        [Test]
        public void DuelLayout_Regression()
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

        private static Grid BuildArrowOverlay(out DuelBattleView duelView, out DuelBattleViewModel view, out DuelUnitViewModel validTarget)
        {
            var existing = Application.Current;
            var app = existing ?? new App();
            if (existing == null)
                ((App)app).InitializeComponent();

            DuelController duel;
            view = CreateView(out duel);
            DriveToLocalTurn(duel, view);

            DuelUnitViewModel? target = null;
            foreach (var skill in view.Skills)
            {
                view.SelectSkillCommand.Execute(skill);
                target = view.Heroes.Concat(view.Monsters).FirstOrDefault(unit => unit.IsTarget);
                if (target != null)
                    break;
            }

            Assert.That(target, Is.Not.Null, "At least one skill must expose a valid hover target.");

            duelView = new DuelBattleView { DataContext = view };
            duelView.Measure(new Size(WindowWidth, WindowHeight));
            duelView.Arrange(new Rect(new Size(WindowWidth, WindowHeight)));
            duelView.UpdateLayout();

            validTarget = target!;
            return (Grid)duelView.FindName("ArrowGrid");
        }

        /// <summary>The hover arrow uses the pre-built 4x4 cells: only visibility is toggled, the
        /// sheet never hits tests, and an invalid/none hover leaves every cell collapsed.</summary>
        [Test]
        public void DuelArrow_HoverShowsBandAndClears()
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    DuelBattleView duelView;
                    DuelBattleViewModel view;
                    DuelUnitViewModel target;
                    var grid = BuildArrowOverlay(out duelView, out view, out target);
                    Assert.That(grid.IsHitTestVisible, Is.False, "The overlay must never intercept card clicks.");

                    var cells = grid.Children.OfType<Rectangle>().ToArray();
                    Assert.That(cells.Length, Is.EqualTo(DuelArrowCells.CellCount));
                    Assert.That(grid.Visibility, Is.EqualTo(Visibility.Collapsed), "The overlay starts collapsed.");

                    duelView.ShowArrowFor(target);
                    var mask = DuelArrowCells.MaskFor(view.CurrentActorTeam);
                    Assert.That(grid.Visibility, Is.EqualTo(Visibility.Visible), "Hovering reveals the overlay.");
                    for (int i = 0; i < cells.Length; i++)
                    {
                        bool expected = mask.Contains(i);
                        Assert.That(cells[i].Visibility == Visibility.Visible, Is.EqualTo(expected),
                            "Cell " + i + " visibility does not match the band mask.");
                    }

                    duelView.ClearArrow();
                    Assert.That(grid.Visibility, Is.EqualTo(Visibility.Collapsed), "Clearing hides the overlay.");
                    Assert.That(cells.All(cell => cell.Visibility == Visibility.Collapsed), Is.True,
                        "Clearing the arrow collapses every cell again.");
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
                throw new AssertionException("Arrow hover failed: " + error);
        }
    }
}