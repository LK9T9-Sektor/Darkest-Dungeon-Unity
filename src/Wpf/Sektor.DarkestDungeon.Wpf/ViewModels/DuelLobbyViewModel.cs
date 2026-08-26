using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Lan.Contracts.Transport;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Duel lobby: hero selection, host/join, party exchange and duel start.</summary>
    public partial class DuelLobbyViewModel : ObservableObject, IDisposable
    {
        private readonly DuelSessionManager session;
        private DuelBattleViewModel? activeBattle;

        /// <summary>Occurs when the duel starts with a ready controller and battle view model.</summary>
        public event Action<DuelController, DuelBattleViewModel>? DuelStarted;

        /// <summary>Gets the four hero slots.</summary>
        public System.Collections.ObjectModel.ObservableCollection<HeroSlotViewModel> Slots { get; } =
            new System.Collections.ObjectModel.ObservableCollection<HeroSlotViewModel>();

        /// <summary>Gets or sets the connection status text.</summary>
        [ObservableProperty]
        private string _status = "Pick heroes, then Host or Join.";

        /// <summary>Gets or sets the session id input/display.</summary>
        [ObservableProperty]
        private string _sessionIdText = string.Empty;

        /// <summary>Pumps the transport callbacks (called by a UI timer).</summary>
        public void Pump()
        {
            session.Pump();
        }

        /// <summary>Gets the command that hosts a new duel room.</summary>
        public IRelayCommand HostCommand { get; }

        /// <summary>Gets the command that joins an existing room.</summary>
        public IRelayCommand JoinCommand { get; }

        /// <summary>Gets the command that copies the session id.</summary>
        public IRelayCommand CopyIdCommand { get; }

        /// <summary>Gets the command that disconnects from the room.</summary>
        public IRelayCommand LeaveCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="DuelLobbyViewModel"/> class.</summary>
        /// <param name="transport">The duel transport.</param>
        public DuelLobbyViewModel(ITransport transport)
        {
            session = new DuelSessionManager(transport);
            for (int i = 0; i < 4; i++)
                Slots.Add(new HeroSlotViewModel(i * 10 + 1));

            session.SessionReady += OnSessionReady;
            session.RivalPartyReceived += OnRivalPartyReceived;
            session.RivalLoaded += OnRivalLoaded;
            session.RivalInputReceived += OnRivalInputReceived;
            session.Disconnected += () => Status = "Disconnected from the session.";

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
            session.Leave();
            Status = "Left the session.";
        }

        private void OnSessionReady()
        {
            if (session.IsHost)
            {
                Status = "Room created. Waiting for a rival...";
                SessionIdText = session.SessionId;
                SendPartyConfig();
            }
            else
            {
                Status = "Joined. Waiting for the host...";
                SendPartyConfig();
            }
        }

        private void OnRivalPartyReceived(DuelPartyConfig rivalConfig)
        {
            Status = "Rival party received (" + rivalConfig.ClassIds.Count + " heroes).";
            if (session.IsHost)
                session.SendLoaded();
        }

        private void OnRivalLoaded()
        {
            TryStartDuel();
        }

        private void OnRivalInputReceived(string method, string payload)
        {
            activeBattle?.ApplyRivalInput(payload);
        }

        private void SendPartyConfig()
        {
            session.SendPartyConfig(BuildConfig());
        }

        private void TryStartDuel()
        {
            if (!session.IsReady || session.RivalParty == null)
                return;

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
            activeBattle = new DuelBattleViewModel(duel, session.SendInput);
            Status = "Duel started. Round 1.";
            DuelStarted?.Invoke(duel, activeBattle);
        }

        private DuelPartyConfig BuildConfig()
        {
            var classIds = Slots.Select(s => s.ClassId).ToList();
            var seeds = Slots.Select(s => s.Seed).ToList();
            return new DuelPartyConfig(classIds, seeds);
        }

        private static DuelHeroPick[] ToPicks(DuelPartyConfig config)
        {
            var picks = new List<DuelHeroPick>();
            for (int i = 0; i < config.ClassIds.Count; i++)
                picks.Add(new DuelHeroPick(config.ClassIds[i], config.Seeds[i]));
            return picks.ToArray();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            session.Dispose();
        }
    }
}