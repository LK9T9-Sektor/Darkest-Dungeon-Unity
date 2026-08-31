using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

using Sektor.DarkestDungeon.Wpf.Ui;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Battle screen view: hosts the pre-built 4x4 hover-arrow overlay and card hover wiring.</summary>
    public partial class DuelBattleView : PumpableScreenBase
    {
        private Rectangle[] _arrowCells = new Rectangle[0];

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
            {
                _arrowCells[i].Visibility = mask.Contains(i) ? Visibility.Visible : Visibility.Collapsed;
            }

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

        private void EnsureCells()
        {
            if (_arrowCells.Length != DuelArrowCells.CellCount)
                _arrowCells = ArrowGrid.Children.OfType<Rectangle>().ToArray();
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