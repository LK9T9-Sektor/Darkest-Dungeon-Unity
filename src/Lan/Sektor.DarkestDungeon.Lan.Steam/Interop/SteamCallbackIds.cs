namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    /// <summary>
    /// Callback identifiers of the callbacks handled by the transport.
    /// Values mirror the k_iCallback constants of the matching structures
    /// in the Steamworks SDK headers.
    /// </summary>
    internal static class SteamCallbackIds
    {
        /// <summary>Result of a CreateLobby request.</summary>
        internal const int LobbyCreated = SteamConstants.SteamMatchmakingCallbacks + 13;

        /// <summary>Result of joining a lobby.</summary>
        internal const int LobbyEnter = SteamConstants.SteamMatchmakingCallbacks + 4;

        /// <summary>A lobby member's state changed.</summary>
        internal const int LobbyChatUpdate = SteamConstants.SteamMatchmakingCallbacks + 6;

        /// <summary>A remote host wants to start a P2P session.</summary>
        internal const int P2PSessionRequest = SteamConstants.SteamNetworkingCallbacks + 2;

        /// <summary>A P2P session failed to connect.</summary>
        internal const int P2PSessionConnectFail = SteamConstants.SteamNetworkingCallbacks + 3;
    }
}
