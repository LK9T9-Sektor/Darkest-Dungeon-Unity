using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Sektor.DarkestDungeon.Wpf.Ui;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Battle screen view: hosts the pre-built hover-arrow strip and card hover wiring.</summary>
    public partial class DuelBattleView : PumpableScreenBase
    {
        private Rectangle[] _arrowCells = new Rectangle[0];
        private bool _calibrated;
        private bool _awaitingLayout;

        /// <summary>Shows the attack arrow for the hovered target card, or hides it when invalid.</summary>
        /// <param name="target">The hovered unit card.</param>
        internal void ShowArrowFor(DuelUnitViewModel target)
        {
            EnsureCells();
            var viewModel = DataContext as DuelBattleViewModel;
            if (viewModel == null || target == null || !viewModel.CanShowArrow(target))
            {
                ClearArrow();
                return;
            }

            var mask = DuelArrowCells.MaskFor(viewModel.CurrentActorTeam, viewModel.CurrentActorRank, target.Rank);
            for (int i = 0; i < _arrowCells.Length; i++)
                _arrowCells[i].Visibility = mask.Contains(i) ? Visibility.Visible : Visibility.Collapsed;

            ArrowGrid.Visibility = Visibility.Visible;
        }

        /// <summary>Collapses the overlay and every pre-built arrow cell.</summary>
        internal void ClearArrow()
        {
            EnsureCells();
            foreach (var cell in _arrowCells)
                cell.Visibility = Visibility.Collapsed;
            ArrowGrid.Visibility = Visibility.Collapsed;
        }

        /// <summary>Calibrates the strip columns against the measured card positions exactly once
        /// (on the first layout that has the cards realized); every later call only toggles the
        /// pre-built cells' Visibility.</summary>
        private void EnsureCells()
        {
            if (_calibrated)
                return;

            _arrowCells = ArrowGrid.Children.OfType<Rectangle>().ToArray();
            if (_arrowCells.Length != DuelArrowCells.CellCount)
                return;

            if (CalibrateArrowColumns())
            {
                _calibrated = true;
                return;
            }

            if (!_awaitingLayout)
            {
                _awaitingLayout = true;
                LayoutUpdated += CalibrateOnNextLayout;
            }
        }

        private void CalibrateOnNextLayout(object? sender, EventArgs e)
        {
            _awaitingLayout = false;
            if (!_calibrated && CalibrateArrowColumns())
            {
                _calibrated = true;
                LayoutUpdated -= CalibrateOnNextLayout;
            }
        }

        private bool CalibrateArrowColumns()
        {
            var ordered = CardsOrderedByPosition();
            if (ordered.Length != DuelArrowCells.CellCount)
                return false;
            if (ordered.Any(card => card.ActualWidth <= 0))
                return false;

            var columns = ArrowGrid.ColumnDefinitions;
            columns[0].Width = new GridLength(SlotX(ordered[0]), GridUnitType.Pixel);
            for (int i = 0; i < DuelArrowCells.CellCount - 1; i++)
                columns[i + 1].Width = new GridLength(SlotX(ordered[i + 1]) - SlotX(ordered[i]), GridUnitType.Pixel);
            columns[DuelArrowCells.CellCount].Width = new GridLength(ordered[DuelArrowCells.CellCount - 1].ActualWidth, GridUnitType.Pixel);
            return true;
        }

        private DuelUnitCardView[] CardsOrderedByPosition()
        {
            return FindVisualChildren<DuelUnitCardView>(this).OrderBy(SlotX).ToArray();
        }

        private double SlotX(Visual visual)
        {
            return visual.TransformToVisual(ArrowGrid).Transform(new Point(0, 0)).X;
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