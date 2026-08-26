using System.Windows;
using Sektor.DarkestDungeon.Wpf.Networking;
using Sektor.DarkestDungeon.Wpf.ViewModels;
using Sektor.DarkestDungeon.Wpf.Views;

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

        private void OpenDuelLobby_Click(object sender, RoutedEventArgs e)
        {
            var lobby = new DuelLobbyView(new DuelLobbyViewModel(DuelTransportFactory.CreateSteamTransport()))
            {
                Owner = this,
            };
            lobby.ShowDialog();
        }
    }
}