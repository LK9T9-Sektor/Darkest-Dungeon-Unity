namespace Sektor.DarkestDungeon.Lan.Steam.Interop
{
    using System;
    using System.Collections.Generic;
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
        private IntPtr _friends;
        private IntPtr _utils;

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

        /// <summary>Gets the ISteamFriends interface pointer.</summary>
        internal IntPtr Friends
        {
            get { return _friends; }
        }

        /// <summary>Gets the ISteamUtils interface pointer.</summary>
        internal IntPtr Utils
        {
            get { return _utils; }
        }

        /// <summary>
        /// Initializes the Steam client and resolves the interfaces used by the transport.
        /// The interface version list is intentionally not pinned: the running Steam client
        /// decides which versions it exposes, so each interface is probed against candidate
        /// versions (newest first) and the first match is kept.
        /// Returns a failure result with the native error message when Steam is not running,
        /// or with the probed candidates when no compatible interface is available.
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

            List<string> triedClient = new List<string>();
            IntPtr client = ResolveFirst(
                SteamConstants.SteamClientCandidates,
                triedClient,
                SteamNative.SteamInternal_CreateInterface);
            if (client == IntPtr.Zero)
            {
                SteamNative.SteamAPI_Shutdown();
                return Result.Failure("The running Steam client exposes none of the supported ISteamClient versions (tried: " + string.Join(", ", triedClient) + ").");
            }

            List<string> triedUser = new List<string>();
            _user = ResolveFirst(
                SteamConstants.SteamUserCandidates,
                triedUser,
                version => SteamNative.ISteamClient_GetISteamUser(client, _hSteamUser, _hSteamPipe, version));

            List<string> triedMatchmaking = new List<string>();
            _matchmaking = ResolveFirst(
                SteamConstants.SteamMatchmakingCandidates,
                triedMatchmaking,
                version => SteamNative.ISteamClient_GetISteamMatchmaking(client, _hSteamUser, _hSteamPipe, version));

            List<string> triedNetworking = new List<string>();
            _networking = ResolveFirst(
                SteamConstants.SteamNetworkingCandidates,
                triedNetworking,
                version => SteamNative.ISteamClient_GetISteamNetworking(client, _hSteamUser, _hSteamPipe, version));

            List<string> triedFriends = new List<string>();
            _friends = ResolveFirst(
                SteamConstants.SteamFriendsCandidates,
                triedFriends,
                version => SteamNative.ISteamClient_GetISteamFriends(client, _hSteamUser, _hSteamPipe, version));

            List<string> triedUtils = new List<string>();
            _utils = ResolveFirst(
                SteamConstants.SteamUtilsCandidates,
                triedUtils,
                version => SteamNative.ISteamClient_GetISteamUtils(client, _hSteamUser, _hSteamPipe, version));

            if (_user == IntPtr.Zero || _matchmaking == IntPtr.Zero || _networking == IntPtr.Zero
                || _friends == IntPtr.Zero || _utils == IntPtr.Zero)
            {
                SteamNative.SteamAPI_Shutdown();
                return Result.Failure("A required ISteam interface is unavailable (User: " + string.Join(", ", triedUser)
                    + "; Matchmaking: " + string.Join(", ", triedMatchmaking)
                    + "; Networking: " + string.Join(", ", triedNetworking)
                    + "; Friends: " + string.Join(", ", triedFriends)
                    + "; Utils: " + string.Join(", ", triedUtils) + ").");
            }

            SteamNative.SteamAPI_ManualDispatch_Init();
            _isInitialized = true;
            return Result.Success();
        }

        /// <summary>Returns the first candidate version the running client accepts.</summary>
        private static IntPtr ResolveFirst(string[] candidates, List<string> tried, Func<string, IntPtr> tryResolve)
        {
            foreach (string version in candidates)
            {
                tried.Add(version);
                IntPtr resolved = tryResolve(version);
                if (resolved != IntPtr.Zero)
                {
                    return resolved;
                }
            }

            return IntPtr.Zero;
        }

        private static Result InitSteamApi()
        {
            IntPtr errorMessage = Marshal.AllocHGlobal(SteamConstants.SteamApiMaxErrorLength);
            try
            {
                Marshal.Copy(new byte[SteamConstants.SteamApiMaxErrorLength], 0, errorMessage, SteamConstants.SteamApiMaxErrorLength);

                ESteamAPIInitResult result;
                using (NativeUtf8.PinnedBuffer versionBuffer = NativeUtf8.ToNative(string.Empty))
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
