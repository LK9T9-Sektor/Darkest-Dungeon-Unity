using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Main menu: choose versus AI or multiplayer duel.</summary>
    public partial class MainMenuViewModel : ObservableObject
    {
        private readonly INavigationService navigation;
        private readonly Func<object> createMultiplayerLobby;
        private readonly Func<object> createSinglePlayerLobby;

        /// <summary>Gets the command opening the single player lobby.</summary>
        public IRelayCommand VsAiCommand { get; }

        /// <summary>Gets the command opening the multiplayer lobby.</summary>
        public IRelayCommand MultiplayerCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="MainMenuViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="createMultiplayerLobby">Creates the multiplayer lobby screen.</param>
        /// <param name="createSinglePlayerLobby">Creates the single player lobby screen.</param>
        public MainMenuViewModel(INavigationService navigation, Func<object> createMultiplayerLobby, Func<object> createSinglePlayerLobby)
        {
            this.navigation = navigation;
            this.createMultiplayerLobby = createMultiplayerLobby;
            this.createSinglePlayerLobby = createSinglePlayerLobby;
            VsAiCommand = new RelayCommand(OpenVsAi);
            MultiplayerCommand = new RelayCommand(OpenMultiplayer);
        }

        private void OpenVsAi()
        {
            navigation.NavigateTo(createSinglePlayerLobby());
        }

        private void OpenMultiplayer()
        {
            navigation.NavigateTo(createMultiplayerLobby());
        }
    }
}
