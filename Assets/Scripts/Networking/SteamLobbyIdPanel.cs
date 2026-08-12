using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Persistent, reusable panel that shows the current Steam lobby id together with a
/// copy-to-clipboard button. Lives across scenes (DontDestroyOnLoad) and is shown both
/// in the shared room list right after the host creates a session and inside the
/// multiplayer dungeon, so the id can be shared with a friend at any moment. Hidden
/// when no Steam session is active or when the player is on an unrelated scene.
/// </summary>
public class SteamLobbyIdPanel : MonoBehaviour
{
    private const string _campaignSelectionSceneName = "CampaignSelection";
    private const string _dungeonMultiplayerSceneName = "DungeonMultiplayer";
    private const string _fontResourcePath = "Fonts/Deutsch";

    private const string _idLabelFormat = "Steam Lobby ID: {0}";
    private const string _copyButtonLabel = "Copy";
    private const string _copiedLabel = "Copied!";

    private const float _copiedFeedbackSeconds = 1f;
    private const int _sortingOrder = 20000;

    private static readonly Color _labelColor = new Color(0.9338235f, 0.7924933f, 0.4463127f);
    private static readonly Color _panelBackgroundColor = new Color(0, 0, 0, 0.6f);
    private static readonly Color _buttonBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);

    private static SteamLobbyIdPanel _instanse;

    private Font _font;
    private GameObject _panel;
    private Text _idLabel;
    private float _copiedTimeLeft;

    /// <summary>Creates the persistent panel object once the first scene has loaded.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instanse != null)
            return;

        GameObject panelObject = new GameObject(nameof(SteamLobbyIdPanel));
        DontDestroyOnLoad(panelObject);
        panelObject.AddComponent<SteamLobbyIdPanel>();
    }

    private void Awake()
    {
        if (_instanse != null)
        {
            Destroy(gameObject);
            return;
        }

        _instanse = this;
        MultiplayerSync.SessionJoined += OnSessionChanged;
        MultiplayerSync.SessionEnded += OnSessionChanged;
        CreateUi();
    }

    private void OnDestroy()
    {
        MultiplayerSync.SessionJoined -= OnSessionChanged;
        MultiplayerSync.SessionEnded -= OnSessionChanged;

        if (_instanse == this)
            _instanse = null;
    }

    private void Update()
    {
        Refresh();

        if (_copiedTimeLeft > 0)
            _copiedTimeLeft -= Time.deltaTime;
    }

    /// <summary>Copies the current Steam lobby id into the system clipboard.</summary>
    private void CopyButtonClicked()
    {
        if (MultiplayerSync.Steam == null)
            return;

        string sessionId = MultiplayerSync.Steam.SessionId;
        if (sessionId.Length == 0)
            return;

        GUIUtility.systemCopyBuffer = sessionId;
        _copiedTimeLeft = _copiedFeedbackSeconds;
    }

    private void OnSessionChanged(string sessionId)
    {
        Refresh();
    }

    private void OnSessionChanged()
    {
        Refresh();
    }

    /// <summary>Shows the panel only for an active Steam session on a relevant scene.</summary>
    private void Refresh()
    {
        bool active = MultiplayerSync.IsSteamSession
            && MultiplayerSync.Steam != null
            && MultiplayerSync.Steam.SessionId.Length > 0
            && IsRelevantScene();

        if (_panel.activeSelf != active)
            _panel.SetActive(active);

        if (active)
        {
            _idLabel.text = _copiedTimeLeft > 0
                ? _copiedLabel
                : string.Format(_idLabelFormat, MultiplayerSync.Steam.SessionId);
        }
    }

    private bool IsRelevantScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName == _campaignSelectionSceneName || sceneName == _dungeonMultiplayerSceneName;
    }

    private void CreateUi()
    {
        _font = Resources.Load<Font>(_fontResourcePath);

        EnsureEventSystem();

        Canvas canvas = CreateCanvas();
        CreatePanel(canvas.transform);
        _panel.SetActive(false);
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
        GameObject canvasObject = new GameObject("SteamLobbyIdCanvas");
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = _sortingOrder;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void CreatePanel(Transform parent)
    {
        _panel = CreateUiObject("SteamLobbyIdPanel", parent);
        RectTransform rect = _panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-16, -84);
        rect.sizeDelta = new Vector2(380, 40);

        Image background = _panel.AddComponent<Image>();
        background.color = _panelBackgroundColor;

        CreateIdLabel(_panel.transform);
        CreateCopyButton(_panel.transform);
    }

    private void CreateIdLabel(Transform parent)
    {
        _idLabel = CreateText("LobbyIdLabel", parent, "",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(290, 36));
        _idLabel.fontSize = 20;
        _idLabel.alignment = TextAnchor.MiddleLeft;
    }

    private void CreateCopyButton(Transform parent)
    {
        GameObject buttonObject = CreateUiObject("CopyButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(1, 0.5f);
        rect.anchoredPosition = new Vector2(-8, 0);
        rect.sizeDelta = new Vector2(70, 32);

        Image background = buttonObject.AddComponent<Image>();
        background.color = _buttonBackgroundColor;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(CopyButtonClicked);

        Text buttonLabel = CreateText("CopyButtonLabel", buttonObject.transform, _copyButtonLabel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(66, 28));
        buttonLabel.raycastTarget = false;
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
