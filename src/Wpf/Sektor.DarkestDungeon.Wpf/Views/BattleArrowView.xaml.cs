using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>
    /// Arrow from the attacking card to its target, drawn over the battle field when a capture
    /// replay plays an attack. The line runs above the cards so it never crosses a neighbour:
    /// up from the attacker, across, then down onto the target.
    /// </summary>
    public partial class BattleArrowView : UserControl
    {
        private const double HeadLength = 16;
        private const double HeadHalfWidth = 8;
        private const double CardGap = 4;
        private const double LaneOffset = 24;

        private IBattleFieldUnit? source;
        private IBattleFieldUnit? target;

        /// <summary>Initializes a new instance of the <see cref="BattleArrowView"/> class.</summary>
        public BattleArrowView()
        {
            InitializeComponent();
            Loaded += (sender, e) => Redraw();
            LayoutUpdated += (sender, e) => Redraw();
        }

        /// <summary>Draws an arrow from the attacker card to the target card.</summary>
        /// <param name="from">The attacking unit.</param>
        /// <param name="to">The attacked unit.</param>
        public void Show(IBattleFieldUnit? from, IBattleFieldUnit? to)
        {
            source = from;
            target = to;
            Redraw();
        }

        /// <summary>Hides the arrow.</summary>
        public void Hide()
        {
            source = null;
            target = null;
            SetShapesVisible(false);
        }

        private void Redraw()
        {
            if (source == null || target == null || ReferenceEquals(source, target) || Visibility != Visibility.Visible)
            {
                SetShapesVisible(false);
                return;
            }

            BattleUnitCardView? fromCard = FindCard(source);
            BattleUnitCardView? toCard = FindCard(target);
            if (fromCard == null || toCard == null || fromCard.ActualWidth <= 0 || toCard.ActualWidth <= 0)
            {
                SetShapesVisible(false);
                return;
            }

            Point fromTop = TopCenterOf(fromCard);
            Point toTop = TopCenterOf(toCard);
            double lane = Math.Min(fromTop.Y, toTop.Y) - LaneOffset;
            var start = new Point(fromTop.X, fromTop.Y - CardGap);
            var tip = new Point(toTop.X, toTop.Y - CardGap);
            var headBase = new Point(tip.X, tip.Y - HeadLength);
            if (headBase.Y <= lane)
            {
                SetShapesVisible(false);
                return;
            }

            Point[] route =
            {
                start,
                new Point(start.X, lane),
                new Point(headBase.X, lane),
                headBase,
            };

            Point[] head =
            {
                tip,
                new Point(tip.X - HeadHalfWidth, headBase.Y),
                new Point(tip.X + HeadHalfWidth, headBase.Y),
            };

            // Reassigning the point collections on every layout pass invalidates the layout again:
            // LayoutUpdated would spin forever and starve the replay timers. Update only on change.
            if (!SamePoints(ArrowLine.Points, route))
                ArrowLine.Points = new PointCollection(route);
            if (!SamePoints(ArrowHead.Points, head))
                ArrowHead.Points = new PointCollection(head);

            SetShapesVisible(true);
        }

        private static bool SamePoints(PointCollection? current, Point[] wanted)
        {
            if (current == null || current.Count != wanted.Length)
                return false;

            for (int index = 0; index < wanted.Length; index++)
            {
                if (Math.Abs(current[index].X - wanted[index].X) > 0.5 ||
                    Math.Abs(current[index].Y - wanted[index].Y) > 0.5)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetShapesVisible(bool visible)
        {
            Visibility visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            if (ArrowLine.Visibility != visibility)
                ArrowLine.Visibility = visibility;
            if (ArrowHead.Visibility != visibility)
                ArrowHead.Visibility = visibility;
        }

        private Point TopCenterOf(FrameworkElement element)
        {
            return element.TransformToVisual(this)
                .Transform(new Point(element.ActualWidth / 2, 0));
        }

        private BattleUnitCardView? FindCard(IBattleFieldUnit unit)
        {
            BattleFieldView? field = FindField();
            if (field == null)
                return null;

            foreach (BattleUnitCardView card in FindVisualChildren<BattleUnitCardView>(field))
            {
                if (ReferenceEquals(card.DataContext, unit))
                    return card;
            }

            return null;
        }

        private BattleFieldView? FindField()
        {
            DependencyObject? current = VisualTreeHelper.GetParent(this);
            while (current != null)
            {
                if (current is BattleFieldView field)
                    return field;
                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
            where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int index = 0; index < count; index++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, index);
                if (child is T match)
                    yield return match;

                foreach (T descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }
    }
}
