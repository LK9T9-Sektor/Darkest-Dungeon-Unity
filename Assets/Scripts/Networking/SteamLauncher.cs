using Sektor.DarkestDungeon.Lan.Contracts.Results;
using Sektor.DarkestDungeon.Lan.Contracts.Transport;
using Sektor.DarkestDungeon.Lan.Steam;

using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runtime-created Steam co-op lobby panel shown on the campaign selection screen.
/// Lets the local player host or join a Steam session, captures the party built in
/// the legacy multiplayer party panel, and drives the transition into the raid scene.
/// Created anew per lobby visit; the Steam session manager persists across scenes.
/// The panel opens on demand (OpenPanel from the legacy multiplayer entry point) and
/// carries the provider toggle between Steam and the legacy Photon room list; while
/// the Photon room list is open a top-of-screen button switches back to this lobby,
/// so the provider choice is always reachable in both directions.
/// </summary>
public class SteamLauncher : MonoBehaviour
{
    private const string _campaignSelectionSceneName = "CampaignSelection";
    private const string _dungeonMultiplayerSceneName = "DungeonMultiplayer";
    private const string _playerNicknamePrefKey = "PlayerNickname";
    private const string _fontResourcePath = "Fonts/Deutsch";

    private const float _joinTimeoutSeconds = 15f;
    private const float _loadDelaySeconds = 1.5f;
    private const string _soundSettingsSpritesPath = "UI/SoundSettingsSprites";

