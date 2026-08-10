namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    /// <summary>
    /// Steamworks interface version strings and callback identity base constants.
    /// Values mirror the Steamworks SDK headers and the vendored Steamworks.NET reference.
    /// </summary>
    internal static class SteamConstants
    {
        /// <summary>ISteamClient interface version.</summary>
        internal const string SteamClientInterfaceVersion = "SteamClient021";

        /// <summary>ISteamUser interface version.</summary>
        internal const string SteamUserInterfaceVersion = "SteamUser023";

        /// <summary>ISteamMatchmaking interface version.</summary>
        internal const string SteamMatchmakingInterfaceVersion = "SteamMatchMaking009";

        /// <summary>ISteamNetworking interface version.</summary>
        internal const string SteamNetworkingInterfaceVersion = "SteamNetworking006";

        /// <summary>Size in bytes of the native SteamErrMsg buffer (k_cchMaxSteamErrMsg).</summary>
        internal const int SteamApiMaxErrorLength = 1024;

        /// <summary>Base callback identity for matchmaking callbacks.</summary>
        internal const int SteamMatchmakingCallbacks = 500;

        /// <summary>Base callback identity for networking callbacks.</summary>
        internal const int SteamNetworkingCallbacks = 1200;
    }
}
