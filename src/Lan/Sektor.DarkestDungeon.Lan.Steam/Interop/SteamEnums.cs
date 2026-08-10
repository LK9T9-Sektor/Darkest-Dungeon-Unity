namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    /// <summary>Result codes returned by Steamworks operations (EResult).</summary>
    internal enum EResult
    {
        /// <summary>Success.</summary>
        OK = 1,

        /// <summary>Generic failure.</summary>
        Fail = 2,

        /// <summary>No or failed network connection.</summary>
        NoConnection = 3,

        /// <summary>Called method busy; action not taken.</summary>
        Busy = 10,

        /// <summary>Access is denied.</summary>
        AccessDenied = 15,

        /// <summary>Operation timed out.</summary>
        Timeout = 16,

        /// <summary>The user is not logged on.</summary>
        NotLoggedOn = 21,

        /// <summary>Too much of a good thing.</summary>
        LimitExceeded = 25,

        /// <summary>Temporary rate limit exceeded; retry later.</summary>
        RateLimitExceeded = 84
    }

    /// <summary>Lobby visibility modes (ELobbyType).</summary>
    internal enum ELobbyType
    {
        /// <summary>Only joinable via a direct invite.</summary>
        Private = 0,

        /// <summary>Shown to friends or invitees, not in the lobby list.</summary>
        FriendsOnly = 1,

        /// <summary>Visible to friends and in the lobby list.</summary>
        Public = 2,

        /// <summary>Returned by search, but not visible to friends.</summary>
        Invisible = 3
    }

    /// <summary>P2P packet send modes (EP2PSend).</summary>
    internal enum EP2PSend
    {
        /// <summary>Basic UDP send; packets can be lost or reordered.</summary>
        Unreliable = 0,

        /// <summary>UDP without buffering; dropped if the connection is not open.</summary>
        UnreliableNoDelay = 1,

        /// <summary>Reliable, ordered message send.</summary>
        Reliable = 2,

        /// <summary>Reliable sends with explicit flushing control.</summary>
        ReliableWithBuffering = 3
    }

    /// <summary>Lobby enter response codes (EChatRoomEnterResponse).</summary>
    internal enum EChatRoomEnterResponse
    {
        /// <summary>Join succeeded.</summary>
        Success = 1
    }

    /// <summary>Lobby member state change flags (EChatMemberStateChange).</summary>
    [System.Flags]
    internal enum EChatMemberStateChange
    {
        /// <summary>This user has joined the lobby.</summary>
        Entered = 0x0001,

        /// <summary>This user has left the lobby.</summary>
        Left = 0x0002,

        /// <summary>User disconnected without leaving.</summary>
        Disconnected = 0x0004,

        /// <summary>User was kicked.</summary>
        Kicked = 0x0008,

        /// <summary>User was kicked and banned.</summary>
        Banned = 0x0010
    }

    /// <summary>P2P session error codes (EP2PSessionError).</summary>
    internal enum EP2PSessionError
    {
        /// <summary>No error.</summary>
        None = 0,

        /// <summary>Local user does not own the running app.</summary>
        NoRightsToApp = 2,

        /// <summary>Target is not responding.</summary>
        Timeout = 4
    }
}
