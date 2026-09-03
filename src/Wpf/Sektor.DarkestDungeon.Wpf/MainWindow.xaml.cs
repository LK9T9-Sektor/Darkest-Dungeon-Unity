using System;
using System.IO;
using System.Windows;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Logging;
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
            var logger = CreateLogger();

            var shell = new ShellViewModel();
            var menu = new MainMenuViewModel(
                shell,
                () => new DuelLobbyViewModel(shell, DuelTransportFactory.CreateSteamTransport(), DuelClasses.AllClassIds, logger),
                () => new SinglePlayerLobbyViewModel(shell, DuelClasses.AllClassIds, logger),
                () => new PveLobbyViewModel(shell, DuelClasses.AllClassIds, logger));
            shell.SetHome(menu);
            shell.NavigateTo(menu);
            return shell;
        }

        private static Core.Common.ILogger CreateLogger()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Logs", "duel.log");
            var provider = new FileLoggerProvider(filePath);
            return new MsLoggerAdapter(provider.CreateLogger("Duel"));
        }
    }
}
