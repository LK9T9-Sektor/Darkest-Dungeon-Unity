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
        private readonly Func<object> createSpineDemo;
        private readonly Func<object> createSpineBattle;

        /// <summary>Gets the command opening the single player lobby.</summary>
        public IRelayCommand VsAiCommand { get; }

        /// <summary>Gets the command opening the PvE lobby (heroes vs monsters).</summary>
        public IRelayCommand PveCommand { get; }

        /// <summary>Gets the command opening the multiplayer lobby.</summary>
        public IRelayCommand MultiplayerCommand { get; }

        /// <summary>Gets the command opening the Spine animation test screen.</summary>
        public IRelayCommand SpineTestCommand { get; }

        /// <summary>Gets the command opening the visual 1x1 spine arena.</summary>
        public IRelayCommand SpineBattleCommand { get; }

        /// <summary>Gets the command that closes the application.</summary>
        public IRelayCommand CloseCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="MainMenuViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="createMultiplayerLobby">Creates the multiplayer lobby screen.</param>
        /// <param name="createSinglePlayerLobby">Creates the single player lobby screen.</param>
        /// <param name="createPveLobby">Creates the PvE lobby screen.</param>
        /// <param name="createSpineDemo">Creates the Spine animation test screen.</param>
        /// <param name="createSpineBattle">Creates the visual 1x1 spine arena screen.</param>
        public MainMenuViewModel(INavigationService navigation, Func<object> createMultiplayerLobby, Func<object> createSinglePlayerLobby, Func<object> createPveLobby, Func<object> createSpineDemo, Func<object> createSpineBattle)
        {
            this.navigation = navigation;
            this.createMultiplayerLobby = createMultiplayerLobby;
            this.createSinglePlayerLobby = createSinglePlayerLobby;
            this.createPveLobby = createPveLobby;
            this.createSpineDemo = createSpineDemo;
            this.createSpineBattle = createSpineBattle;
            VsAiCommand = new RelayCommand(OpenVsAi);
            PveCommand = new RelayCommand(OpenPve);
            MultiplayerCommand = new RelayCommand(OpenMultiplayer);
            SpineTestCommand = new RelayCommand(OpenSpineDemo);
            SpineBattleCommand = new RelayCommand(OpenSpineBattle);
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

        private void OpenSpineDemo()
        {
            navigation.NavigateTo(createSpineDemo());
        }

        private void OpenSpineBattle()
        {
            navigation.NavigateTo(createSpineBattle());
        }

        private void Close()
        {
            Application.Current?.Shutdown();
        }
    }
}
