using System;
using System.IO;
using System.Windows;
using Sektor.DarkestDungeon.Clients.EventLog;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Logging;
using Sektor.DarkestDungeon.Wpf.Networking;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf
{
    /// <summary>Main window hosting all screens via shell navigation.</summary>
    public partial class MainWindow : Window
    {
        private const string _captureRunEnvironmentVariable = "DARKEST_CAPTURE_RUN";

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
            var iconCatalog = new HeroIconCatalog(Path.Combine(AppContext.BaseDirectory, "Content", "HeroIcons"));
            var skillCatalog = new HeroSkillCatalog(DuelClasses.AllClasses, iconCatalog);
            var spineCatalog = new SpineDemoCatalog(Path.Combine(AppContext.BaseDirectory, "Content", "Spine"));
            var menu = new MainMenuViewModel(
                shell,
                () => new DuelLobbyViewModel(shell, DuelClasses.AllClassIds, logger, skillCatalog, iconCatalog, spineCatalog),
                () => new MultiplayerLobbyViewModel(shell, DuelTransportFactory.CreateSteamTransport(), DuelClasses.AllClassIds, logger, skillCatalog, iconCatalog, spineCatalog),
                () => new SpineDemoViewModel(
                    shell,
                    new SpineDemoCatalog(Path.Combine(AppContext.BaseDirectory, "Content", "Spine")),
                    iconCatalog,
                    skillCatalog),
                () => new SpineBattleViewModel(
                    shell,
                    new SpineDemoCatalog(Path.Combine(AppContext.BaseDirectory, "Content", "Spine")),
                    logger,
                    () => new CaptureReplayViewModel(
                        shell,
                        new GameEventLogReader(),
                        new CaptureAnimationCatalog(Path.Combine(AppContext.BaseDirectory, "Content", "Spine")),
                        logger,
                        ResolveCaptureReplayDirectory(),
                        new DispatcherReplayPacer())),
                () => new EstateReplayViewModel(
                    shell,
                    new GameEventLogReader(),
                    logger,
                    ResolveEstateReplayDirectory(),
                    new DispatcherReplayPacer()));
            shell.SetHome(menu);
            shell.NavigateTo(menu);
            return shell;
        }

        private static string ResolveCaptureReplayDirectory()
        {
            string? overrideDirectory = Environment.GetEnvironmentVariable(_captureRunEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(overrideDirectory))
                return Path.GetFullPath(overrideDirectory.Trim().Trim('"'));

            string reference = Path.Combine(AppContext.BaseDirectory, "Content", "Captures", "Reference");
            return GameEventCaptureLocator.ResolveReplayDirectory(
                AppContext.BaseDirectory,
                reference,
                GameEventRunKinds.Raid);
        }

        private static string ResolveEstateReplayDirectory()
        {
            string reference = Path.Combine(AppContext.BaseDirectory, "Content", "Captures", "EstateReference");
            return GameEventCaptureLocator.ResolveReplayDirectory(
                AppContext.BaseDirectory,
                reference,
                GameEventRunKinds.Estate);
        }

        private static Core.Common.ILogger CreateLogger()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Logs", "duel.log");
            var provider = new FileLoggerProvider(filePath);
            return new MsLoggerAdapter(provider.CreateLogger("Duel"));
        }
    }
}
