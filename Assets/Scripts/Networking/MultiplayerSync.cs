using System.Collections;

using Photon;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Provider-agnostic facade over the legacy multiplayer flow. Legacy scripts call this
/// static facade; depending on the active provider it forwards to the Steam session
/// manager or to the original Photon paths, so the game layer never branches on the
/// transport provider directly.
/// </summary>
public static class MultiplayerSync
{
    private const string ProviderPrefKey = "MultiplayerProvider";
    private const string SteamProviderValue = "steam";
    private const string PhotonProviderValue = "photon";

    private static MultiplayerPartyData _localPartyData;
    private static MultiplayerPartyData _rivalPartyData;
    private static RaidParty _cachedHostRaidParty;

    /// <summary>Gets or sets the active Steam session manager; null when Steam is not in use.</summary>
    public static SteamSessionManager Steam { get; set; }

    /// <summary>Raised when a Steam session was created or joined; carries the session id.</summary>
    public static event System.Action<string> SessionJoined;

    /// <summary>Raised when the Steam session ended (left or lost).</summary>
    public static event System.Action SessionEnded;

    /// <summary>
    /// Gets a value indicating whether Steam is the selected multiplayer provider.
    /// The choice is persisted in player prefs and defaults to Steam.
    /// </summary>
    public static bool IsSteamProvider
    {
        get
        {
            string saved = PlayerPrefs.GetString(ProviderPrefKey);
            return saved.Length == 0 || saved == SteamProviderValue;
        }
    }

    /// <summary>Persists the multiplayer provider selection.</summary>
    public static void SetSteamProvider(bool steam)
    {
        PlayerPrefs.SetString(ProviderPrefKey, steam ? SteamProviderValue : PhotonProviderValue);
        PlayerPrefs.Save();
        Debug.Log("[STEAM] Provider set to " + (steam ? "STEAM" : "PHOTON") + ".");
    }

    /// <summary>Gets a value indicating whether the current session runs over Steam.</summary>
    public static bool IsSteamSession
    {
        get { return Steam != null && Steam.IsSessionActive; }
    }

    /// <summary>Gets the number of players in the session (host included).</summary>
    public static int PlayerCount
    {
        get
        {
            if (IsSteamSession)
                return Steam.PlayerCount;

            return PhotonNetwork.room != null ? PhotonNetwork.room.PlayerCount : 1;
        }
    }

    /// <summary>Gets the identifier of the local player.</summary>
    public static string LocalPlayerId
    {
        get
        {
            if (IsSteamSession)
                return Steam.LocalPlayerId;

            return PhotonNetwork.player != null ? PhotonNetwork.player.ID.ToString() : "local";
        }
    }

    /// <summary>Gets the display name of the local player.</summary>
    public static string LocalName
    {
        get
        {
            if (IsSteamSession)
                return Steam.LocalName;

            return PhotonNetwork.playerName;
        }
    }

    /// <summary>Gets a value indicating whether the local player hosts the session.</summary>
    public static bool IsHost
    {
        get
        {
            if (IsSteamSession)
                return Steam.IsHost;

            return PhotonNetwork.isMasterClient;
        }
    }

    /// <summary>Gets the display name of the session host.</summary>
    public static string HostName
    {
        get
        {
            if (IsSteamSession)
                return Steam.IsHost ? LocalName : RivalName;

            return PhotonNetwork.masterClient != null ? PhotonNetwork.masterClient.NickName : LocalName;
        }
    }

    /// <summary>Gets the display name of the rival player.</summary>
    public static string RivalName
    {
        get
        {
            if (IsSteamSession)
                return Steam.RivalName;

            if (PhotonNetwork.otherPlayers != null && PhotonNetwork.otherPlayers.Length > 0)
                return PhotonNetwork.otherPlayers[0].NickName;

            return "Rival";
        }
    }

    /// <summary>Gets the number of rival players in the session.</summary>
    public static int RivalCount
    {
        get
        {
            if (IsSteamSession)
                return Steam.RivalCount;

            return PhotonNetwork.otherPlayers != null ? PhotonNetwork.otherPlayers.Length : 0;
        }
    }

    /// <summary>Gets the identifiers of all session participants, the local player included.</summary>
    public static string[] PlayerIds
    {
        get
        {
            if (IsSteamSession)
                return Steam.PlayerIds;

            PhotonPlayer[] players = PhotonNetwork.playerList;
            string[] ids = new string[players.Length];
            for (int i = 0; i < players.Length; i++)
                ids[i] = players[i].ID.ToString();
            return ids;
        }
    }

    /// <summary>Gets the party composition of the local player, captured at session start.</summary>
    public static MultiplayerPartyData LocalPartyData
    {
        get { return _localPartyData; }
    }

