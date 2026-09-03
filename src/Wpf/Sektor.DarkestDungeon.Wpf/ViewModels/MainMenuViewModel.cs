using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Main menu: choose versus AI, PvE or multiplayer duel.</summary>
    public partial class MainMenuViewModel : ObservableObject
    {
        private readonly INavigationService navigation;
        private readonly Func<object> createMultiplayerLobby;
        private readonly Func<object> createSinglePlayerLobby;
        private readonly Func<object> createPveLobby;

        /// <summary>Gets the command opening the single player lobby.</summary>
        public IRelayCommand VsAiCommand { get; }

        /// <summary>Gets the command opening the PvE lobby (heroes vs monsters).</summary>
        public IRelayCommand PveCommand { get; }

        /// <summary>Gets the command opening the multiplayer lobby.</summary>
        public IRelayCommand MultiplayerCommand { get; }

        /// <summary>Gets the command that closes the application.</summary>
        public IRelayCommand CloseCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="MainMenuViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="createMultiplayerLobby">Creates the multiplayer lobby screen.</param>
        /// <param name="createSinglePlayerLobby">Creates the single player lobby screen.</param>
        /// <param name="createPveLobby">Creates the PvE lobby screen.</param>
        public MainMenuViewModel(INavigationService navigation, Func<object> createMultiplayerLobby, Func<object> createSinglePlayerLobby, Func<object> createPveLobby)
        {
            this.navigation = navigation;
            this.createMultiplayerLobby = createMultiplayerLobby;
            this.createSinglePlayerLobby = createSinglePlayerLobby;
            this.createPveLobby = createPveLobby;
            VsAiCommand = new RelayCommand(OpenVsAi);
            PveCommand = new RelayCommand(OpenPve);
            MultiplayerCommand = new RelayCommand(OpenMultiplayer);
            CloseCommand = new RelayCommand(Close);
        }

        private void OpenVsAi()
        {
            navigation.NavigateTo(createSinglePlayerLobby());
        }

        private void OpenPve()
        {
            navigation.NavigateTo(createPveLobby());
        }

        private void OpenMultiplayer()
        {
            navigation.NavigateTo(createMultiplayerLobby());
        }

        private void Close()
        {
            Application.Current?.Shutdown();
        }
    }
}
