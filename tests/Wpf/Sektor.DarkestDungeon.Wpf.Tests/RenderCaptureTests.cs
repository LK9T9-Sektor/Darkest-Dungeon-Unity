using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Line = System.Windows.Shapes.Line;
using ShapePath = System.Windows.Shapes.Path;
using Polygon = System.Windows.Shapes.Polygon;
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
            public event Action<string>? SkillPreviewed;
            public event Action<int>? TargetPreviewed;
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
            // Tray skill squares are clickable (SelectCommand wired); the stat-sheet inspect squares
            // (visible because OpenStats ran) render read-only and must not be counted as tray buttons.
            List<Button> traySkillButtons = skillButtons.FindAll(button => button.Command != null);
            Button? moveButton = buttons.Find(button => button.ToolTip?.ToString() == "Move to an adjacent rank");
            Button? passButton = buttons.Find(button => button.ToolTip?.ToString() == "Skip the turn");

            Assert.That(view.Skills.Count, Is.GreaterThan(0), "The local turn should populate skill buttons.");
            Assert.That(traySkillButtons.Count, Is.EqualTo(view.Skills.Count));
            Assert.That(moveButton, Is.Not.Null);
            Assert.That(passButton, Is.Not.Null);

            double skillsCenterY = CenterY(duelView, traySkillButtons[0]);
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

        private static Canvas BuildArrowOverlay(out DuelBattleView duelView, out DuelBattleViewModel view, out DuelUnitViewModel validTarget)
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
                target = view.Heroes.Concat(view.Monsters)
                    .FirstOrDefault(unit => unit.IsTarget && !unit.IsCurrent);
                if (target != null)
                    break;
            }

            Assert.That(target, Is.Not.Null, "At least one skill must expose a valid non-self hover target.");

            duelView = new DuelBattleView { DataContext = view };
            duelView.Measure(new Size(WindowWidth, WindowHeight));
            duelView.Arrange(new Rect(new Size(WindowWidth, WindowHeight)));
            duelView.UpdateLayout();

            validTarget = target!;
            return (Canvas)duelView.FindName("TargetLayer");
        }

        /// <summary>The hover arrow is a badge + elbow path + arrowhead computed by math: the badge
        /// floats above the acting card while a skill is selected, the sheet never hits tests, a valid
        /// hover reveals the elbow arrow to every target with a 3-point arrowhead colored by the skill
        /// tone, and clearing hides the arrows but keeps the badge.</summary>
        [Test]
        public void DuelArrow_HoverShowsLineAndClears()
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    DuelBattleView duelView;
                    DuelBattleViewModel view;
                    DuelUnitViewModel target;
                    BuildArrowOverlay(out duelView, out view, out target);

                    var path = (ShapePath)duelView.FindName("SkillArrow1");
                    var head = (Polygon)duelView.FindName("SkillArrowHead1");
                    var badge = (FrameworkElement)duelView.FindName("SkillBadge");
                    Assert.That(path, Is.Not.Null);
                    Assert.That(head, Is.Not.Null);
                    Assert.That(badge, Is.Not.Null);
                    Assert.That(path.IsHitTestVisible, Is.False,
                        "The drawn skill arrows must never intercept card clicks.");
                    Assert.That(head.IsHitTestVisible, Is.False,
                        "The drawn arrowheads must never intercept card clicks.");
                    Assert.That(((System.Windows.Shapes.Line)duelView.FindName("ArrowLine")).IsHitTestVisible, Is.False,
                        "The drawn move line must never intercept card clicks.");

                    Assert.That(path.Visibility, Is.EqualTo(Visibility.Collapsed), "The skill arrows start hidden.");
                    Assert.That(head.Visibility, Is.EqualTo(Visibility.Collapsed), "The skill arrowheads start hidden.");

                    duelView.ShowArrowFor(target);

                    Assert.That(badge.Visibility, Is.EqualTo(Visibility.Visible),
                        "The selected-skill badge floats above the acting card while a skill is selected.");
                    Assert.That(path.Visibility, Is.EqualTo(Visibility.Visible), "Hovering a valid target reveals the skill arrow.");
                    Assert.That(head.Visibility, Is.EqualTo(Visibility.Visible), "Hovering reveals the arrowhead.");
                    Assert.That(head.Points.Count, Is.EqualTo(3));

                    var geometry = path.Data as PathGeometry;
                    Assert.That(geometry, Is.Not.Null);
                    Assert.That(geometry.Figures.Count, Is.EqualTo(1));
                    Assert.That(geometry.Figures[0].Segments.Count, Is.EqualTo(2),
                        "The elbow arrow has two segments: horizontal out of the badge side, then down to the target.");
                    Assert.That(path.Stroke, Is.EqualTo(SkillToneClassifier.ArrowBrush(view.SelectedSkillTone)),
                        "The arrow color follows the selected skill tone.");

                    int validTargets = view.Heroes.Concat(view.Monsters).Count(card => card.IsTarget && !card.IsCurrent);
                    int visibleArrows = 0;
                    for (int i = 1; i <= 4; i++)
                        if (((ShapePath)duelView.FindName("SkillArrow" + i)).Visibility == Visibility.Visible)
                            visibleArrows++;
                    Assert.That(visibleArrows, Is.EqualTo(view.SelectedSkillIsMultiTarget ? validTargets : 1),
                        "AOE/party skills draw one elbow arrow to every valid target; single-target skills only to the hovered one.");

                    duelView.ClearArrow();
                    Assert.That(path.Visibility, Is.EqualTo(Visibility.Collapsed), "Clearing hides the skill arrows.");
                    Assert.That(head.Visibility, Is.EqualTo(Visibility.Collapsed), "Clearing hides the arrowheads.");
                    Assert.That(badge.Visibility, Is.EqualTo(Visibility.Visible),
                        "The badge stays visible while the skill is still selected.");
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

        /// <summary>The skill-button tooltip must not bind DataContext on its detached content (no
        /// visual tree -> binding-source errors in the trace). It resolves through the ToolTip's own
        /// PlacementTarget, exactly like ToolTipService wires it when the popup opens.</summary>
        [Test]
        public void SkillTooltip_ResolvesDataContext_FromPlacementTarget()
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
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
                    duelView.Measure(new Size(WindowWidth, WindowHeight));
                    duelView.Arrange(new Rect(new Size(WindowWidth, WindowHeight)));
                    duelView.UpdateLayout();

                    var skillButton = FindVisualChildren<Button>(duelView)
                        .FirstOrDefault(button => button.DataContext is DuelSkillViewModel);
                    Assert.That(skillButton, Is.Not.Null);
                    var skill = (DuelSkillViewModel)skillButton!.DataContext;

                    var tooltip = skillButton.ToolTip as ToolTip;
                    Assert.That(tooltip, Is.Not.Null, "The skill button hosts an explicit ToolTip.");

                    Assert.That(tooltip.DataContext, Is.Null,
                        "While closed and detached the tooltip has no DataContext yet (no binding error).");

                    tooltip.PlacementTarget = skillButton;
                    tooltip.Dispatcher.Invoke(DispatcherPriority.DataBind, new Action(() => { }));

                    Assert.That(tooltip.DataContext, Is.SameAs(skill),
                        "ToolTip.DataContext follows the placement target's DataContext at open time.");
                    Assert.That(tooltip.Content, Is.TypeOf<SkillTooltipView>());
                    Assert.That(((SkillTooltipView)tooltip.Content).DataContext, Is.SameAs(skill),
                        "The tooltip content inherits the DataContext for its bindings.");
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
                throw new AssertionException("Skill tooltip DataContext failed: " + error);
        }

        /// <summary>Fully opens a skill-button ToolTip the way ToolTipService would, then asserts the
        /// structured tooltip is not empty/tiny: the content DataContext resolves, the readable name
        /// is shown (not the raw skill id), the base info and the effect rows are populated, and the
        /// rendered control has a real size (not a collapsed sliver).</summary>
        [Test]
        public void SkillTooltip_Open_RendersFully()
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
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
                    duelView.Measure(new Size(WindowWidth, WindowHeight));
                    duelView.Arrange(new Rect(new Size(WindowWidth, WindowHeight)));
                    duelView.UpdateLayout();

                    var skillButton = FindVisualChildren<Button>(duelView)
                        .FirstOrDefault(button => button.DataContext is DuelSkillViewModel);
                    Assert.That(skillButton, Is.Not.Null);
                    var skill = (DuelSkillViewModel)skillButton!.DataContext!;

                    var tooltip = (ToolTip)skillButton.ToolTip!;
                    tooltip.PlacementTarget = skillButton;
                    tooltip.IsOpen = true;
                    tooltip.Dispatcher.Invoke(DispatcherPriority.Loaded, new Action(() => { }));
                    tooltip.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
                    tooltip.UpdateLayout();

                    var content = (SkillTooltipView)tooltip.Content;
                    Assert.That(content.DataContext, Is.SameAs(skill),
                        "The opened tooltip content must inherit the skill DataContext.");

                    content.Measure(new Size(420, double.PositiveInfinity));
                    content.Arrange(new Rect(content.DesiredSize));
                    content.UpdateLayout();

                    Assert.That(content.ActualHeight, Is.GreaterThan(40),
                        "The tooltip must render content much taller than a bare header line / sliver.");
                    Assert.That(content.ActualWidth, Is.GreaterThan(80),
                        "The tooltip must render a usable width, not a collapsed sliver.");

                    TestContext.WriteLine(
                        $"DIAG id={skill.Id} name={skill.DisplayName} level={skill.Level} " +
                        $"baseInfo=[{skill.BaseInfo}] effectRows={skill.EffectRows.Count} " +
                        $"w={content.ActualWidth:0} h={content.ActualHeight:0}");

                    tooltip.IsOpen = false;
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
                throw new AssertionException("Open skill tooltip failed: " + error);
        }

        /// <summary>The move arrow is a swap line between the two card centers with an arrowhead at
        /// each end (⇄) because a move exchanges ranks; clearing hides line and both heads.</summary>
        [Test]
        public void MoveArrow_SwapLine_SpansCentersWithTwoHeads()
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var existing = Application.Current;
                    var app = existing ?? new App();
                    if (existing == null)
                        ((App)app).InitializeComponent();

                    DuelController duel;
                    var view = CreateView(out duel);
                    DriveToLocalTurn(duel, view);

                    view.MoveCommand.Execute(null);
                    DuelUnitViewModel? target = null;
                    foreach (var unit in view.Heroes.Concat(view.Monsters))
                    {
                        if (unit.IsTarget && !unit.IsCurrent)
                        {
                            target = unit;
                            break;
                        }
                    }

                    Assert.That(target, Is.Not.Null, "Move mode must mark an adjacent ally as a target.");
                    Assert.That(view.IsMoveMode, Is.True, "MoveCommand must enter move mode.");

                    var duelView = new DuelBattleView { DataContext = view };
                    duelView.Measure(new Size(WindowWidth, WindowHeight));
                    duelView.Arrange(new Rect(new Size(WindowWidth, WindowHeight)));
                    duelView.UpdateLayout();

                    var line = (Line)duelView.FindName("ArrowLine");
                    var head = (Polygon)duelView.FindName("ArrowHead");
                    var reverse = (Polygon)duelView.FindName("ArrowHeadReverse");
                    Assert.That(line, Is.Not.Null);
                    Assert.That(head, Is.Not.Null);
                    Assert.That(reverse, Is.Not.Null);

                    duelView.ShowArrowFor(target!);

                    Assert.That(line.Visibility, Is.EqualTo(Visibility.Visible), "The swap line is drawn.");
                    Assert.That(head.Visibility, Is.EqualTo(Visibility.Visible), "The target-end arrowhead is drawn.");
                    Assert.That(reverse.Visibility, Is.EqualTo(Visibility.Visible),
                        "The reverse (actor-end) arrowhead is drawn, signaling the rank exchange.");
                    Assert.That(head.Points.Count, Is.EqualTo(3));
                    Assert.That(reverse.Points.Count, Is.EqualTo(3));

                    var actorCard = FindVisualChildren<DuelUnitCardView>(duelView)
                        .First(card => card.DataContext is DuelUnitViewModel unit && unit.IsCurrent);
                    var layer = (Canvas)duelView.FindName("TargetLayer");
                    Point actorCenter = actorCard.TransformToVisual(layer)
                        .Transform(new Point(actorCard.RenderSize.Width / 2, actorCard.RenderSize.Height / 2));

                    Assert.That(line.X1, Is.EqualTo(actorCenter.X).Within(1),
                        "The move arrow must start at the acting card's horizontal center.");
                    Assert.That(line.Y1, Is.EqualTo(actorCenter.Y).Within(1),
                        "The move arrow must start at the acting card's vertical center (not its top edge).");
                    Assert.That(Math.Abs(line.X2 - line.X1), Is.GreaterThan(1),
                        "The move arrow spans toward the adjacent target card.");

                    duelView.ClearArrow();
                    Assert.That(line.Visibility, Is.EqualTo(Visibility.Collapsed), "Clearing hides the swap line.");
                    Assert.That(head.Visibility, Is.EqualTo(Visibility.Collapsed), "Clearing hides the target head.");
                    Assert.That(reverse.Visibility, Is.EqualTo(Visibility.Collapsed), "Clearing hides the reverse head.");
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
                throw new AssertionException("Move arrow failed: " + error);
        }

        /// <summary>The selected-skill badge must never be pushed above the top of the target canvas
        /// (the previous BadgeLift clipped it against the Viewbox); its top coordinate stays in-bounds
        /// and the badge remains visible while a skill is selected.</summary>
        [Test]
        public void Badge_StaysWithinCanvasTop()
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var existing = Application.Current;
                    var app = existing ?? new App();
                    if (existing == null)
                        ((App)app).InitializeComponent();

                    DuelController duel;
                    var view = CreateView(out duel);
                    DriveToLocalTurn(duel, view);

                    DuelUnitViewModel? target = null;
                    foreach (var skill in view.Skills)
                    {
                        view.SelectSkillCommand.Execute(skill);
                        target = view.Heroes.Concat(view.Monsters)
                            .FirstOrDefault(unit => unit.IsTarget && !unit.IsCurrent);
                        if (target != null)
                            break;
                    }
                    Assert.That(target, Is.Not.Null);

                    var duelView = new DuelBattleView { DataContext = view };
                    duelView.Measure(new Size(WindowWidth, WindowHeight));
                    duelView.Arrange(new Rect(new Size(WindowWidth, WindowHeight)));
                    duelView.UpdateLayout();

                    var badge = (FrameworkElement)duelView.FindName("SkillBadge");
                    Assert.That(badge, Is.Not.Null);

                    duelView.ShowArrowFor(target!);
                    duelView.UpdateLayout();

                    double top = Canvas.GetTop(badge);
                    Assert.That(badge.Visibility, Is.EqualTo(Visibility.Visible), "The badge is visible while a skill is selected.");
                    Assert.That(top, Is.GreaterThanOrEqualTo(0),
                        $"The badge top must not go above the canvas (was {top:0.0}); it would be clipped.");
                    Assert.That(top + badge.ActualHeight, Is.LessThanOrEqualTo(duelView.FindName("TargetLayer") is Canvas layer
                        ? layer.ActualHeight + 0.5 : double.MaxValue), "The badge bottom stays within the canvas.");
                    Assert.That(badge.IsHitTestVisible, Is.True,
                        "The badge opts back into hit testing so hovering it fires the skill tooltip.");
                    Button? badgeButton = FindVisualChildren<Button>(badge).FirstOrDefault();
                    Assert.That(badgeButton, Is.Not.Null);
                    Assert.That(badgeButton.IsHitTestVisible, Is.True,
                        "The badge's inner icon button is hoverable, so its tooltip can fire.");
                    Assert.That(badgeButton.Command, Is.Null,
                        "The floating badge is a pure indicator (icon-only, non-clickable).");
                    Assert.That(FindVisualChildren<TextBlock>(badge).Any(tb =>
                            tb.Visibility == Visibility.Visible && tb.Text == view.SelectedSkill?.DisplayNameUpper),
                        Is.False, "The compact badge shows no in-square text (name lives in the tooltip).");
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
                throw new AssertionException("Badge clamp failed: " + error);
        }

        /// <summary>The character-info sheet fills its structured skill squares from the inspected unit
        /// and each rendered SkillSquareView carries a readable name and the structured tooltip.</summary>
        [Test]
        public void StatsSheet_ShowsSkillSquares()
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var existing = Application.Current;
                    var app = existing ?? new App();
                    if (existing == null)
                        ((App)app).InitializeComponent();

                    DuelController duel;
                    var view = CreateView(out duel);
                    DriveToLocalTurn(duel, view);
                    view.OpenStatsCommand.Execute(view.Heroes[0]);

                    Assert.That(view.StatsTarget.Skills.Count, Is.GreaterThan(0),
                        "The stat sheet must expose structured skills, not just a plain text line.");
                    Assert.That(view.StatsTarget.Skills[0].DisplayName, Does.Not.Contain("_"),
                        "Skill names in the sheet are readable, not raw ids.");

                    var square = new SkillSquareView { DataContext = view.StatsTarget.Skills[0] };
                    square.Measure(new Size(60, 100));
                    square.Arrange(new Rect(square.DesiredSize));
                    square.UpdateLayout();

                    var label = FindVisualChildren<TextBlock>(square)
                        .FirstOrDefault(tb => tb.Text == view.StatsTarget.Skills[0].DisplayNameUpper);
                    Assert.That(label, Is.Not.Null, "The skill square renders the skill name below the icon slot.");

                    Button? button = FindVisualChildren<Button>(square).FirstOrDefault();
                    Assert.That(button, Is.Not.Null);
                    Assert.That(button.Command, Is.Null, "In the readonly sheet the square is not a select button.");
                    Assert.That(button.ToolTip, Is.TypeOf<ToolTip>(),
                        "The inspect square still hosts the structured skill tooltip.");
                    Assert.That(button.IsHitTestVisible, Is.True,
                        "The readonly square must stay hoverable so its tooltip can fire.");
                    Assert.That(button.IsEnabled, Is.True,
                        "The readonly square is enabled (unlike the tray it has no usable/turn gate).");
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
                throw new AssertionException("Stats sheet skill squares failed: " + error);
        }
    }
}