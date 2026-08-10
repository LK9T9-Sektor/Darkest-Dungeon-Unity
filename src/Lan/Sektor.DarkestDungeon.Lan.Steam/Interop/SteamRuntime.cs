namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    using System;
    using System.Runtime.InteropServices;

    using Sektor.DarkestDungeon.Lan.Contracts.Results;

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
        /// Returns a failure result with the native error message when Steam is not running,
        /// the client version mismatches, or a required interface is unavailable.
        /// </summary>
        internal Result Initialize()
        {
            if (_isInitialized)
            {
                return Result.Success();
            }

            Result initResult = InitSteamApi();
            if (!initResult.IsSuccess)
            {
                return initResult;
            }

            _hSteamPipe = SteamNative.SteamAPI_GetHSteamPipe();
            _hSteamUser = SteamNative.SteamAPI_GetHSteamUser();

            IntPtr client = SteamNative.SteamInternal_CreateInterface(SteamConstants.SteamClientInterfaceVersion);
            if (client == IntPtr.Zero)
            {
                SteamNative.SteamAPI_Shutdown();
                return Result.Failure("Failed to resolve ISteamClient.");
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
                return Result.Failure("A required ISteam interface is unavailable.");
            }

            SteamNative.SteamAPI_ManualDispatch_Init();
            _isInitialized = true;
            return Result.Success();
        }

        private static Result InitSteamApi()
        {
            string versionList = SteamConstants.SteamClientInterfaceVersion + "\0"
                + SteamConstants.SteamUserInterfaceVersion + "\0"
                + SteamConstants.SteamMatchmakingInterfaceVersion + "\0"
                + SteamConstants.SteamNetworkingInterfaceVersion + "\0";

            IntPtr errorMessage = Marshal.AllocHGlobal(SteamConstants.SteamApiMaxErrorLength);
            try
            {
                Marshal.Copy(new byte[SteamConstants.SteamApiMaxErrorLength], 0, errorMessage, SteamConstants.SteamApiMaxErrorLength);

                ESteamAPIInitResult result;
                using (NativeUtf8.PinnedBuffer versionBuffer = NativeUtf8.ToNative(versionList))
                {
                    result = SteamNative.SteamInternal_SteamAPI_Init(versionBuffer.Pointer, errorMessage);
                }

                if (result != ESteamAPIInitResult.OK)
                {
                    string nativeError = NativeUtf8.FromNative(errorMessage);
                    return Result.Failure("SteamAPI init failed (" + result + "): " + nativeError);
                }

                return Result.Success();
            }
            finally
            {
                Marshal.FreeHGlobal(errorMessage);
            }
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
