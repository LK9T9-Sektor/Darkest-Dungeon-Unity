using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Lan.Contracts.Transport;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent Steam session facade for the Unity layer. Owns the transport, pumps its
/// callbacks every frame, forwards inbound messages to the raid bridge and translates
/// session lifecycle events into MultiplayerSync notifications.
/// </summary>
public class SteamSessionManager : MonoBehaviour
{
    private const string SessionName = "DarkestDungeon Coop";
    private const int MaxSessionPlayers = 2;
    private const string DungeonMultiplayerSceneName = "DungeonMultiplayer";
    private const string CampaignSelectionSceneName = "CampaignSelection";

    private ITransport _transport;
    private SteamRaidBridge _bridge;
    private string _sessionId;
    private string _localName;
    private string _lastError;
    private bool _initialized;

    /// <summary>
    /// Gets a value indicating whether the transport was bound and the Steam runtime
    /// initialized successfully; false when Steam is unavailable.
    /// </summary>
    public bool IsInitialized
    {
        get { return _transport != null && _initialized; }
    }

    /// <summary>Gets a value indicating whether the session is currently active.</summary>
    public bool IsSessionActive
    {
        get { return _transport != null && _transport.IsSessionActive; }
    }

    /// <summary>Gets the id of the current session; empty when not in a session.</summary>
    public string SessionId
    {
        get { return _sessionId; }
    }

    /// <summary>Gets the identifier of the local player.</summary>
    public string LocalPlayerId
    {
        get { return _transport != null ? _transport.LocalPlayerId : string.Empty; }
    }

    /// <summary>Gets the identifier of the session host.</summary>
    public string HostPlayerId
    {
        get { return _transport != null ? _transport.HostPlayerId : string.Empty; }
    }

    /// <summary>Gets a value indicating whether the local player hosts the session.</summary>
    public bool IsHost
    {
        get
        {
            string hostId = HostPlayerId;
            return hostId.Length > 0 && LocalPlayerId == hostId;
        }
    }

    /// <summary>Gets the number of session participants, the local player included.</summary>
    public int PlayerCount
    {
        get
        {
            if (!IsSessionActive)
                return 0;

            return _transport.GetSessionPlayers().Length + 1;
        }
    }

    /// <summary>Gets the number of rival players in the session.</summary>
    public int RivalCount
    {
        get
        {
            if (_transport == null)
                return 0;

            return _transport.GetSessionPlayers().Length;
        }
    }

    /// <summary>Gets the display name of the local player.</summary>
    public string LocalName
    {
        get { return _localName; }
    }

    /// <summary>Gets the display name of the rival player.</summary>
    public string RivalName
    {
        get
        {
            string[] rivals = _transport != null ? _transport.GetSessionPlayers() : new string[0];
            if (rivals.Length == 0)
                return "Rival";

            string id = rivals[0];
            string suffix = id.Length >= 3 ? id.Substring(id.Length - 3) : id;
            return "Player " + suffix;
        }
    }

    /// <summary>Gets the identifier of the first rival player.</summary>
    public string RivalPlayerId
    {
        get
        {
            string[] rivals = _transport != null ? _transport.GetSessionPlayers() : new string[0];
            return rivals.Length > 0 ? rivals[0] : string.Empty;
        }
    }

    /// <summary>Gets the identifiers of all session participants, the local player included.</summary>
    public string[] PlayerIds
    {
        get
        {
            string[] rivals = _transport != null ? _transport.GetSessionPlayers() : new string[0];
            string[] ids = new string[rivals.Length + 1];
            ids[0] = LocalPlayerId;
            for (int i = 0; i < rivals.Length; i++)
                ids[i + 1] = rivals[i];

            return ids;
        }
    }

    /// <summary>Gets the last transport error description; empty when none occurred.</summary>
    public string LastError
    {
        get { return _lastError; }
    }

    /// <summary>
    /// Binds the transport and starts pumping its callbacks; the local name is used for
    /// the player display. When a previous initialization failed, the old transport is
    /// released and the new one bound instead, so a later retry (e.g. after the Steam
    /// client was started) works without reloading.
    /// </summary>
    public void Initialize(ITransport transport, string localName)
    {
        if (_initialized)
            return;

        if (_transport != null)
        {
            UnbindTransport();
            _transport.Dispose();
        }

        _transport = transport;
        _localName = localName;
        _bridge = new SteamRaidBridge(transport);
        _lastError = string.Empty;
        _initialized = false;

        BindTransport(transport);

        Result initResult = _transport.Initialize();
        if (initResult.IsSuccess)
        {
            _initialized = true;
        }
        else
        {
            _lastError = initResult.ErrorMessage;
            MultiplayerSync.WriteError("STEAM", "Steam unavailable: " + initResult.ErrorMessage);
        }
    }

