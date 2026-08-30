using System;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Lan.Contracts.Transport;

namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>Wire message types of the duel protocol.</summary>
    public static class DuelWire
    {
        /// <summary>The party config message type.</summary>
        public const string PartyConfig = "party_config";

        /// <summary>The readiness barrier message type.</summary>
        public const string PlayerLoaded = "player_loaded";

        /// <summary>The skill input RPC method name.</summary>
        public const string HeroSkill = "hero_skill";

        /// <summary>The prefix for input (RPC-style) messages.</summary>
        public const string RpcPrefix = "rpc.";

        /// <summary>Builds an RPC message type.</summary>
        /// <param name="method">The method name.</param>
        /// <returns>The message type.</returns>
        public static string Rpc(string method)
        {
            return RpcPrefix + method;
        }
    }

    /// <summary>Owns an <see cref="ITransport"/> session: host/join, pump, party exchange and readiness barrier.</summary>
    public class DuelSessionManager : IDisposable
    {
        private readonly ITransport transport;
        private bool rivalLoaded;

        /// <summary>Occurs when the local player created or joined a session.</summary>
        public event Action? SessionReady;

        /// <summary>Occurs when the rival party config arrives.</summary>
        public event Action<DuelPartyConfig>? RivalPartyReceived;

        /// <summary>Occurs when the rival signals readiness.</summary>
        public event Action? RivalLoaded;

        /// <summary>Occurs when an RPC input arrives from the rival (method, payload).</summary>
        public event Action<string, string>? RivalInputReceived;

        /// <summary>Occurs when the session is lost.</summary>
        public event Action? Disconnected;

        /// <summary>Gets the current session id (empty when not in a session).</summary>
        public string SessionId { get; private set; } = string.Empty;

        /// <summary>Gets the local player id.</summary>
        public string LocalPlayerId { get { return transport.LocalPlayerId; } }

        /// <summary>Gets the rival player id (empty when none).</summary>
        public string RivalPlayerId
        {
            get
            {
                var players = transport.GetSessionPlayers();
                return players.Length > 0 ? players[0] : string.Empty;
            }
        }

        /// <summary>Gets a value indicating whether the local player is the host.</summary>
        public bool IsHost
        {
            get
            {
                string hostId = transport.HostPlayerId;
                return hostId.Length > 0 && LocalPlayerId == hostId;
            }
        }

        /// <summary>Gets the rival party config if already received.</summary>
        public DuelPartyConfig? RivalParty { get; private set; }

        /// <summary>Gets a value indicating whether both parties are ready to start.</summary>
        public bool IsReady { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="DuelSessionManager"/> class.</summary>
        /// <param name="transport">The transport.</param>
        public DuelSessionManager(ITransport transport)
        {
            this.transport = transport;
            transport.SessionJoined += OnSessionJoined;
            transport.PlayerJoined += OnPlayerJoined;
            transport.MessageReceived += OnMessageReceived;
            transport.Disconnected += OnDisconnected;
        }

        /// <summary>Initializes the transport provider.</summary>
        /// <returns>The result.</returns>
        public Result Start()
        {
            return transport.Initialize();
        }

        /// <summary>Pumps transport callbacks and messages.</summary>
        public void Pump()
        {
            transport.RunCallbacks();
        }

        /// <summary>Hosts a new duel session.</summary>
        /// <param name="sessionName">The session name.</param>
        /// <returns>The result.</returns>
        public Result HostSession(string sessionName)
        {
            return transport.CreateSession(sessionName, 2);
        }

        /// <summary>Joins an existing session by its id.</summary>
        /// <param name="sessionId">The session id.</param>
        /// <returns>The result.</returns>
        public Result JoinSession(string sessionId)
        {
            return transport.JoinSession(sessionId);
        }

        /// <summary>Sends the local party config to the rival.</summary>
        /// <param name="config">The party config.</param>
        public void SendPartyConfig(DuelPartyConfig config)
        {
            transport.SendMessage(DuelWire.PartyConfig, config.Serialize());
        }

        /// <summary>Signals readiness to start the duel.</summary>
        public void SendLoaded()
        {
            transport.SendMessage(DuelWire.PlayerLoaded, "");
        }

        /// <summary>Sends an input (RPC-style) message.</summary>
        /// <param name="method">The method name.</param>
        /// <param name="payload">The payload.</param>
        public void SendInput(string method, string payload)
        {
            transport.SendMessage(DuelWire.Rpc(method), payload);
        }

        /// <summary>Leaves the session.</summary>
        public void Leave()
        {
            transport.LeaveSession();
        }

        private void OnSessionJoined(string sessionId)
        {
            SessionId = sessionId;
            SessionReady?.Invoke();
        }

        private void OnPlayerJoined(string playerId)
        {
            if (IsHost)
                SessionReady?.Invoke();
        }

        private void OnMessageReceived(TransportMessage message)
        {
            switch (message.Type)
            {
                case DuelWire.PartyConfig:
                    RivalParty = DuelPartyConfig.Deserialize(message.Payload);
                    UpdateReadiness();
                    RivalPartyReceived?.Invoke(RivalParty!);
                    break;
                case DuelWire.PlayerLoaded:
                    rivalLoaded = true;
                    UpdateReadiness();
                    RivalLoaded?.Invoke();
                    break;
                default:
                    if (message.Type.StartsWith(DuelWire.RpcPrefix))
                    {
                        string method = message.Type.Substring(DuelWire.RpcPrefix.Length);
                        RivalInputReceived?.Invoke(method, message.Payload);
                    }
                    break;
            }
        }

        private void UpdateReadiness()
        {
            IsReady = RivalParty != null && rivalLoaded;
        }

        private void OnDisconnected()
        {
            SessionId = string.Empty;
            IsReady = false;
            rivalLoaded = false;
            Disconnected?.Invoke();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            transport.SessionJoined -= OnSessionJoined;
            transport.PlayerJoined -= OnPlayerJoined;
            transport.MessageReceived -= OnMessageReceived;
            transport.Disconnected -= OnDisconnected;
            transport.Dispose();
        }
    }
}