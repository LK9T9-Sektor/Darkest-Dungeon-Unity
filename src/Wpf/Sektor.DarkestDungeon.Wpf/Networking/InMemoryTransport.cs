using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Lan.Contracts.Results;
using Sektor.DarkestDungeon.Lan.Contracts.Transport;

namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>In-memory <see cref="ITransport"/> for local dev and tests: two linked instances exchange messages synchronously.</summary>
    public sealed class InMemoryTransport : ITransport
    {
        private readonly string _playerId;
        private InMemoryTransport _peer;
        private string _sessionId;
        private string _hostId;

        /// <summary>Creates a transport representing the given player.</summary>
        /// <param name="playerId">The player id.</param>
        public InMemoryTransport(string playerId)
        {
            _playerId = playerId;
        }

        /// <inheritdoc/>
        public event Action<string> SessionJoined;

        /// <inheritdoc/>
        public event Action<string> PlayerJoined;

        /// <inheritdoc/>
        public event Action<string> PlayerLeft;

        /// <inheritdoc/>
        public event Action<TransportMessage> MessageReceived;

        /// <inheritdoc/>
        public event Action<string> SessionInviteReceived;

        /// <inheritdoc/>
        public event Action Disconnected;

        /// <inheritdoc/>
        public string LocalPlayerId { get { return _playerId; } }

        /// <inheritdoc/>
        public string HostPlayerId { get { return _hostId ?? string.Empty; } }

        /// <inheritdoc/>
        public bool IsSessionActive { get; private set; }

        /// <summary>Links this transport to a peer for direct delivery.</summary>
        /// <param name="peer">The peer.</param>
        public void LinkTo(InMemoryTransport peer)
        {
            _peer = peer;
        }

        /// <inheritdoc/>
        public Result Initialize()
        {
            return Result.Success();
        }

        /// <inheritdoc/>
        public void RunCallbacks()
        {
        }

        /// <inheritdoc/>
        public Result CreateSession(string sessionName, int maxPlayers)
        {
            if (IsSessionActive)
                return Result.Failure("Already in a session.");

            _sessionId = sessionName;
            _hostId = _playerId;
            IsSessionActive = true;
            SessionJoined?.Invoke(sessionName);
            return Result.Success();
        }

        /// <inheritdoc/>
        public Result JoinSession(string sessionId)
        {
            if (IsSessionActive)
                return Result.Failure("Already in a session.");

            if (_peer == null || !_peer.IsSessionActive)
                return Result.Failure("Host has no active session.");

            _sessionId = sessionId;
            _hostId = _peer._hostId;
            IsSessionActive = true;
            SessionJoined?.Invoke(sessionId);

            _peer.NotifyPlayerJoined(_playerId);
            NotifyPlayerJoined(_peer._playerId);
            return Result.Success();
        }

        /// <inheritdoc/>
        public Result LeaveSession()
        {
            if (!IsSessionActive)
                return Result.Failure("Not in a session.");

            IsSessionActive = false;
            _sessionId = null;
            _peer?.NotifyPlayerLeft(_playerId);
            return Result.Success();
        }

        /// <inheritdoc/>
        public Result SendMessage(string type, string payload)
        {
            if (!IsSessionActive)
                return Result.Failure("Not in a session.");

            if (_peer == null)
                return Result.Failure("No peer linked.");

            _peer.NotifyMessageReceived(new TransportMessage(_playerId, type, payload));
            return Result.Success();
        }

        /// <inheritdoc/>
        public string[] GetSessionPlayers()
        {
            var players = new List<string>();
            if (IsSessionActive && _peer != null)
                players.Add(_peer._playerId);
            return players.ToArray();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _peer?.NotifyDisconnected();
            _peer = null;
            IsSessionActive = false;
        }

        internal void NotifyDisconnected()
        {
            Disconnected?.Invoke();
        }

        internal void NotifyPlayerJoined(string playerId)
        {
            PlayerJoined?.Invoke(playerId);
        }

        internal void NotifyPlayerLeft(string playerId)
        {
            PlayerLeft?.Invoke(playerId);
        }

        internal void NotifyMessageReceived(TransportMessage message)
        {
            MessageReceived?.Invoke(message);
        }

        internal void NotifyInviteReceived(string sessionId)
        {
            SessionInviteReceived?.Invoke(sessionId);
        }
    }
}