namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    using System;

    /// <summary>
    /// Owns the Steamworks runtime: initialization, interface resolution and the
    /// manual callback dispatch pump. All native state lives here so that
    /// <see cref="SteamTransport"/> only deals with domain concepts.
    /// </summary>
    internal sealed class SteamRuntime : IDisposable
    {
        private bool _isInitialized;
        private int _hSteamPipe;
        private int _hSteamUser;
        private IntPtr _matchmaking;
        private IntPtr _networking;
        private IntPtr _user;

        /// <summary>Gets a value indicating whether the runtime is initialized.</summary>
        internal bool IsInitialized
        {
            get { return _isInitialized; }
        }

        /// <summary>Gets the ISteamMatchmaking interface pointer.</summary>
        internal IntPtr Matchmaking
        {
            get { return _matchmaking; }
        }

        /// <summary>Gets the ISteamNetworking interface pointer.</summary>
        internal IntPtr Networking
        {
            get { return _networking; }
        }

        /// <summary>Gets the ISteamUser interface pointer.</summary>
        internal IntPtr User
        {
            get { return _user; }
        }

        /// <summary>
        /// Initializes the Steam client and resolves the interfaces used by the transport.
        /// Returns false when Steam is not running or a required interface is unavailable.
        /// </summary>
        internal bool Initialize()
        {
            if (_isInitialized)
            {
                return true;
            }

            if (!SteamNative.SteamAPI_Init())
            {
                return false;
            }

            _hSteamPipe = SteamNative.SteamAPI_GetHSteamPipe();
            _hSteamUser = SteamNative.SteamAPI_GetHSteamUser();

            IntPtr client = SteamNative.SteamInternal_CreateInterface(SteamConstants.SteamClientInterfaceVersion);
            if (client == IntPtr.Zero)
            {
                SteamNative.SteamAPI_Shutdown();
                return false;
            }

            _matchmaking = SteamNative.ISteamClient_GetISteamMatchmaking(
                client, _hSteamUser, _hSteamPipe, SteamConstants.SteamMatchmakingInterfaceVersion);
            _networking = SteamNative.ISteamClient_GetISteamNetworking(
                client, _hSteamUser, _hSteamPipe, SteamConstants.SteamNetworkingInterfaceVersion);
            _user = SteamNative.ISteamClient_GetISteamUser(
                client, _hSteamUser, _hSteamPipe, SteamConstants.SteamUserInterfaceVersion);

            if (_matchmaking == IntPtr.Zero || _networking == IntPtr.Zero || _user == IntPtr.Zero)
            {
                SteamNative.SteamAPI_Shutdown();
                return false;
            }

            SteamNative.SteamAPI_ManualDispatch_Init();
            _isInitialized = true;
            return true;
        }

        /// <summary>
        /// Pumps the manual dispatch queue, invoking the given handler for every callback
        /// message. Must be called regularly (every frame in a game loop).
        /// </summary>
        internal void Pump(Action<int, IntPtr> handler)
        {
            if (!_isInitialized)
            {
                return;
            }

            SteamNative.SteamAPI_ManualDispatch_RunFrame(_hSteamPipe);
            CallbackMsg_t message;
            while (SteamNative.SteamAPI_ManualDispatch_GetNextCallback(_hSteamPipe, out message))
            {
                if (handler != null)
                {
                    handler(message.m_iCallback, message.m_pubParam);
                }

                SteamNative.SteamAPI_ManualDispatch_FreeLastCallback(_hSteamPipe);
            }
        }

        public void Dispose()
        {
            if (_isInitialized)
            {
                SteamNative.SteamAPI_Shutdown();
                _isInitialized = false;
            }
        }
    }
}
