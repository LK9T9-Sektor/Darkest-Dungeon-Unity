using System.Windows;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Networking;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf
{
    /// <summary>Main window hosting all screens via shell navigation.</summary>
    public partial class MainWindow : Window
    {
        /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = CreateShell();
        }

        private static ShellViewModel CreateShell()
        {
            var shell = new ShellViewModel();
            var menu = new MainMenuViewModel(
                shell,
                () => new DuelLobbyViewModel(shell, DuelTransportFactory.CreateSteamTransport(), DuelClasses.AllClassIds),
                () => new SinglePlayerLobbyViewModel(shell, DuelClasses.AllClassIds));
            shell.SetHome(menu);
            shell.NavigateTo(menu);
            return shell;
        }
    }
}
