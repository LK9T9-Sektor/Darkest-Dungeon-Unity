using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Sektor.DarkestDungeon.Wpf.Ui;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Battle screen view: hosts the selected-skill badge and the hover target arrow.</summary>
    public partial class DuelBattleView : PumpableScreenBase
    {
        private const double BadgeWidth = 50;
        private const double BadgeHeight = 50;
        private const double BadgeGap = 6;
        private const double BadgeLift = 0;
        private const double ArrowHeadLength = 14;
        private const double ArrowHeadSpread = 7;
        private const int MaxSkillArrows = 4;

        /// <summary>Shows the target arrows for the hovered card, or hides them when invalid.</summary>
        /// <param name="target">The hovered unit card.</param>
        internal void ShowArrowFor(DuelUnitViewModel target)
        {
            var viewModel = DataContext as DuelBattleViewModel;
            if (viewModel == null || target == null)
            {
                ClearArrow();
                return;
            }

            // On the opponent's turn the player's hover must not touch the rival's preview arrow.
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
            var viewModel = DataContext as DuelBattleViewModel;
            if (viewModel != null && !viewModel.IsLocalTurn)
                return;

            HideLine();
            HideSkillArrows();
            UpdateBadge();
        }

        /// <summary>Draws the rival (AI/network) reveal arrow during the rival's reveal: a skill elbow
        /// arrow to the target, or the ⇄ move line for a move, matching what the actor is about to do.</summary>
        /// <param name="viewModel">The battle view model.</param>
        internal void RedrawAiArrow(DuelBattleViewModel viewModel)
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
                SkillArrowPaths[i].Visibility = Visibility.Collapsed;
                SkillArrowHeads[i].Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>Draws the move swap line: a straight line between the two card centers with an
        /// arrowhead at each end (⇄), since a move exchanges the ranks of the actor and an adjacent
        /// ally.</summary>
        /// <param name="target">The hovered target.</param>
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

        /// <summary>Draws elbow arrows to the valid targets of the selected skill: the badge sits above a
        /// horizontal spine, and each target gets a vertical drop from the spine down into its top edge
        /// with an arrowhead. For AOE/party skills one arrow is drawn to every valid target; for a
        /// single-target skill only the hovered target is highlighted. The color follows the skill tone.</summary>
        /// <param name="viewModel">The battle view model.</param>
        /// <param name="hoveredTarget">The currently hovered card.</param>
        private void DrawSkillArrows(DuelBattleViewModel viewModel, DuelUnitViewModel hoveredTarget)
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

        /// <summary>Draws one elbow arrow per target using the current badge position and skill tone.</summary>
        /// <param name="viewModel">The battle view model.</param>
        /// <param name="targets">The target cards.</param>
        private void DrawElbowArrows(DuelBattleViewModel viewModel, List<DuelUnitViewModel> targets)
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

            Brush brush = Ui.SkillToneClassifier.ArrowBrush(
                viewModel.IsLocalTurn ? viewModel.SelectedSkillTone : ActiveBadgeTone(viewModel));

            Point badgeCenter = BadgeCenter();
            double exitY = badgeCenter.Y;

            int count = Math.Min(targets.Count, MaxSkillArrows);
            for (int i = 0; i < MaxSkillArrows; i++)
            {
                if (i >= count)
                {
                    SkillArrowPaths[i].Visibility = Visibility.Collapsed;
                    SkillArrowHeads[i].Visibility = Visibility.Collapsed;
                    continue;
                }

                var targetCard = FindCard(targets[i].CombatId);
                if (targetCard == null)
                {
                    SkillArrowPaths[i].Visibility = Visibility.Collapsed;
                    SkillArrowHeads[i].Visibility = Visibility.Collapsed;
                    continue;
                }

                Point targetTop = TopCenter(targetCard);
                bool targetOnLeft = targetTop.X < badgeCenter.X;
                double exitX = targetOnLeft ? badgeCenter.X - BadgeWidth / 2 : badgeCenter.X + BadgeWidth / 2;

                // The line exits the skill badge from its left/right edge at the badge's vertical
                // center, runs horizontally above the cards, then drops into the target top edge.
                var figure = new PathFigure { StartPoint = new Point(exitX, exitY), IsClosed = false, IsFilled = false };
                figure.Segments.Add(new LineSegment(new Point(targetTop.X, exitY), true));
                figure.Segments.Add(new LineSegment(targetTop, true));

                var geometry = new PathGeometry();
                geometry.Figures.Add(figure);

                var path = SkillArrowPaths[i];
                path.Data = geometry;
                path.Stroke = brush;
                path.Visibility = Visibility.Visible;

                var head = SkillArrowHeads[i];
                head.Points = new PointCollection(TargetArrowMath.ArrowHead(
                    targetTop, new Point(targetTop.X, exitY), ArrowHeadLength, ArrowHeadSpread));
                head.Fill = brush;
                head.Visibility = Visibility.Visible;
            }
        }

        private static Ui.SkillTone ActiveBadgeTone(DuelBattleViewModel viewModel)
        {
            var badgeSkill = viewModel.IsLocalTurn ? viewModel.SelectedSkill : viewModel.AiSkillPreview;
            return badgeSkill != null ? badgeSkill.Tone : Ui.SkillTone.Attack;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            LayoutUpdated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            UpdateBadge();
        }

                private void UpdateBadge()
        {
            var viewModel = DataContext as DuelBattleViewModel;
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

        private DuelUnitCardView? FindActorCard()
        {
            return FindVisualChildren<DuelUnitCardView>(this)
                .FirstOrDefault(card => card.DataContext is DuelUnitViewModel unit && unit.IsCurrent);
        }

        private DuelUnitCardView? FindCard(int combatId)
        {
            return FindVisualChildren<DuelUnitCardView>(this)
                .FirstOrDefault(card => card.DataContext is DuelUnitViewModel unit && unit.CombatId == combatId);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
            where T : DependencyObject
        {
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

        private void OnUnitMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DuelUnitViewModel target)
                ShowArrowFor(target);
        }

        private void OnUnitMouseLeave(object sender, MouseEventArgs e)
        {
            ClearArrow();
        }

        private void OnUnitMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClearArrow();
        }
    }
}