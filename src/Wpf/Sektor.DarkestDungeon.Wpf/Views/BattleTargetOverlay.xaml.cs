using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Sektor.DarkestDungeon.Wpf.Ui;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Selected-skill badge, hover arrows and rival preview arrows over the battle cards.</summary>
    public partial class BattleTargetOverlay : UserControl
    {
        private const double BadgeWidth = 50;
        private const double BadgeHeight = 50;
        private const double BadgeGap = 6;
        private const double BadgeLift = 0;
        private const double ArrowHeadLength = 14;
        private const double ArrowHeadSpread = 7;
        private const int MaxSkillArrows = 4;

        private readonly Path[] skillArrowPaths;
        private readonly Polygon[] skillArrowHeads;

        /// <summary>Identifies the <see cref="CardsRoot"/> dependency property.</summary>
        public static readonly DependencyProperty CardsRootProperty = DependencyProperty.Register(
            nameof(CardsRoot),
            typeof(FrameworkElement),
            typeof(BattleTargetOverlay),
            new PropertyMetadata(null));

        /// <summary>Initializes a new instance of the <see cref="BattleTargetOverlay"/> class.</summary>
        public BattleTargetOverlay()
        {
            InitializeComponent();
            skillArrowPaths = new[] { SkillArrow1, SkillArrow2, SkillArrow3, SkillArrow4 };
            skillArrowHeads = new[] { SkillArrowHead1, SkillArrowHead2, SkillArrowHead3, SkillArrowHead4 };
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        /// <summary>Gets or sets the element whose visual tree contains the unit cards.</summary>
        public FrameworkElement? CardsRoot
        {
            get { return (FrameworkElement?)GetValue(CardsRootProperty); }
            set { SetValue(CardsRootProperty, value); }
        }

        /// <summary>Shows the target arrows for the hovered card, or hides them when invalid.</summary>
        /// <param name="target">The hovered unit card.</param>
        internal void ShowArrowFor(DuelUnitViewModel target)
        {
            var viewModel = DataContext as IDuelBattleViewData;
            if (viewModel == null || target == null)
            {
                ClearArrow();
                return;
            }

            if (!viewModel.IsLocalTurn)
                return;

            UpdateBadge();
            if (!viewModel.CanShowArrow(target))
            {
                HideLine();
                HideSkillArrows();
                return;
            }

            if (viewModel.IsMoveMode)
            {
                HideSkillArrows();
                DrawMoveArrow(target);
                return;
            }

            HideLine();
            DrawSkillArrows(viewModel, target);
        }

        /// <summary>Collapses all hover lines and arrowheads; the selected-skill badge stays visible.</summary>
        internal void ClearArrow()
        {
            var viewModel = DataContext as IDuelBattleViewData;
            if (viewModel != null && !viewModel.IsLocalTurn)
                return;

            HideLine();
            HideSkillArrows();
            UpdateBadge();
        }

        /// <summary>Draws the rival reveal arrow during the rival's turn.</summary>
        /// <param name="viewModel">The battle view model.</param>
        internal void RedrawAiArrow(IDuelBattleViewData viewModel)
        {
            if (viewModel.IsLocalTurn || viewModel.AiTargetPreview == null)
            {
                HideSkillArrows();
                HideLine();
                return;
            }

            if (viewModel.IsMovePreview)
            {
                HideSkillArrows();
                DrawMoveArrow(viewModel.AiTargetPreview);
                return;
            }

            HideLine();
            DrawElbowArrows(viewModel, new List<DuelUnitViewModel> { viewModel.AiTargetPreview });
        }

        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var overlay = (BattleTargetOverlay)sender;
            if (e.OldValue is INotifyPropertyChanged oldViewModel)
                oldViewModel.PropertyChanged -= overlay.OnViewModelPropertyChanged;
            if (e.NewValue is INotifyPropertyChanged newViewModel)
                newViewModel.PropertyChanged += overlay.OnViewModelPropertyChanged;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            LayoutUpdated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            UpdateBadge();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DuelBattleViewModel.AiSkillPreview):
                case nameof(DuelBattleViewModel.SelectedSkill):
                    UpdateBadge();
                    break;
                case nameof(DuelBattleViewModel.AiTargetPreview):
                case nameof(DuelBattleViewModel.IsMovePreview):
                    if (DataContext is IDuelBattleViewData viewModel)
                        RedrawAiArrow(viewModel);
                    break;
            }
        }

        private void HideLine()
        {
            ArrowLine.Visibility = Visibility.Collapsed;
            ArrowHead.Visibility = Visibility.Collapsed;
            ArrowHeadReverse.Visibility = Visibility.Collapsed;
        }

        private void HideSkillArrows()
        {
            for (int i = 0; i < MaxSkillArrows; i++)
            {
                skillArrowPaths[i].Visibility = Visibility.Collapsed;
                skillArrowHeads[i].Visibility = Visibility.Collapsed;
            }
        }

        private void DrawMoveArrow(DuelUnitViewModel target)
        {
            var actorCard = FindActorCard();
            var targetCard = FindCard(target.CombatId);
            if (actorCard == null || targetCard == null)
            {
                HideLine();
                return;
            }

            Point start = Center(actorCard);
            Point end = Center(targetCard);

            ArrowLine.X1 = start.X;
            ArrowLine.Y1 = start.Y;
            ArrowLine.X2 = end.X;
            ArrowLine.Y2 = end.Y;
            ArrowLine.Visibility = Visibility.Visible;

            ArrowHead.Points = new PointCollection(
                TargetArrowMath.ArrowHead(end, start, ArrowHeadLength, ArrowHeadSpread));
            ArrowHead.Visibility = Visibility.Visible;

            ArrowHeadReverse.Points = new PointCollection(
                TargetArrowMath.ArrowHead(start, end, ArrowHeadLength, ArrowHeadSpread));
            ArrowHeadReverse.Visibility = Visibility.Visible;
        }

        private void DrawSkillArrows(IDuelBattleViewData viewModel, DuelUnitViewModel hoveredTarget)
        {
            var actorCard = FindActorCard();
            if (actorCard == null || !(actorCard.DataContext is DuelUnitViewModel actor))
            {
                HideSkillArrows();
                return;
            }

            List<DuelUnitViewModel> targets;
            if (viewModel.SelectedSkillIsMultiTarget)
            {
                targets = viewModel.Heroes.Concat(viewModel.Monsters)
                    .Where(card => card.IsTarget && card.CombatId != actor.CombatId)
                    .ToList();
            }
            else
            {
                targets = new List<DuelUnitViewModel> { hoveredTarget };
            }

            DrawElbowArrows(viewModel, targets);
        }

        private void DrawElbowArrows(IDuelBattleViewData viewModel, List<DuelUnitViewModel> targets)
        {
            if (targets.Count == 0)
            {
                HideSkillArrows();
                return;
            }

            var actorCard = FindActorCard();
            if (actorCard == null)
            {
                HideSkillArrows();
                return;
            }

            Brush brush = SkillToneClassifier.ArrowBrush(
                viewModel.IsLocalTurn ? viewModel.SelectedSkillTone : ActiveBadgeTone(viewModel));

            Point badgeCenter = BadgeCenter();
            double exitY = badgeCenter.Y;

            int count = Math.Min(targets.Count, MaxSkillArrows);
            for (int i = 0; i < MaxSkillArrows; i++)
            {
                if (i >= count)
                {
                    skillArrowPaths[i].Visibility = Visibility.Collapsed;
                    skillArrowHeads[i].Visibility = Visibility.Collapsed;
                    continue;
                }

                var targetCard = FindCard(targets[i].CombatId);
                if (targetCard == null)
                {
                    skillArrowPaths[i].Visibility = Visibility.Collapsed;
                    skillArrowHeads[i].Visibility = Visibility.Collapsed;
                    continue;
                }

                Point targetTop = TopCenter(targetCard);
                bool targetOnLeft = targetTop.X < badgeCenter.X;
                double exitX = targetOnLeft ? badgeCenter.X - BadgeWidth / 2 : badgeCenter.X + BadgeWidth / 2;

                var figure = new PathFigure { StartPoint = new Point(exitX, exitY), IsClosed = false, IsFilled = false };
                figure.Segments.Add(new LineSegment(new Point(targetTop.X, exitY), true));
                figure.Segments.Add(new LineSegment(targetTop, true));

                var geometry = new PathGeometry();
                geometry.Figures.Add(figure);

                var path = skillArrowPaths[i];
                path.Data = geometry;
                path.Stroke = brush;
                path.Visibility = Visibility.Visible;

                var head = skillArrowHeads[i];
                head.Points = new PointCollection(TargetArrowMath.ArrowHead(
                    targetTop, new Point(targetTop.X, exitY), ArrowHeadLength, ArrowHeadSpread));
                head.Fill = brush;
                head.Visibility = Visibility.Visible;
            }
        }

        private static SkillTone ActiveBadgeTone(IDuelBattleViewData viewModel)
        {
            var badgeSkill = viewModel.IsLocalTurn ? viewModel.SelectedSkill : viewModel.AiSkillPreview;
            return badgeSkill != null ? badgeSkill.Tone : SkillTone.Attack;
        }

        private void UpdateBadge()
        {
            var viewModel = DataContext as IDuelBattleViewData;
            if (viewModel == null)
            {
                SkillBadge.Visibility = Visibility.Collapsed;
                return;
            }

            var skill = viewModel.IsLocalTurn ? viewModel.SelectedSkill : viewModel.AiSkillPreview;
            if (skill == null)
            {
                SkillBadge.Visibility = Visibility.Collapsed;
                return;
            }

            var actorCard = FindActorCard();
            if (actorCard == null)
                return;

            SkillBadge.DataContext = skill;
            Point top = TopCenter(actorCard);
            Canvas.SetLeft(SkillBadge, top.X - BadgeWidth / 2);
            double badgeTop = top.Y - BadgeHeight - BadgeGap - BadgeLift;
            badgeTop = Math.Max(0, Math.Min(badgeTop, Math.Max(0, TargetLayer.ActualHeight - BadgeHeight)));
            Canvas.SetTop(SkillBadge, badgeTop);
            SkillBadge.Visibility = Visibility.Visible;
        }

        private Point BadgeCenter()
        {
            return new Point(Canvas.GetLeft(SkillBadge) + BadgeWidth / 2, Canvas.GetTop(SkillBadge) + BadgeHeight / 2);
        }

        private Point TopCenter(FrameworkElement element)
        {
            return element.TransformToVisual(TargetLayer).Transform(new Point(element.RenderSize.Width / 2, 0));
        }

        private Point Center(FrameworkElement element)
        {
            return element.TransformToVisual(TargetLayer)
                .Transform(new Point(element.RenderSize.Width / 2, element.RenderSize.Height / 2));
        }

        private BattleUnitCardView? FindActorCard()
        {
            return FindVisualChildren<BattleUnitCardView>(CardsRoot ?? Parent)
                .FirstOrDefault(card => card.DataContext is DuelUnitViewModel unit && unit.IsCurrent);
        }

        private BattleUnitCardView? FindCard(int combatId)
        {
            return FindVisualChildren<BattleUnitCardView>(CardsRoot ?? Parent)
                .FirstOrDefault(card => card.DataContext is DuelUnitViewModel unit && unit.CombatId == combatId);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject? root)
            where T : DependencyObject
        {
            if (root == null)
                yield break;

            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                    yield return match;
                foreach (var descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }
    }
}
