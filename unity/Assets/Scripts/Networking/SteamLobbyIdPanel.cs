using UnityEngine;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// Persistent, reusable panel that shows the current Steam lobby id together with a
/// copy-to-clipboard button. Lives across scenes (DontDestroyOnLoad) and is shown
/// whenever a Steam session is active, in the shared room list and inside the
/// multiplayer dungeon, so the id can be shared with a friend at any moment. Hidden
/// only when no Steam session is active.
/// </summary>
public class SteamLobbyIdPanel : MonoBehaviour
{
    private const string _idLabelFormat = "Steam Lobby ID: {0}";
    private const string _copyButtonLabel = "Copy";
    private const string _copiedLabel = "Copied!";

    private const float _copiedFeedbackSeconds = 1f;
    private const int _sortingOrder = 20000;

    private static SteamLobbyIdPanel _instanse;

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

    /// <summary>Shows the panel for an active Steam session regardless of the current scene.</summary>
    private void Refresh()
    {
        bool active = MultiplayerSync.IsSteamSession
            && MultiplayerSync.Steam != null
            && MultiplayerSync.Steam.SessionId.Length > 0;

        if (_panel.activeSelf != active)
            _panel.SetActive(active);

        if (active)
        {
            _idLabel.text = _copiedTimeLeft > 0
                ? _copiedLabel
                : string.Format(_idLabelFormat, MultiplayerSync.Steam.SessionId);
        }
    }

    private void CreateUi()
    {
        RuntimeUiFactory.EnsureEventSystem();

        Canvas canvas = RuntimeUiFactory.CreateCanvas("SteamLobbyIdCanvas", transform, _sortingOrder);
        CreatePanel(canvas.transform);
        _panel.SetActive(false);
    }

    private void CreatePanel(Transform parent)
    {
        _panel = RuntimeUiFactory.CreateUiObject("SteamLobbyIdPanel", parent);
        RectTransform rect = _panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-90, -20);
        rect.sizeDelta = new Vector2(380, 40);

        Image background = _panel.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        CreateIdLabel(_panel.transform);
        CreateCopyButton(_panel.transform);
    }

    private void CreateIdLabel(Transform parent)
    {
        _idLabel = RuntimeUiFactory.CreateText("LobbyIdLabel", parent, "",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(290, 36),
            UiStyle.Small, UiStyle.Label, TextAnchor.MiddleLeft);
    }

    private void CreateCopyButton(Transform parent)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject("CopyButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(1, 0.5f);
        rect.anchoredPosition = new Vector2(-8, 0);
        rect.sizeDelta = new Vector2(70, 32);

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.ButtonBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(CopyButtonClicked);

        RuntimeUiFactory.CreateText("CopyButtonLabel", buttonObject.transform, _copyButtonLabel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(66, 28),
            UiStyle.Small, UiStyle.Label);
    }
}
