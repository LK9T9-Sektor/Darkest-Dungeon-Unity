namespace Sektor.DarkestDungeon.Lan.Contracts.Transport
{
    using Sektor.DarkestDungeon.Lan.Contracts.Results;

    /// <summary>
    /// Message transport abstraction that isolates game logic from any concrete network provider.
    /// The transport only delivers bytes (as messages); it never owns game state.
    /// </summary>
    public interface ITransport : System.IDisposable
    {
        /// <summary>Raised when the local player successfully created or joined a session; carries the session id.</summary>
        event System.Action<string> SessionJoined;

        /// <summary>Raised when another player joins the session; carries the player id.</summary>
        event System.Action<string> PlayerJoined;

        /// <summary>Raised when another player leaves the session; carries the player id.</summary>
        event System.Action<string> PlayerLeft;

        /// <summary>Raised when a message arrives from any session participant.</summary>
        event System.Action<TransportMessage> MessageReceived;

        /// <summary>Raised when the session is lost or the transport disconnects unexpectedly.</summary>
        event System.Action Disconnected;

        /// <summary>Gets the opaque identifier of the local player.</summary>
        string LocalPlayerId { get; }

        /// <summary>Gets a value indicating whether the transport is currently in an active session.</summary>
        bool IsSessionActive { get; }

        /// <summary>Initializes the provider connection; must be called before any session call.</summary>
        Result Initialize();

        /// <summary>
        /// Pumps provider callbacks and drains incoming messages. Must be called regularly
        /// (every frame in a game loop) for callbacks and messages to be delivered.
        /// </summary>
        void RunCallbacks();

        /// <summary>Creates a new session with the local player as the host.</summary>
        Result CreateSession(string sessionName, int maxPlayers);

        /// <summary>Joins an existing session by its identifier.</summary>
        Result JoinSession(string sessionId);

        /// <summary>Leaves the current session.</summary>
        Result LeaveSession();

        /// <summary>Sends a message to all other session participants; reliable and ordered.</summary>
        Result SendMessage(string type, string payload);

        /// <summary>Gets the identifiers of all players currently in the session, excluding the local player.</summary>
        string[] GetSessionPlayers();
    }
}
