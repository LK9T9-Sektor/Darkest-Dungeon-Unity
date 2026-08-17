using System.Windows;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf
{
    /// <summary>Main window hosting the battle screen mockup.</summary>
    public partial class MainWindow : Window
    {
        /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new BattleScreenViewModel();
        }
    }
}
