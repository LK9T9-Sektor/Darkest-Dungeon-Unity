using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Lan.Contracts.Transport;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Navigation;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Multiplayer duel lobby: hero selection, host/join, party exchange and duel start.</summary>
    public partial class DuelLobbyViewModel : ObservableObject, IPumpable, IDisposable
    {
        private readonly DuelSessionManager session;
        private readonly INavigationService navigation;
        private readonly IReadOnlyList<string> availableClasses;
        private DateTime? waitingSince;
        private bool started;
        private bool disposed;

        /// <summary>Gets the four hero slots.</summary>
        public ObservableCollection<HeroSlotViewModel> Slots { get; } = new ObservableCollection<HeroSlotViewModel>();

        /// <summary>Gets or sets the connection status text.</summary>
        [ObservableProperty]
        private string _status = "Pick heroes, then Host or Join.";

        /// <summary>Gets or sets the session id input/display.</summary>
        [ObservableProperty]
        private string _sessionIdText = string.Empty;

        /// <summary>Gets or sets the elapsed waiting time for the rival (mm:ss).</summary>
        [ObservableProperty]
        private string _waitingTime = string.Empty;

        /// <summary>Pumps the transport callbacks (called by a UI timer).</summary>
        public void Pump()
        {
            if (disposed)
                return;
            session.Pump();
            UpdateWaitingTime();
        }

        /// <summary>Gets the command that hosts a new duel room.</summary>
        public IRelayCommand HostCommand { get; }

        /// <summary>Gets the command that joins an existing room.</summary>
        public IRelayCommand JoinCommand { get; }

        /// <summary>Gets the command that copies the session id.</summary>
        public IRelayCommand CopyIdCommand { get; }

        /// <summary>Gets the command that disconnects and returns to the main menu.</summary>
        public IRelayCommand LeaveCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="DuelLobbyViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="transport">The duel transport.</param>
        /// <param name="availableClasses">The selectable hero class ids.</param>
        public DuelLobbyViewModel(INavigationService navigation, ITransport transport, IReadOnlyList<string> availableClasses)
        {
            this.navigation = navigation;
            this.availableClasses = availableClasses;
            session = new DuelSessionManager(transport);
            for (int i = 0; i < 4; i++)
                Slots.Add(new HeroSlotViewModel(i * 10 + 1, availableClasses));

            session.SessionReady += OnSessionReady;
            session.RivalPartyReceived += OnRivalPartyReceived;
            session.RivalLoaded += OnRivalLoaded;
            session.Disconnected += OnDisconnected;

            HostCommand = new RelayCommand(Host);
            JoinCommand = new RelayCommand(Join);
            CopyIdCommand = new RelayCommand(CopyId);
            LeaveCommand = new RelayCommand(Leave);
        }

        private void Host()
        {
            var result = session.Start();
            if (!result.IsSuccess)
            {
                Status = "Steam init failed: " + result.ErrorMessage;
                return;
            }
            session.HostSession("duel-" + Guid.NewGuid().ToString("N").Substring(0, 8));
        }

        private void Join()
        {
            if (string.IsNullOrWhiteSpace(SessionIdText))
            {
                Status = "Enter a session id first.";
                return;
            }
            var result = session.Start();
            if (!result.IsSuccess)
            {
                Status = "Steam init failed: " + result.ErrorMessage;
                return;
            }
            var join = session.JoinSession(SessionIdText.Trim());
            if (!join.IsSuccess)
                Status = "Join failed: " + join.ErrorMessage;
        }

        private void CopyId()
        {
            if (SessionIdText.Length > 0)
                Clipboard.SetText(SessionIdText);
        }

        private void Leave()
        {
            Dispose();
            navigation.GoHome();
        }

        private void OnSessionReady()
        {
            Status = session.IsHost ? "Room created. Waiting for a rival..." : "Joined. Waiting for the host...";
            SessionIdText = session.SessionId;
            waitingSince = DateTime.UtcNow;
            WaitingTime = "Waiting 00:00";
            SendPartyConfig();
        }

        private void OnRivalPartyReceived(DuelPartyConfig rivalConfig)
        {
            Status = "Rival party received (" + rivalConfig.ClassIds.Count + " heroes).";
            session.SendLoaded();
        }

        private void OnRivalLoaded()
        {
            TryStartDuel();
        }

        private void OnDisconnected()
        {
            waitingSince = null;
            WaitingTime = string.Empty;
        }

        private void SendPartyConfig()
        {
            session.SendPartyConfig(BuildConfig());
        }

        private void TryStartDuel()
        {
            if (started || !session.IsReady || session.RivalParty == null)
                return;
            started = true;
            waitingSince = null;
            WaitingTime = string.Empty;

            string[] orderedIds = session.LocalPlayerId == string.Empty
                ? new[] { "local", "rival" }
                : new[] { session.LocalPlayerId, session.RivalPlayerId };
            int sessionSeed = DuelSeed.ComputeSessionSeed(orderedIds);

            var duel = new DuelController();
            if (session.IsHost)
                duel.StartDuel(ToPicks(BuildConfig()), ToPicks(session.RivalParty), sessionSeed, isHost: true);
            else
                duel.StartDuel(ToPicks(session.RivalParty), ToPicks(BuildConfig()), sessionSeed, isHost: false);

            duel.StartBattle();

            var link = new NetworkRivalLink(session);
            var battle = new DuelBattleViewModel(duel, link, () =>
            {
                Dispose();
                navigation.GoHome();
            });
            Status = "Duel started. Round 1.";
            navigation.NavigateTo(battle);
        }

        private DuelPartyConfig BuildConfig()
        {
            var classIds = Slots.Select(s => s.ClassId).ToList();
            var seeds = Slots.Select(s => s.Seed).ToList();
            var skillIds = Slots.Select(s => s.SelectedSkillIds).ToList();
            return new DuelPartyConfig(classIds, seeds, skillIds);
        }

        private static DuelHeroPick[] ToPicks(DuelPartyConfig config)
        {
            var picks = new List<DuelHeroPick>();
            for (int i = 0; i < config.ClassIds.Count; i++)
                picks.Add(new DuelHeroPick(
                    config.ClassIds[i],
                    config.Seeds[i],
                    i < config.SelectedSkillIds.Count ? config.SelectedSkillIds[i] : null));
            return picks.ToArray();
        }

        private void UpdateWaitingTime()
        {
            if (waitingSince == null)
                return;
            var elapsed = DateTime.UtcNow - waitingSince.Value;
            WaitingTime = "Waiting " + ((int)elapsed.TotalMinutes).ToString("00") + ":" + elapsed.Seconds.ToString("00");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            session.Dispose();
        }
    }
}
