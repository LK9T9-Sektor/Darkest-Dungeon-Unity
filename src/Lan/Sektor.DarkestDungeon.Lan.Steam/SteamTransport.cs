namespace Sektor.DarkestDungeon.Lan.Steam
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    using Sektor.DarkestDungeon.Lan.Contracts.Results;
    using Sektor.DarkestDungeon.Lan.Contracts.Transport;
    using Sektor.DarkestDungeon.Lan.Steam.Interop;

    /// <summary>
    /// <see cref="ITransport"/> implementation over Steam P2P. Steam lobbies represent
    /// sessions; reliable, ordered messages are exchanged through the SteamNetworking
    /// P2P packet API. The AppID comes from the Steam client or steam_appid.txt next to
    /// the executable; the transport never hardcodes it.
    /// </summary>
    public sealed class SteamTransport : ITransport
    {
        private const int GameChannel = 1;
        private const int ReceiveBufferSize = 65535;
        private const int MaxDrainedPerPump = 256;
        private const string LobbyDataHostSteamId = "host_steam_id";
        private const string ConnectRichPresenceKey = "connect";
        private const string JoinLobbyUrlPrefix = "steam://joinlobby/";

        private readonly ITransportCodec _codec;
        private readonly SteamRuntime _runtime;
        private readonly Dictionary<int, Action<IntPtr>> _callbacks;

        private ulong _currentLobbyId;
        private bool _sessionCreationPending;

        /// <summary>Creates the transport with the given wire codec.</summary>
        public SteamTransport(ITransportCodec codec)
        {
            _codec = codec;
            _runtime = new SteamRuntime();
            _callbacks = new Dictionary<int, Action<IntPtr>>
            {
                { SteamCallbackIds.LobbyCreated, HandleLobbyCreated },
                { SteamCallbackIds.LobbyEnter, HandleLobbyEnter },
                { SteamCallbackIds.LobbyChatUpdate, HandleLobbyChatUpdate },
                { SteamCallbackIds.GameLobbyJoinRequested, HandleGameLobbyJoinRequested },
                { SteamCallbackIds.P2PSessionRequest, HandleP2PSessionRequest },
                { SteamCallbackIds.P2PSessionConnectFail, HandleP2PSessionConnectFail }
            };
        }

        /// <inheritdoc />
        public event Action<string> SessionJoined;

        /// <inheritdoc />
        public event Action<string> PlayerJoined;

        /// <inheritdoc />
        public event Action<string> PlayerLeft;

        /// <inheritdoc />
        public event Action<TransportMessage> MessageReceived;

        /// <inheritdoc />
        public event Action<string> SessionInviteReceived;

        /// <inheritdoc />
        public event Action Disconnected;

        /// <inheritdoc />
        public string LocalPlayerId
        {
            get
            {
                if (!_runtime.IsInitialized)
                {
                    return string.Empty;
                }

                return SteamNative.ISteamUser_GetSteamID(_runtime.User).ToString();
            }
        }

        /// <inheritdoc />
        public bool IsSessionActive
        {
            get { return _currentLobbyId != 0; }
        }

        /// <inheritdoc />
        public Result Initialize()
        {
            return _runtime.Initialize();
        }

        /// <inheritdoc />
        public void RunCallbacks()
        {
            _runtime.Pump(DispatchCallback);
            DrainIncomingMessages();
        }

        /// <inheritdoc />
        public Result CreateSession(string sessionName, int maxPlayers)
        {
            if (IsSessionActive)
            {
                return Result.Failure("Already in a session.");
            }

            if (_sessionCreationPending)
            {
                return Result.Failure("Session creation already in progress.");
            }

            _sessionCreationPending = true;
            SteamNative.ISteamMatchmaking_CreateLobby(_runtime.Matchmaking, (int)ELobbyType.Public, maxPlayers);
            return Result.Success();
        }

        /// <inheritdoc />
        public Result JoinSession(string sessionId)
        {
            if (IsSessionActive)
            {
                return Result.Failure("Already in a session.");
            }

            ulong lobbyId;
            if (!ulong.TryParse(sessionId, out lobbyId))
            {
                return Result.Failure("Invalid session id.");
            }

            SteamNative.ISteamMatchmaking_JoinLobby(_runtime.Matchmaking, lobbyId);
            return Result.Success();
        }

        /// <inheritdoc />
        public Result LeaveSession()
        {
            if (!IsSessionActive)
            {
                return Result.Failure("Not in a session.");
            }

            SteamNative.ISteamMatchmaking_LeaveLobby(_runtime.Matchmaking, _currentLobbyId);
            _currentLobbyId = 0;
            _sessionCreationPending = false;
            ClearJoinableState();
            return Result.Success();
        }

        /// <inheritdoc />
        public Result SendMessage(string type, string payload)
        {
            if (!IsSessionActive)
            {
                return Result.Failure("Not in a session.");
            }

            string text = _codec.Serialize(new TransportMessage(LocalPlayerId, type, payload));
            byte[] data = Encoding.UTF8.GetBytes(text);
            if (SendToAllPlayers(data))
            {
                return Result.Success();
            }

            return Result.Failure("Failed to send message.");
        }

        /// <inheritdoc />
        public string[] GetSessionPlayers()
        {
            List<string> players = new List<string>();
            if (!IsSessionActive)
            {
                return players.ToArray();
            }

            ulong myId = SteamNative.ISteamUser_GetSteamID(_runtime.User);
            int count = SteamNative.ISteamMatchmaking_GetNumLobbyMembers(_runtime.Matchmaking, _currentLobbyId);
            for (int i = 0; i < count; i++)
            {
                ulong memberId = SteamNative.ISteamMatchmaking_GetLobbyMemberByIndex(_runtime.Matchmaking, _currentLobbyId, i);
                if (memberId != myId)
                {
                    players.Add(memberId.ToString());
                }
            }

            return players.ToArray();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (IsSessionActive)
            {
                SteamNative.ISteamMatchmaking_LeaveLobby(_runtime.Matchmaking, _currentLobbyId);
                _currentLobbyId = 0;
            }

            ClearJoinableState();
            _runtime.Dispose();
        }

        private void DispatchCallback(int callbackId, IntPtr param)
        {
            Action<IntPtr> handler;
            if (_callbacks.TryGetValue(callbackId, out handler))
            {
                handler(param);
            }
        }

        private void HandleLobbyCreated(IntPtr param)
        {
            LobbyCreated_t callback = (LobbyCreated_t)Marshal.PtrToStructure(param, typeof(LobbyCreated_t));
            _sessionCreationPending = false;
            if (callback.m_eResult != EResult.OK)
            {
                return;
            }

            bool wasInSession = IsSessionActive;
            _currentLobbyId = callback.m_ulSteamIDLobby;
            SetLobbyData(LobbyDataHostSteamId, LocalPlayerId);
            UpdateJoinableState();
            if (!wasInSession)
            {
                OnSessionJoined(_currentLobbyId.ToString());
            }
        }

        private void HandleLobbyEnter(IntPtr param)
        {
            LobbyEnter_t callback = (LobbyEnter_t)Marshal.PtrToStructure(param, typeof(LobbyEnter_t));
            if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.Success)
            {
                return;
            }

            bool wasInSession = IsSessionActive;
            _currentLobbyId = callback.m_ulSteamIDLobby;
            _sessionCreationPending = false;
            UpdateJoinableState();
            if (wasInSession)
            {
                return;
            }

            NotifyExistingPlayers();
            OnSessionJoined(_currentLobbyId.ToString());
        }

        private void HandleLobbyChatUpdate(IntPtr param)
        {
            LobbyChatUpdate_t callback = (LobbyChatUpdate_t)Marshal.PtrToStructure(param, typeof(LobbyChatUpdate_t));
            if (callback.m_ulSteamIDLobby != _currentLobbyId)
            {
                return;
            }

            string changedId = callback.m_ulSteamIDUserChanged.ToString();
            uint state = callback.m_rgfChatMemberStateChange;

            if ((state & (uint)EChatMemberStateChange.Entered) != 0)
            {
                Action<string> joined = PlayerJoined;
                if (joined != null)
                {
                    joined(changedId);
                }
            }

            uint leftState = (uint)(EChatMemberStateChange.Left | EChatMemberStateChange.Disconnected);
            if ((state & leftState) != 0)
            {
                Action<string> left = PlayerLeft;
                if (left != null)
                {
                    left(changedId);
                }
            }
        }

        private void HandleP2PSessionRequest(IntPtr param)
        {
            P2PSessionRequest_t callback = (P2PSessionRequest_t)Marshal.PtrToStructure(param, typeof(P2PSessionRequest_t));
            SteamNative.ISteamNetworking_AcceptP2PSessionWithUser(_runtime.Networking, callback.m_steamIDRemote);
        }

        private void HandleGameLobbyJoinRequested(IntPtr param)
        {
            GameLobbyJoinRequested_t callback = (GameLobbyJoinRequested_t)Marshal.PtrToStructure(param, typeof(GameLobbyJoinRequested_t));
            Action<string> invite = SessionInviteReceived;
            if (invite != null)
            {
                invite(callback.m_steamIDLobby.ToString());
            }
        }

        private void HandleP2PSessionConnectFail(IntPtr param)
        {
            Action disconnected = Disconnected;
            if (disconnected != null)
            {
                disconnected();
            }
        }

        private void SetLobbyData(string key, string value)
        {
            using (NativeUtf8.PinnedBuffer keyBuffer = NativeUtf8.ToNative(key))
            using (NativeUtf8.PinnedBuffer valueBuffer = NativeUtf8.ToNative(value))
            {
                SteamNative.ISteamMatchmaking_SetLobbyData(
                    _runtime.Matchmaking, _currentLobbyId, keyBuffer.Pointer, valueBuffer.Pointer);
            }
        }

        private void UpdateJoinableState()
        {
            uint appId = SteamNative.ISteamUtils_GetAppID(_runtime.Utils);
            string connect = JoinLobbyUrlPrefix + appId + "/" + _currentLobbyId;
            using (NativeUtf8.PinnedBuffer keyBuffer = NativeUtf8.ToNative(ConnectRichPresenceKey))
            using (NativeUtf8.PinnedBuffer valueBuffer = NativeUtf8.ToNative(connect))
            {
                SteamNative.ISteamFriends_SetRichPresence(_runtime.Friends, keyBuffer.Pointer, valueBuffer.Pointer);
            }
        }

        private void ClearJoinableState()
        {
            SteamNative.ISteamFriends_ClearRichPresence(_runtime.Friends);
        }

        private void NotifyExistingPlayers()
        {
            string[] existing = GetSessionPlayers();
            Action<string> joined = PlayerJoined;
            if (joined == null)
            {
                return;
            }

            for (int i = 0; i < existing.Length; i++)
            {
                joined(existing[i]);
            }
        }

        private void OnSessionJoined(string sessionId)
        {
            Action<string> joined = SessionJoined;
            if (joined != null)
            {
                joined(sessionId);
            }
        }

        private bool SendToAllPlayers(byte[] data)
        {
            List<string> players = new List<string>(GetSessionPlayers());
            bool success = true;
            for (int i = 0; i < players.Count; i++)
            {
                ulong target;
                if (!ulong.TryParse(players[i], out target))
                {
                    success = false;
                    continue;
                }

                if (!SteamNative.ISteamNetworking_SendP2PPacket(
                    _runtime.Networking, target, data, (uint)data.Length, (int)EP2PSend.Reliable, GameChannel))
                {
                    success = false;
                }
            }

            return success;
        }

        private void DrainIncomingMessages()
        {
            uint size;
            int drained = 0;
            while (drained < MaxDrainedPerPump && SteamNative.ISteamNetworking_IsP2PPacketAvailable(_runtime.Networking, out size, GameChannel))
            {
                byte[] buffer = new byte[Math.Min(size, ReceiveBufferSize)];
                uint bytesRead;
                ulong remote;
                if (SteamNative.ISteamNetworking_ReadP2PPacket(_runtime.Networking, buffer, size, out bytesRead, out remote, GameChannel))
                {
                    string text = Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);
                    TransportMessage decoded = _codec.Deserialize(text);
                    TransportMessage received = new TransportMessage(
                        remote.ToString(), decoded.Type, decoded.Payload);

                    Action<TransportMessage> handler = MessageReceived;
                    if (handler != null)
                    {
                        handler(received);
                    }
                }

                drained++;
            }
        }
    }
}
