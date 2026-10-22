using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Main menu: choose a duel against the AI or a multiplayer duel.</summary>
    public partial class MainMenuViewModel : ObservableObject
    {
        private readonly INavigationService navigation;
        private readonly Func<object> createDuelLobby;
        private readonly Func<object> createMultiplayerLobby;
        private readonly Func<object> createSpineDemo;
        private readonly Func<object> createSpineBattle;
        private readonly Func<object> createEstateReplay;

        /// <summary>Gets the command opening the single-player duel lobby.</summary>
        public IRelayCommand DuelCommand { get; }

        /// <summary>Gets the command opening the multiplayer lobby.</summary>
        public IRelayCommand MultiplayerCommand { get; }

        /// <summary>Gets the command opening the Spine animation test screen.</summary>
        public IRelayCommand SpineTestCommand { get; }

        /// <summary>Gets the command opening the visual 1x1 spine arena.</summary>
        public IRelayCommand SpineBattleCommand { get; }

        /// <summary>Gets the command opening the estate capture replay.</summary>
        public IRelayCommand EstateReplayCommand { get; }

        /// <summary>Gets the command that closes the application.</summary>
        public IRelayCommand CloseCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="MainMenuViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="createDuelLobby">Creates the single-player duel lobby screen.</param>
        /// <param name="createMultiplayerLobby">Creates the multiplayer lobby screen.</param>
        /// <param name="createSpineDemo">Creates the Spine animation test screen.</param>
        /// <param name="createSpineBattle">Creates the visual 1x1 spine arena screen.</param>
        /// <param name="createEstateReplay">Creates the estate capture replay screen.</param>
        public MainMenuViewModel(
            INavigationService navigation,
            Func<object> createDuelLobby,
            Func<object> createMultiplayerLobby,
            Func<object> createSpineDemo,
            Func<object> createSpineBattle,
            Func<object> createEstateReplay)
        {
            this.navigation = navigation;
            this.createDuelLobby = createDuelLobby;
            this.createMultiplayerLobby = createMultiplayerLobby;
            this.createSpineDemo = createSpineDemo;
            this.createSpineBattle = createSpineBattle;
            this.createEstateReplay = createEstateReplay;
            DuelCommand = new RelayCommand(OpenDuel);
            MultiplayerCommand = new RelayCommand(OpenMultiplayer);
            SpineTestCommand = new RelayCommand(OpenSpineDemo);
            SpineBattleCommand = new RelayCommand(OpenSpineBattle);
            EstateReplayCommand = new RelayCommand(OpenEstateReplay);
            CloseCommand = new RelayCommand(Close);
        }

        private void OpenDuel()
        {
            navigation.NavigateTo(createDuelLobby());
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

        private void OpenEstateReplay()
        {
            navigation.NavigateTo(createEstateReplay());
        }

        private void Close()
        {
            Application.Current?.Shutdown();
        }
    }
}
