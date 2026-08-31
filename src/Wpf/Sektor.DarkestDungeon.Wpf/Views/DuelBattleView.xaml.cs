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
        private const double BadgeWidth = 40;
        private const double BadgeHeight = 24;
        private const double BadgeGap = 6;
        private const double ArrowHeadLength = 14;
        private const double ArrowHeadSpread = 7;

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            LayoutUpdated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            UpdateBadge();
        }

        /// <summary>Shows the target arrow for the hovered card, or hides it when invalid.</summary>
        /// <param name="target">The hovered unit card.</param>
        internal void ShowArrowFor(DuelUnitViewModel target)
        {
            var viewModel = DataContext as DuelBattleViewModel;
            if (viewModel == null || target == null)
            {
                ClearArrow();
                return;
            }

            UpdateBadge();
            if (!viewModel.CanShowArrow(target))
            {
                HideLine();
                return;
            }

            var actorCard = FindActorCard();
            var targetCard = FindCard(target.CombatId);
            if (actorCard == null || targetCard == null)
            {
                HideLine();
                return;
            }

            Point start = SkillBadge.Visibility == Visibility.Visible ? BadgeCenter() : TopCenter(actorCard);
            Point end = Center(targetCard);

            ArrowLine.X1 = start.X;
            ArrowLine.Y1 = start.Y;
            ArrowLine.X2 = end.X;
            ArrowLine.Y2 = end.Y;
            ArrowLine.Visibility = Visibility.Visible;

            ArrowHead.Points = new PointCollection(
                TargetArrowMath.ArrowHead(end, start, ArrowHeadLength, ArrowHeadSpread));
            ArrowHead.Visibility = Visibility.Visible;
        }

        /// <summary>Collapses the hover line and arrowhead; the selected-skill badge stays visible.</summary>
        internal void ClearArrow()
        {
            HideLine();
            UpdateBadge();
        }

        private void HideLine()
        {
            ArrowLine.Visibility = Visibility.Collapsed;
            ArrowHead.Visibility = Visibility.Collapsed;
        }

        private void UpdateBadge()
        {
            var viewModel = DataContext as DuelBattleViewModel;
            if (viewModel == null || viewModel.SelectedSkill == null || viewModel.IsLocalTurn == false)
            {
                SkillBadge.Visibility = Visibility.Collapsed;
                return;
            }

            var actorCard = FindActorCard();
            if (actorCard == null)
                return;

            SkillBadgeText.Text = viewModel.SelectedSkill.DisplayNameUpper;
            Point top = TopCenter(actorCard);
            Canvas.SetLeft(SkillBadge, top.X - BadgeWidth / 2);
            Canvas.SetTop(SkillBadge, top.Y - BadgeHeight - BadgeGap);
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