    private static readonly Color _labelColor = new Color(0.9338235f, 0.7924933f, 0.4463127f);
    private static readonly Color _activeToggleColor = new Color(0.45f, 0.38f, 0.2f, 0.95f);
    private static readonly Color _inactiveToggleColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);

    private static SteamLauncher _instanse;

    private static SteamSessionManager _sessionManager;

    private Font _font;
    private GameObject _panel;
    private Text _statusLabel;
    private Text _sessionIdLabel;
    private Text _playerLabel;
    private InputField _roomIdField;
    private Button _hostButton;
    private Button _joinButton;
    private Button _leaveButton;
    private Button _closeButton;
    private Button _photonToggleButton;
    private Button _steamToggleButton;
    private Button _photonSwitchButton;
    private Image _photonToggleBackground;
    private Image _steamToggleBackground;
    private float _joinTimeLeft;
    private bool _waitingForSession;

    /// <summary>Creates the launcher object once the campaign selection scene has loaded.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instanse != null)
            return;

        if (SceneManager.GetActiveScene().name != _campaignSelectionSceneName)
            return;

        GameObject launcherObject = new GameObject(nameof(SteamLauncher));
        launcherObject.AddComponent<SteamLauncher>();
    }

    /// <summary>
    /// Opens the Steam lobby panel. Creates the launcher on demand when absent, so the
    /// panel works regardless of the scene the game started in; the startup initializer
    /// only runs once per app run.
    /// </summary>
    public static void OpenPanel()
    {
        EnsureLauncher();

        if (_instanse == null)
        {
            Debug.LogError("[STEAM] Lobby panel unavailable.");
            return;
        }

        if (!MultiplayerSync.IsSteamProvider)
            MultiplayerSync.SetSteamProvider(true);

        MultiplayerMenuState.Open(MultiplayerMenuState.Menu.Steam);
        _instanse._panel.SetActive(true);
        _instanse.ApplyToggleHighlight();
        _instanse.EnsureSessionManager();
        _instanse.RefreshStatus();
        Debug.Log("[STEAM] Lobby panel opened.");
    }

    /// <summary>Creates the launcher object when absent, so the Photon room list can offer the Steam switch button.</summary>
    public static void EnsureLauncher()
    {
        if (_instanse != null)
            return;

        GameObject launcherObject = new GameObject(nameof(SteamLauncher));
        launcherObject.AddComponent<SteamLauncher>();
    }

    private void Awake()
    {
        if (_instanse != null)
        {
            Destroy(gameObject);
            return;
        }

        _instanse = this;
        MultiplayerSync.SessionJoined += OnSessionJoined;
        MultiplayerSync.SessionEnded += OnSessionEnded;

        CreateUi();
    }

    private void OnDestroy()
    {
        MultiplayerSync.SessionJoined -= OnSessionJoined;
        MultiplayerSync.SessionEnded -= OnSessionEnded;

        if (_instanse == this)
            _instanse = null;
    }

    private void Update()
    {
        SyncPhotonSwitchButton();

        if (_waitingForSession)
        {
            _joinTimeLeft -= Time.deltaTime;
            if (_joinTimeLeft <= 0f)
            {
                _waitingForSession = false;
                EnableControls();
                SetStatus("Session request timed out. Check the ROOM_ID and try again.");
                Debug.LogWarning("[STEAM] Session request timed out.");
            }

            return;
        }

        if (_panel != null && _panel.activeSelf && Input.GetKeyUp(KeyCode.Escape))
            ClosePanel();
    }

    private void CreateUi()
    {
        _font = Resources.Load<Font>(_fontResourcePath);

        EnsureEventSystem();

        Canvas canvas = CreateCanvas();
        CreatePanel(canvas.transform);
        _panel.SetActive(false);
    }

    private void EnsureSessionManager()
    {
        SteamSessionManager manager = FindObjectOfType<SteamSessionManager>();
        if (manager == null)
        {
            GameObject managerObject = new GameObject(nameof(SteamSessionManager));
            DontDestroyOnLoad(managerObject);
            manager = managerObject.AddComponent<SteamSessionManager>();
        }

        _sessionManager = manager;

        if (!manager.IsInitialized)
            manager.Initialize(new SteamTransport(new JsonTransportCodec()), ResolveLocalName(manager));

        MultiplayerSync.Steam = manager;

        if (manager.IsInitialized)
        {
            _playerLabel.text = "Steam ID: " + manager.LocalPlayerId;
        }
        else
        {
            _playerLabel.text = "Steam unavailable: " + manager.LastError;
            _hostButton.interactable = false;
            _joinButton.interactable = false;
        }

        RefreshStatus();
    }

    private string ResolveLocalName(SteamSessionManager manager)
    {
        string saved = PlayerPrefs.GetString(_playerNicknamePrefKey);
        if (saved.Length > 0)
            return saved;

        string id = manager.LocalPlayerId;
        return "Player" + (id.Length >= 4 ? id.Substring(id.Length - 4) : id);
    }

    private void RefreshStatus()
    {
        SteamSessionManager manager = _sessionManager;
        if (manager == null)
            return;

        if (manager.IsSessionActive)
        {
            SetStatus("Session active.");
            _sessionIdLabel.text = "ROOM_ID: " + manager.SessionId;
            _playerLabel.text = "Steam ID: " + manager.LocalPlayerId;
        }
    }

    private void OnSessionJoined(string sessionId)
    {
        _waitingForSession = false;
        EnableControls();
        _sessionIdLabel.text = "ROOM_ID: " + sessionId;
        SetStatus("Session joined! Waiting for the opponent...");
        Debug.Log("[STEAM] Session joined: " + sessionId);

        MultiplayerSync.EnsureLocalPartyData();
        _sessionManager.SendPartyConfig(MultiplayerSync.LocalPartyData);
        StartCoroutine(LoadRaidSceneRoutine());
    }

    private void OnSessionEnded()
    {
        _waitingForSession = false;
        EnableControls();
        _roomIdField.text = string.Empty;
        _sessionIdLabel.text = "ROOM_ID: -";
        RefreshStatus();
        SetStatus("Session closed.");
        Debug.Log("[STEAM] Session closed.");
    }

    private IEnumerator LoadRaidSceneRoutine()
    {
        SetStatus("Loading the raid scene...");
        yield return new WaitForSeconds(_loadDelaySeconds);

        if (_sessionManager.IsSessionActive)
        {
            Debug.Log("[STEAM] Loading raid scene: " + _dungeonMultiplayerSceneName);
            MultiplayerSync.LoadLevel(_dungeonMultiplayerSceneName);
        }
    }

    private void HostButtonClicked()
    {
        MultiplayerPartyData party = MultiplayerSync.BuildLocalPartyData();
        if (party == null)
        {
            SetStatus("Build your party in the party panel first!");
            return;
        }

        _waitingForSession = true;
        _joinTimeLeft = _joinTimeoutSeconds;
        DisableControls();
        SetStatus("Creating Steam session...");
        Debug.Log("[STEAM] Host session requested.");

        Result result = _sessionManager.HostSession();
        if (!result.IsSuccess)
        {
            _waitingForSession = false;
            EnableControls();
            SetStatus("Host failed: " + result.ErrorMessage);
            Debug.LogError("[STEAM] Host failed: " + result.ErrorMessage);
        }
    }

    private void JoinButtonClicked()
    {
        string roomId = _roomIdField.text.Trim();
        if (roomId.Length == 0)
        {
            SetStatus("Enter the ROOM_ID of the host first.");
            return;
        }

        MultiplayerPartyData party = MultiplayerSync.BuildLocalPartyData();
        if (party == null)
        {
            SetStatus("Build your party in the party panel first!");
            return;
        }

        _waitingForSession = true;
        _joinTimeLeft = _joinTimeoutSeconds;
        DisableControls();
        SetStatus("Joining Steam session " + roomId + "...");
        Debug.Log("[STEAM] Join session requested: " + roomId);

        Result result = _sessionManager.JoinSession(roomId);
        if (!result.IsSuccess)
        {
            _waitingForSession = false;
            EnableControls();
            SetStatus("Join failed: " + result.ErrorMessage);
            Debug.LogError("[STEAM] Join failed: " + result.ErrorMessage);
        }
    }

    private void LeaveButtonClicked()
    {
        if (_sessionManager != null && _sessionManager.IsSessionActive)
        {
            Debug.Log("[STEAM] Leaving session.");
            _sessionManager.LeaveSession();
            return;
        }

        _waitingForSession = false;
        EnableControls();
        SetStatus("No active session.");
    }

    private void PhotonToggleClicked()
    {
        MultiplayerSync.SetSteamProvider(false);
        Debug.Log("[STEAM] Provider switched to PHOTON.");

        _panel.SetActive(false);
        RoomSelector roomSelector = FindObjectOfType<RoomSelector>();
        if (roomSelector != null)
            roomSelector.SaveSelectionStart();
    }

    private void SteamToggleClicked()
    {
        MultiplayerSync.SetSteamProvider(true);
        Debug.Log("[STEAM] Provider switched to STEAM.");

        _panel.SetActive(true);
        ApplyToggleHighlight();
        RefreshStatus();
    }

    private void ClosePanel()
    {
        if (_panel != null)
            _panel.SetActive(false);

        MultiplayerMenuState.Close();
        Debug.Log("[STEAM] Lobby panel closed.");
    }

    /// <summary>
    /// Shows a "Open STEAM Lobby" button while the Photon room list is open, so the
    /// player can always switch back to the Steam window. The button appears once the
    /// room list is fully opened and disappears when the room list is closed.
    /// </summary>
    private void SyncPhotonSwitchButton()
    {
        bool photonOpen = MultiplayerMenuState.Current == MultiplayerMenuState.Menu.Photon;

        if (!photonOpen)
        {
            if (_photonSwitchButton != null)
            {
                Destroy(_photonSwitchButton.gameObject);
                _photonSwitchButton = null;
            }
            return;
        }

        if (_photonSwitchButton != null)
            return;

        RoomSelector roomSelector = FindObjectOfType<RoomSelector>();
        if (roomSelector != null && roomSelector.ReturnButton != null && roomSelector.ReturnButton.interactable)
            CreatePhotonSwitchButton();
    }

    /// <summary>Creates the top-of-screen button that switches from the Photon room list back to the Steam lobby.</summary>
    private void CreatePhotonSwitchButton()
    {
        GameObject buttonObject = CreateUiObject("SteamSwitchButton", _panel.transform.parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -60);
        rect.sizeDelta = new Vector2(260, 48);

        Image background = buttonObject.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        _photonSwitchButton = buttonObject.AddComponent<Button>();
        _photonSwitchButton.targetGraphic = background;
        _photonSwitchButton.onClick.AddListener(SwitchToSteam);

        Text buttonLabel = CreateText("SteamSwitchLabel", buttonObject.transform, "Open STEAM Lobby",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(250, 42));
        buttonLabel.raycastTarget = false;
    }

    /// <summary>Switches the provider back to Steam: closes the Photon room list and opens the lobby panel.</summary>
    private void SwitchToSteam()
    {
        MultiplayerSync.SetSteamProvider(true);
        Debug.Log("[STEAM] Provider switched to STEAM from the room list.");

        RoomSelector roomSelector = FindObjectOfType<RoomSelector>();
        if (roomSelector != null)
            roomSelector.ReturnButtonClicked();

        OpenPanel();
    }

    private void SetStatus(string text)
    {
        if (_statusLabel != null)
            _statusLabel.text = text;
    }

    private void DisableControls()
    {
        _hostButton.interactable = false;
        _joinButton.interactable = false;
        _leaveButton.interactable = false;
        _closeButton.interactable = false;
        _roomIdField.interactable = false;
        _photonToggleButton.interactable = false;
        _steamToggleButton.interactable = false;
    }

    private void EnableControls()
    {
        bool initialized = _sessionManager != null && _sessionManager.IsInitialized;
        _hostButton.interactable = initialized;
        _joinButton.interactable = initialized;
        _leaveButton.interactable = _sessionManager != null && _sessionManager.IsSessionActive;
        _closeButton.interactable = true;
        _roomIdField.interactable = initialized;
        _photonToggleButton.interactable = true;
        _steamToggleButton.interactable = true;
    }

    private void ApplyToggleHighlight()
    {
        bool steam = MultiplayerSync.IsSteamProvider;
        _steamToggleBackground.color = steam ? _activeToggleColor : _inactiveToggleColor;
        _photonToggleBackground.color = steam ? _inactiveToggleColor : _activeToggleColor;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject(nameof(EventSystem));
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("SteamLobbyCanvas");
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void CreatePanel(Transform parent)
    {
        _panel = CreateUiObject("SteamLobbyPanel", parent);
        RectTransform rect = _panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(1024, 740);

        Image background = _panel.AddComponent<Image>();
        SoundSettingsSprites sprites = Resources.Load<SoundSettingsSprites>(_soundSettingsSpritesPath);
        if (sprites != null && sprites.WindowFrame != null)
        {
            background.sprite = sprites.WindowFrame;
            background.type = Image.Type.Sliced;
        }
        else
        {
            background.color = new Color(0, 0, 0, 0.85f);
        }

        CreateTitle(_panel.transform);
        CreateProviderToggle(_panel.transform);
        CreatePlayerLabel(_panel.transform);
        CreateSessionIdLabel(_panel.transform);
        CreateRoomIdField(_panel.transform);
        CreateStatusLabel(_panel.transform);
        CreateHostButton(_panel.transform);
        CreateJoinButton(_panel.transform);
        CreateLeaveButton(_panel.transform);
        CreateCloseButton(_panel.transform);
    }

    private void CreateTitle(Transform parent)
    {
        Text title = CreateText("LobbyTitle", parent, "Steam Co-op Lobby",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(800, 44));
        title.fontSize = 28;
    }

    private void CreateProviderToggle(Transform parent)
    {
        CreateText("ProviderLabel", parent, "Provider:",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-250, -84), new Vector2(90, 30));

        _photonToggleButton = CreateToggleButton(parent, "PhotonToggle", "PHOTON",
            new Vector2(-120, -84), PhotonToggleClicked, out _photonToggleBackground);
        _steamToggleButton = CreateToggleButton(parent, "SteamToggle", "STEAM",
            new Vector2(60, -84), SteamToggleClicked, out _steamToggleBackground);

        ApplyToggleHighlight();
    }

    private void CreatePlayerLabel(Transform parent)
    {
        _playerLabel = CreateText("PlayerLabel", parent, "Steam ID: -",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -142), new Vector2(800, 28));
    }

    private void CreateSessionIdLabel(Transform parent)
    {
        _sessionIdLabel = CreateText("SessionIdLabel", parent, "ROOM_ID: -",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -176), new Vector2(800, 28));
    }

    private void CreateRoomIdField(Transform parent)
    {
        GameObject fieldObject = CreateUiObject("RoomIdField", parent);
        RectTransform rect = fieldObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -216);
        rect.sizeDelta = new Vector2(760, 42);

        Image background = fieldObject.AddComponent<Image>();
        background.color = new Color(1, 1, 1, 0.9f);

        _roomIdField = fieldObject.AddComponent<InputField>();
        _roomIdField.targetGraphic = background;

        Text placeholder = CreateText("RoomIdPlaceholder", fieldObject.transform, "Host's ROOM_ID",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(740, 34));
        placeholder.color = new Color(0.2f, 0.2f, 0.2f);
        _roomIdField.placeholder = placeholder;

        Text inputText = CreateText("RoomIdText", fieldObject.transform, "",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(740, 34));
        inputText.color = new Color(0.1f, 0.1f, 0.1f);
        inputText.raycastTarget = true;
        _roomIdField.textComponent = inputText;
    }

    private void CreateStatusLabel(Transform parent)
    {
        _statusLabel = CreateText("StatusLabel", parent, "Waiting for Steam...",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -268), new Vector2(800, 30));
    }

    private void CreateHostButton(Transform parent)
    {
        _hostButton = CreateActionButton(parent, "HostButton", "Host New Session",
            new Vector2(-200, -316), HostButtonClicked);
    }

    private void CreateJoinButton(Transform parent)
    {
        _joinButton = CreateActionButton(parent, "JoinButton", "Join Session",
            new Vector2(200, -316), JoinButtonClicked);
    }

    private void CreateLeaveButton(Transform parent)
    {
        _leaveButton = CreateActionButton(parent, "LeaveButton", "Leave Session",
            new Vector2(0, -346), LeaveButtonClicked);
    }

    private void CreateCloseButton(Transform parent)
    {
        GameObject buttonObject = CreateUiObject("CloseButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(476, 334);
        rect.sizeDelta = new Vector2(32, 32);

        Image background = buttonObject.AddComponent<Image>();
        SoundSettingsSprites sprites = Resources.Load<SoundSettingsSprites>(_soundSettingsSpritesPath);
        Sprite closeIcon = sprites != null ? sprites.CloseIcon : null;
        if (closeIcon != null)
        {
            background.sprite = closeIcon;
            background.color = Color.white;
        }
        else
        {
            background.color = new Color(0, 0, 0, 0.75f);
        }

        _closeButton = buttonObject.AddComponent<Button>();
        _closeButton.targetGraphic = background;
        _closeButton.onClick.AddListener(ClosePanel);
    }

    private Button CreateToggleButton(Transform parent, string name, string label, Vector2 position,
        UnityEngine.Events.UnityAction onClick, out Image background)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(120, 36);

        background = buttonObject.AddComponent<Image>();
        background.color = _inactiveToggleColor;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        Text buttonLabel = CreateText(name + "Label", buttonObject.transform, label,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(112, 30));
        buttonLabel.raycastTarget = false;

        return button;
    }

    private Button CreateActionButton(Transform parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(240, 44);

        Image background = buttonObject.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);

        Text buttonLabel = CreateText(name + "Label", buttonObject.transform, label,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(230, 38));
        buttonLabel.raycastTarget = false;

        return button;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name);
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
    }

    private Text CreateText(string name, Transform parent, string text,
        Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = CreateUiObject(name, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text uiText = textObject.AddComponent<Text>();
        uiText.text = text;
        uiText.font = _font != null ? _font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        uiText.fontSize = 20;
        uiText.color = _labelColor;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.raycastTarget = false;
        return uiText;
    }
}
