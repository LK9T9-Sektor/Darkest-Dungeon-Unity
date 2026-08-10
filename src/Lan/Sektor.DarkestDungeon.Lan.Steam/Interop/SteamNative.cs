namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Minimal P/Invoke bindings to the Steamworks flat API (steam_api64).
    /// Only the subset required by <see cref="SteamTransport"/> is bound.
    /// The native function signatures and callback layouts mirror the Steamworks
    /// SDK headers; src/External/Steamworks.NET is the vendored reference material.
    /// </summary>
    internal static class SteamNative
    {
        private const string NativeLibraryName = "steam_api64";

        // Lifecycle ------------------------------------------------------------------

        [DllImport(NativeLibraryName, EntryPoint = "SteamInternal_SteamAPI_Init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ESteamAPIInitResult SteamInternal_SteamAPI_Init(IntPtr pszInternalCheckInterfaceVersions, IntPtr pOutErrMsg);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_Shutdown", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SteamAPI_Shutdown();

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_GetHSteamPipe", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int SteamAPI_GetHSteamPipe();

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_GetHSteamUser", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int SteamAPI_GetHSteamUser();

        [DllImport(NativeLibraryName, EntryPoint = "SteamInternal_CreateInterface", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr SteamInternal_CreateInterface(string pchVersion);

        // Manual callback dispatch ---------------------------------------------------

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ManualDispatch_Init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SteamAPI_ManualDispatch_Init();

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ManualDispatch_RunFrame", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SteamAPI_ManualDispatch_RunFrame(int hSteamPipe);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ManualDispatch_GetNextCallback", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool SteamAPI_ManualDispatch_GetNextCallback(int hSteamPipe, out CallbackMsg_t pCallbackMsg);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ManualDispatch_FreeLastCallback", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SteamAPI_ManualDispatch_FreeLastCallback(int hSteamPipe);

        // ISteamClient ---------------------------------------------------------------

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamClient_GetISteamMatchmaking", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ISteamClient_GetISteamMatchmaking(IntPtr instancePtr, int hSteamUser, int hSteamPipe, string pchVersion);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamClient_GetISteamNetworking", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ISteamClient_GetISteamNetworking(IntPtr instancePtr, int hSteamUser, int hSteamPipe, string pchVersion);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamClient_GetISteamUser", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ISteamClient_GetISteamUser(IntPtr instancePtr, int hSteamUser, int hSteamPipe, string pchVersion);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamClient_GetISteamFriends", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ISteamClient_GetISteamFriends(IntPtr instancePtr, int hSteamUser, int hSteamPipe, string pchVersion);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamClient_GetISteamUtils", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ISteamClient_GetISteamUtils(IntPtr instancePtr, int hSteamUser, int hSteamPipe, string pchVersion);

        // ISteamMatchmaking ----------------------------------------------------------

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamMatchmaking_CreateLobby", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong ISteamMatchmaking_CreateLobby(IntPtr instancePtr, int eLobbyType, int cMaxMembers);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamMatchmaking_JoinLobby", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong ISteamMatchmaking_JoinLobby(IntPtr instancePtr, ulong steamIDLobby);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamMatchmaking_LeaveLobby", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ISteamMatchmaking_LeaveLobby(IntPtr instancePtr, ulong steamIDLobby);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamMatchmaking_GetNumLobbyMembers", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ISteamMatchmaking_GetNumLobbyMembers(IntPtr instancePtr, ulong steamIDLobby);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamMatchmaking_GetLobbyMemberByIndex", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong ISteamMatchmaking_GetLobbyMemberByIndex(IntPtr instancePtr, ulong steamIDLobby, int iMember);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamMatchmaking_SetLobbyData", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool ISteamMatchmaking_SetLobbyData(IntPtr instancePtr, ulong steamIDLobby, IntPtr pchKey, IntPtr pchValue);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamMatchmaking_GetLobbyData", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr ISteamMatchmaking_GetLobbyData(IntPtr instancePtr, ulong steamIDLobby, IntPtr pchKey);

        // ISteamNetworking -----------------------------------------------------------

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamNetworking_SendP2PPacket", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool ISteamNetworking_SendP2PPacket(IntPtr instancePtr, ulong steamIDRemote, byte[] pubData, uint cubData, int eP2PSendType, int nChannel);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamNetworking_IsP2PPacketAvailable", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool ISteamNetworking_IsP2PPacketAvailable(IntPtr instancePtr, out uint pcubMsgSize, int nChannel);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamNetworking_ReadP2PPacket", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool ISteamNetworking_ReadP2PPacket(IntPtr instancePtr, byte[] pubDest, uint cubDest, out uint pcubMsgSize, out ulong psteamIDRemote, int nChannel);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamNetworking_AcceptP2PSessionWithUser", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool ISteamNetworking_AcceptP2PSessionWithUser(IntPtr instancePtr, ulong steamIDRemote);

        // ISteamUser -----------------------------------------------------------------

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamUser_GetSteamID", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong ISteamUser_GetSteamID(IntPtr instancePtr);

        // ISteamFriends --------------------------------------------------------------

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamFriends_SetRichPresence", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool ISteamFriends_SetRichPresence(IntPtr instancePtr, IntPtr pchKey, IntPtr pchValue);

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamFriends_ClearRichPresence", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ISteamFriends_ClearRichPresence(IntPtr instancePtr);

        // ISteamUtils ----------------------------------------------------------------

        [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamUtils_GetAppID", CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint ISteamUtils_GetAppID(IntPtr instancePtr);
    }
}
