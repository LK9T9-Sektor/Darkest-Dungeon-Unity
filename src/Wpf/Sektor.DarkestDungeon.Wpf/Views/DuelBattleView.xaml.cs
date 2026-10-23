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

        private BattleTargetOverlay? TargetOverlay
        {
            get { return BattleFieldControl.Overlay as BattleTargetOverlay; }
        }

        internal void ShowArrowFor(DuelUnitViewModel target)
        {
            TargetOverlay?.ShowArrowFor(target);
        }

        internal void ClearArrow()
        {
            TargetOverlay?.ClearArrow();
        }

        private void OnFieldUnitHovered(object? sender, BattleFieldUnitEventArgs e)
        {
            if (e.Unit is DuelUnitViewModel target)
                TargetOverlay?.ShowArrowFor(target);
        }

        private void OnFieldHoverCleared(object? sender, EventArgs e)
        {
            TargetOverlay?.ClearArrow();
        }
    }
}
