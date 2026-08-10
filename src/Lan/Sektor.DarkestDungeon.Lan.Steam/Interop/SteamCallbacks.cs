namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>Native callback dispatch message (CallbackMsg_t).</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CallbackMsg_t
    {
        /// <summary>Specific user to whom this callback applies.</summary>
        internal int m_hSteamUser;

        /// <summary>Callback identifier; corresponds to the structure's callback constant.</summary>
        internal int m_iCallback;

        /// <summary>Pointer to the callback structure.</summary>
        internal IntPtr m_pubParam;

        /// <summary>Size of the data pointed to by <see cref="m_pubParam"/>.</summary>
        internal int m_cubParam;
    }

    /// <summary>Result of a CreateLobby request (callback 513).</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LobbyCreated_t
    {
        /// <summary>k_EResultOK when the lobby was successfully created.</summary>
        internal EResult m_eResult;

        /// <summary>Lobby id; zero when creation failed.</summary>
        internal ulong m_ulSteamIDLobby;
    }

    /// <summary>Result of joining a lobby (callback 504).</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LobbyEnter_t
    {
        /// <summary>SteamID of the lobby that was entered.</summary>
        internal ulong m_ulSteamIDLobby;

        /// <summary>Permissions of the current user.</summary>
        internal uint m_rgfChatPermissions;

        /// <summary>True when only invited users may join.</summary>
        [MarshalAs(UnmanagedType.I1)]
        internal bool m_bLocked;

        /// <summary>EChatRoomEnterResponse value.</summary>
        internal uint m_EChatRoomEnterResponse;
    }

    /// <summary>A lobby member's state changed (callback 506).</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LobbyChatUpdate_t
    {
        /// <summary>Lobby id.</summary>
        internal ulong m_ulSteamIDLobby;

        /// <summary>User whose lobby status changed.</summary>
        internal ulong m_ulSteamIDUserChanged;

        /// <summary>Chat member who made the change.</summary>
        internal ulong m_ulSteamIDMakingChange;

        /// <summary>Bitfield of EChatMemberStateChange values.</summary>
        internal uint m_rgfChatMemberStateChange;
    }

    /// <summary>A friend requested to join the host's lobby through Steam (callback 315).</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct GameLobbyJoinRequested_t
    {
        /// <summary>The lobby the friend wants to join.</summary>
        internal ulong m_steamIDLobby;

        /// <summary>The friend who requested the join.</summary>
        internal ulong m_steamIDFriend;
    }

    /// <summary>A remote host wants to start a P2P session (callback 1202).</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct P2PSessionRequest_t
    {
        /// <summary>User who wants to talk to us.</summary>
        internal ulong m_steamIDRemote;
    }

    /// <summary>A P2P session failed to connect (callback 1203).</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct P2PSessionConnectFail_t
    {
        /// <summary>User we were sending packets to.</summary>
        internal ulong m_steamIDRemote;

        /// <summary>EP2PSessionError value indicating the cause.</summary>
        internal byte m_eP2PSessionError;
    }
}