    /// <summary>Creates a new session with the local player as the host.</summary>
    public Result HostSession()
    {
        if (_transport == null)
            return Result.Failure("Steam transport is not initialized.");

        return _transport.CreateSession(SessionName, MaxSessionPlayers);
    }

    /// <summary>Joins an existing session by its identifier.</summary>
    public Result JoinSession(string sessionId)
    {
        if (_transport == null)
            return Result.Failure("Steam transport is not initialized.");

        return _transport.JoinSession(sessionId);
    }

    /// <summary>
    /// Leaves the current session and notifies the game layer. When leaving while inside
    /// the raid scene, the campaign selection is loaded so the host does not stay stuck
    /// on the faded-out dungeon.
    /// </summary>
    public void LeaveSession()
    {
        if (!IsSessionActive)
            return;

        _transport.LeaveSession();
        _sessionId = string.Empty;
        MultiplayerSync.OnSessionEnded();
        ReturnToLobbyWhenInRaid();
    }

    /// <summary>
    /// Releases the transport when the application is quitting, so the Steam client no
    /// longer reports the game as running. Reliable in both the editor (play mode stop)
    /// and built players (process exit), unlike scene unloads which may be skipped.
    /// </summary>
    private void OnApplicationQuit()
    {
        Shutdown();
    }

    /// <summary>Sends an RPC to the rival and executes it locally.</summary>
    public void SendRpc(string method, object[] args)
    {
        if (_bridge != null)
            _bridge.SendRpc(method, args);
    }

    /// <summary>Sends the local party composition to the rival.</summary>
    public void SendPartyConfig(MultiplayerPartyData data)
    {
        if (_transport == null || data == null)
            return;

        _transport.SendMessage(SteamRaidBridge.PartyConfigType, data.Serialize());
    }

    /// <summary>Pumps transport callbacks every frame so messages are delivered on the main thread.</summary>
    private void Update()
    {
        if (_initialized && _transport != null)
            _transport.RunCallbacks();
    }

    private void OnSessionJoined(string sessionId)
    {
        _sessionId = sessionId;
        MultiplayerSync.WriteLog("STEAM", "Session joined: " + sessionId);
        MultiplayerSync.WriteLog("STEAM", "ROOM_ID=" + sessionId);
        MultiplayerSync.OnSessionJoined(sessionId);
    }

    private void OnPlayerJoined(string playerId)
    {
        MultiplayerSync.WriteLog("STEAM", "Player joined: " + playerId);
        MultiplayerSync.EnsureLocalPartyData();
        SendPartyConfig(MultiplayerSync.LocalPartyData);
    }

    private void OnPlayerLeft(string playerId)
    {
        MultiplayerSync.WriteLog("STEAM", "Player left: " + playerId);
        LeaveSession();
    }

    private void OnMessageReceived(TransportMessage message)
    {
        if (_bridge != null)
            _bridge.Dispatch(message);
    }

    private void OnSessionInviteReceived(string sessionId)
    {
        MultiplayerSync.WriteLog("STEAM", "Invitation received for session " + sessionId);
    }

    private void OnDisconnected()
    {
        MultiplayerSync.WriteLog("STEAM", "Session disconnected.");
        LeaveSession();
    }

    /// <summary>Returns to the campaign selection when the session ends while inside the raid scene.</summary>
    private void ReturnToLobbyWhenInRaid()
    {
        if (SceneManager.GetActiveScene().name == DungeonMultiplayerSceneName)
            SceneManager.LoadScene(CampaignSelectionSceneName);
    }

    /// <summary>Releases the transport bindings when the object is destroyed.</summary>
    private void OnDestroy()
    {
        Shutdown();
    }

    /// <summary>
    /// Disposes the transport and clears the runtime state. Idempotent: safe to call
    /// from both OnApplicationQuit and OnDestroy without double-releasing native Steam.
    /// </summary>
    private void Shutdown()
    {
        if (_transport == null)
            return;

        UnbindTransport();
        _transport.Dispose();
        _transport = null;
        _initialized = false;
    }

    private void BindTransport(ITransport transport)
    {
        transport.SessionJoined += OnSessionJoined;
        transport.PlayerJoined += OnPlayerJoined;
        transport.PlayerLeft += OnPlayerLeft;
        transport.MessageReceived += OnMessageReceived;
        transport.SessionInviteReceived += OnSessionInviteReceived;
        transport.Disconnected += OnDisconnected;
    }

    private void UnbindTransport()
    {
        _transport.SessionJoined -= OnSessionJoined;
        _transport.PlayerJoined -= OnPlayerJoined;
        _transport.PlayerLeft -= OnPlayerLeft;
        _transport.MessageReceived -= OnMessageReceived;
        _transport.SessionInviteReceived -= OnSessionInviteReceived;
        _transport.Disconnected -= OnDisconnected;
    }
}
