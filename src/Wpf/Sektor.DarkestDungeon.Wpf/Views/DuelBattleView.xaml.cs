using System;
using System.Windows;
using System.Windows.Input;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Battle screen view: stats overlay, keyboard shortcuts and card hover routing.</summary>
    public partial class DuelBattleView : PumpableScreenBase
    {
        private void OnStatsCloseRequested(object? sender, EventArgs e)
        {
            CloseStats();
        }

        private void OnStatsBackdropClick(object sender, MouseButtonEventArgs e)
        {
            CloseStats();
            e.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && UnitInfoOverlay.Visibility == Visibility.Visible)
            {
                CloseStats();
                e.Handled = true;
            }
        }

        private void CloseStats()
        {
            if (DataContext is IDuelBattleViewData viewModel)
                viewModel.CloseStatsCommand.Execute(null);
        }

        internal void ShowArrowFor(DuelUnitViewModel target)
        {
            TargetOverlay.ShowArrowFor(target);
        }

        internal void ClearArrow()
        {
            TargetOverlay.ClearArrow();
        }

        private void OnUnitMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DuelUnitViewModel target)
                TargetOverlay.ShowArrowFor(target);
        }

        private void OnUnitMouseLeave(object sender, MouseEventArgs e)
        {
            TargetOverlay.ClearArrow();
        }

        private void OnUnitMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TargetOverlay.ClearArrow();
        }
    }
}