    /// <summary>Gets the raid party of the session host (hero side).</summary>
    public static RaidParty HostRaidParty
    {
        get
        {
            if (!IsSteamSession)
            {
                if (PhotonNetwork.masterClient != null)
                    return new RaidParty(PhotonNetwork.masterClient);

                return null;
            }

            MultiplayerPartyData data = IsHost ? _localPartyData : _rivalPartyData;
            if (data == null)
                return null;

            if (_cachedHostRaidParty == null)
                _cachedHostRaidParty = new RaidParty(data);

            return _cachedHostRaidParty;
        }
    }

    /// <summary>Gets the raid party of the rival (monster side in the arena fight).</summary>
    public static RaidParty MonsterSideRaidParty
    {
        get
        {
            if (!IsSteamSession)
            {
                PhotonPlayer invader = PhotonNetwork.isMasterClient
                    ? (PhotonNetwork.otherPlayers != null && PhotonNetwork.otherPlayers.Length > 0
                        ? PhotonNetwork.otherPlayers[0] : null)
                    : PhotonNetwork.player;
                if (invader == null)
                    return null;

                return new RaidParty(invader);
            }

            MultiplayerPartyData data = IsHost ? _rivalPartyData : _localPartyData;
            if (data == null)
                return null;

            return new RaidParty(data);
        }
    }

    /// <summary>Replaces the local party composition with the captured value; returns null when no party panel is present.</summary>
    public static MultiplayerPartyData BuildLocalPartyData()
    {
        _localPartyData = MultiplayerPartyData.CaptureFromPanel();
        _cachedHostRaidParty = null;
        return _localPartyData;
    }

    /// <summary>Captures the party shown in the lobby panel if not already captured.</summary>
    public static void EnsureLocalPartyData()
    {
        if (_localPartyData == null)
            BuildLocalPartyData();
    }

    /// <summary>Delivers a remote party composition from the given sender into the local bridge.</summary>
    public static void OnPartyConfigReceived(string senderId, MultiplayerPartyData data)
    {
        if (data == null)
            return;

        _rivalPartyData = data;
    }

    /// <summary>Invoked by the session manager when the Steam session was created or joined.</summary>
    public static void OnSessionJoined(string sessionId)
    {
        _cachedHostRaidParty = null;
        System.Action<string> handler = SessionJoined;
        if (handler != null)
            handler(sessionId);
    }

    /// <summary>Invoked by the session manager when the Steam session ended.</summary>
    public static void OnSessionEnded()
    {
        _localPartyData = null;
        _rivalPartyData = null;
        _cachedHostRaidParty = null;
        Steam = null;

        System.Action handler = SessionEnded;
        if (handler != null)
            handler();
    }

    /// <summary>Sends an RPC to all participants and executes it locally (PhotonTargets.All semantics).</summary>
    public static void SendRpc(string method, params object[] args)
    {
        if (IsSteamSession)
        {
            Steam.SendRpc(method, args);
            return;
        }

        PhotonGameManager gameManager = PhotonGameManager.Instanse;
        if (gameManager != null)
            gameManager.photonView.RPC(method, PhotonTargets.All, args);
    }

    /// <summary>Runs the legacy two-player preparation gate shared by both providers.</summary>
    public static IEnumerator PreparationCheck()
    {
        if (!IsSteamSession)
        {
            PhotonGameManager gameManager = PhotonGameManager.Instanse;
            if (gameManager != null)
                yield return gameManager.StartCoroutine(PhotonGameManager.PreparationCheck());

            yield break;
        }

        while (DarkestSoundManager.NarrationQueue.Count > 0 || DarkestSoundManager.CurrentNarration != null)
            yield return null;

        SendRpc(nameof(PhotonGameManager.PlayerLoaded));
        while (PhotonGameManager.PlayersPreparedCount < PlayerCount)
            yield return null;

        PhotonGameManager.PlayersPreparedCount = 0;
    }

    /// <summary>Loads the given level through the active provider.</summary>
    public static void LoadLevel(string levelName)
    {
        if (IsSteamSession)
        {
            SceneManager.LoadScene(levelName);
            return;
        }

        PhotonNetwork.LoadLevel(levelName);
    }

    /// <summary>Leaves the current session through the active provider.</summary>
    public static void LeaveRoom()
    {
        if (IsSteamSession)
        {
            Steam.LeaveSession();
            return;
        }

        PhotonGameManager gameManager = PhotonGameManager.Instanse;
        if (gameManager != null)
            gameManager.LeaveRoom();
    }

    /// <summary>Computes a stable integer hash for the given text, provider independent.</summary>
    public static int StableHash(string text)
    {
        int hash = 17;
        for (int i = 0; i < text.Length; i++)
            hash = hash * 31 + text[i];

        return hash;
    }
}
