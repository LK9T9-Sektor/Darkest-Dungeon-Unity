namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    /// <summary>
    /// Steamworks interface version strings and callback identity base constants.
    /// Values mirror the Steamworks SDK headers and the vendored Steamworks.NET reference.
    /// The running Steam client decides which interface versions it exposes, so instead of a
    /// single hardcoded version each interface is resolved by probing candidates in order.
    /// </summary>
    internal static class SteamConstants
    {
        /// <summary>ISteamClient version candidates, newest first.</summary>
        internal static readonly string[] SteamClientCandidates =
        {
            "SteamClient022",
            "SteamClient021",
            "SteamClient020",
            "SteamClient017",
        };

        /// <summary>ISteamUser version candidates, newest first.</summary>
        internal static readonly string[] SteamUserCandidates =
        {
            "SteamUser023",
            "SteamUser022",
            "SteamUser021",
            "SteamUser020",
            "SteamUser019",
        };

        /// <summary>ISteamMatchmaking version candidates, newest first.</summary>
        internal static readonly string[] SteamMatchmakingCandidates =
        {
            "SteamMatchMaking009",
            "SteamMatchMaking008",
        };

        /// <summary>ISteamNetworking version candidates, newest first.</summary>
        internal static readonly string[] SteamNetworkingCandidates =
        {
            "SteamNetworking006",
            "SteamNetworking005",
        };

        /// <summary>Size in bytes of the native SteamErrMsg buffer (k_cchMaxSteamErrMsg).</summary>
        internal const int SteamApiMaxErrorLength = 1024;

        /// <summary>Base callback identity for matchmaking callbacks.</summary>
        internal const int SteamMatchmakingCallbacks = 500;

        /// <summary>Base callback identity for networking callbacks.</summary>
        internal const int SteamNetworkingCallbacks = 1200;

        /// <summary>Base callback identity for friends callbacks.</summary>
        internal const int SteamFriendsCallbacks = 300;
    }
}
