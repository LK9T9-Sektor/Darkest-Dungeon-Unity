namespace Sektor.DarkestDungeon.Lan.Tests.Support
{
    using System;
    using System.Collections.Generic;

    using Sektor.DarkestDungeon.Lan.Contracts.Results;
    using Sektor.DarkestDungeon.Lan.Contracts.Transport;

    /// <summary>
    /// In-memory <see cref="ITransport"/> test double: two linked instances deliver messages
    /// synchronously, which lets unit tests exercise the transport contract without a network.
    /// </summary>
    public sealed class InMemoryTransport : ITransport
    {
        private readonly string _playerId;
        private InMemoryTransport _peer;
        private string _sessionId;

        /// <summary>Creates a transport representing the given player.</summary>
        public InMemoryTransport(string playerId)
        {
            _playerId = playerId;
        }

        /// <inheritdoc />
        public event Action<string> SessionJoined;

        /// <inheritdoc />
        public event Action<string> PlayerJoined;

        /// <inheritdoc />
        public event Action<string> PlayerLeft;

        /// <inheritdoc />
        public event Action<TransportMessage> MessageReceived;

        /// <inheritdoc />
        public event Action Disconnected;

        /// <inheritdoc />
        public string LocalPlayerId
        {
            get { return _playerId; }
        }

        /// <inheritdoc />
        public bool IsSessionActive { get; private set; }

        /// <summary>Connects this transport to another instance for direct delivery.</summary>
        public void LinkTo(InMemoryTransport peer)
        {
            _peer = peer;
        }

        /// <inheritdoc />
        public Result Initialize()
        {
            return Result.Success();
        }

        /// <inheritdoc />
        public void RunCallbacks()
        {
        }

        /// <inheritdoc />
        public Result CreateSession(string sessionName, int maxPlayers)
        {
            if (IsSessionActive)
            {
                return Result.Failure("Already in a session.");
            }

            _sessionId = sessionName;
            IsSessionActive = true;
            Action<string> joined = SessionJoined;
            if (joined != null)
            {
                joined(sessionName);
            }

            return Result.Success();
        }

        /// <inheritdoc />
        public Result JoinSession(string sessionId)
        {
            if (IsSessionActive)
            {
                return Result.Failure("Already in a session.");
            }

            if (_peer == null || !_peer.IsSessionActive)
            {
                return Result.Failure("Host has no active session.");
            }

            _sessionId = sessionId;
            IsSessionActive = true;
            Action<string> joined = SessionJoined;
            if (joined != null)
            {
                joined(sessionId);
            }

            _peer.NotifyPlayerJoined(_playerId);
            NotifyPlayerJoined(_peer._playerId);
            return Result.Success();
        }

        /// <inheritdoc />
        public Result LeaveSession()
        {
            if (!IsSessionActive)
            {
                return Result.Failure("Not in a session.");
            }

            IsSessionActive = false;
            _sessionId = null;
            if (_peer != null)
            {
                _peer.NotifyPlayerLeft(_playerId);
            }

            return Result.Success();
        }

        /// <inheritdoc />
        public Result SendMessage(string type, string payload)
        {
            if (!IsSessionActive)
            {
                return Result.Failure("Not in a session.");
            }

            if (_peer == null)
            {
                return Result.Failure("No peer linked.");
            }

            _peer.NotifyMessageReceived(new TransportMessage(_playerId, type, payload));
            return Result.Success();
        }

        /// <inheritdoc />
        public string[] GetSessionPlayers()
        {
            List<string> players = new List<string>();
            if (!IsSessionActive || _peer == null)
            {
                return players.ToArray();
            }

            players.Add(_peer._playerId);
            return players.ToArray();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_peer != null)
            {
                _peer.NotifyDisconnected();
            }

            _peer = null;
            IsSessionActive = false;
        }

        internal void NotifyDisconnected()
        {
            Action disconnected = Disconnected;
            if (disconnected != null)
            {
                disconnected();
            }
        }

        internal void NotifyPlayerJoined(string playerId)
        {
            Action<string> joined = PlayerJoined;
            if (joined != null)
            {
                joined(playerId);
            }
        }

        internal void NotifyPlayerLeft(string playerId)
        {
            Action<string> left = PlayerLeft;
            if (left != null)
            {
                left(playerId);
            }
        }

        internal void NotifyMessageReceived(TransportMessage message)
        {
            Action<TransportMessage> received = MessageReceived;
            if (received != null)
            {
                received(message);
            }
        }
    }
}
