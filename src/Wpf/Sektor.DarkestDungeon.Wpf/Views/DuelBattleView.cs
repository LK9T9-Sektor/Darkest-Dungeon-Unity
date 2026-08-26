using System.Windows;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Window rendering the live duel state with click inputs.</summary>
    public partial class DuelBattleView : Window
    {
        /// <summary>Initializes a new instance of the <see cref="DuelBattleView"/> class.</summary>
        /// <param name="viewModel">The duel battle view model.</param>
        public DuelBattleView(DuelBattleViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}