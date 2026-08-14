using UnityEngine;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// Persistent in-game chat/log button shown below the settings gear button. Opens a
/// regular window with the multiplayer log, whose lines mirror the Steam console
/// format of the transport implementation ("[HH:mm:ss.fff] [Category] message") and
/// are kept in <see cref="MultiplayerSync"/>. Created once per play session and lives
/// across scenes (DontDestroyOnLoad), like the sound settings window.
/// </summary>
public class MultiplayerLogUI : MonoBehaviour
{
    private const string _soundSettingsSpritesPath = "UI/SoundSettingsSprites";

    private const string _chatButtonLabel = "SEND";
    private const string _windowTitleLabel = "SESSION LOG";

    private const int _sortingOrder = 25000;

    private static MultiplayerLogUI _instanse;

    private SoundSettingsSprites _sprites;
    private GameObject _window;
    private Text _logText;
    private ScrollRect _scrollRect;
    private RectTransform _contentRect;

    /// <summary>Creates the persistent log UI object once the first scene has loaded.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instanse != null)
            return;

        GameObject logUiObject = new GameObject(nameof(MultiplayerLogUI));
        DontDestroyOnLoad(logUiObject);
        logUiObject.AddComponent<MultiplayerLogUI>();
    }

    private void Awake()
    {
        if (_instanse != null)
        {
            Destroy(gameObject);
            return;
        }

        _instanse = this;
        CreateUi();
    }

    private void OnDestroy()
    {
        if (_instanse == this)
            _instanse = null;
    }

    private void Update()
    {
        if (_window != null && _window.activeSelf)
            RefreshLog();
    }

    /// <summary>Toggles the log window and refreshes its content.</summary>
    private void ChatButtonClicked()
    {
        if (_window == null)
            return;

        bool show = !_window.activeSelf;
        _window.SetActive(show);
        if (show)
            RefreshLog();
    }

    /// <summary>Rebuilds the log text and pins the scroll view to the latest entries.</summary>
    private void RefreshLog()
    {
        string[] lines = MultiplayerSync.LogLines;
        _logText.text = lines.Length > 0
            ? string.Join("\n", lines)
            : "No multiplayer activity yet.";

        _contentRect.sizeDelta = new Vector2(0, Mathf.Max(_logText.preferredHeight, 200));
        _scrollRect.verticalNormalizedPosition = 0;
    }

    private void CreateUi()
    {
        _sprites = Resources.Load<SoundSettingsSprites>(_soundSettingsSpritesPath);

        RuntimeUiFactory.EnsureEventSystem();

        Canvas canvas = RuntimeUiFactory.CreateCanvas("MultiplayerLogCanvas", transform, _sortingOrder);
        CreateChatButton(canvas.transform);
        CreateWindow(canvas.transform);
        _window.SetActive(false);
    }

    private void CreateChatButton(Transform parent)
    {
        GameObject buttonObject = RuntimeUiFactory.CreateUiObject("ChatButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-16, -80);
        rect.sizeDelta = new Vector2(56, 56);

        Image background = buttonObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(ChatButtonClicked);

        Text buttonLabel = RuntimeUiFactory.CreateText("ChatButtonLabel", buttonObject.transform, _chatButtonLabel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(54, 54),
            UiStyle.Body, UiStyle.Label);
    }

    private void CreateWindow(Transform parent)
    {
        _window = RuntimeUiFactory.CreateUiObject("LogWindow", parent);
        RectTransform rect = _window.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(900, 540);

        Image background = _window.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        CreateTitle(_window.transform);
        CreateCloseButton(_window.transform);
        CreateLogScrollArea(_window.transform);
    }

    private void CreateTitle(Transform parent)
    {
        Text title = RuntimeUiFactory.CreateText("LogWindowTitle", parent, _windowTitleLabel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(600, 40),
            UiStyle.Title, UiStyle.Label);
    }

    private void CreateCloseButton(Transform parent)
    {
        Sprite closeIcon = _sprites != null ? _sprites.CloseIcon : null;
        RuntimeUiFactory.CreateStepperButton(parent, "LogCloseButton", closeIcon, new Vector2(418, 246), CloseWindow);
    }

    private void CloseWindow()
    {
        if (_window != null)
            _window.SetActive(false);
    }

    private void CreateLogScrollArea(Transform parent)
    {
        GameObject viewportObject = RuntimeUiFactory.CreateUiObject("LogViewport", parent);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0.5f, 0.5f);
        viewportRect.anchorMax = new Vector2(0.5f, 0.5f);
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.anchoredPosition = new Vector2(0, -20);
        viewportRect.sizeDelta = new Vector2(860, 470);

        Image viewportMask = viewportObject.AddComponent<Image>();
        viewportMask.color = new Color(0, 0, 0, 0.35f);
        viewportMask.raycastTarget = true;
        viewportObject.AddComponent<RectMask2D>();

        GameObject contentObject = RuntimeUiFactory.CreateUiObject("LogContent", viewportObject.transform);
        _contentRect = contentObject.GetComponent<RectTransform>();
        _contentRect.anchorMin = new Vector2(0, 1);
        _contentRect.anchorMax = new Vector2(1, 1);
        _contentRect.pivot = new Vector2(0.5f, 1);
        _contentRect.anchoredPosition = new Vector2(0, 0);
        _contentRect.sizeDelta = new Vector2(0, 200);

        _logText = RuntimeUiFactory.CreateText("LogText", contentObject.transform, "",
            new Vector2(0, 1f), new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(840, 200),
            UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft);
        _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _logText.verticalOverflow = VerticalWrapMode.Overflow;

        ScrollRect scrollRect = viewportObject.AddComponent<ScrollRect>();
        scrollRect.content = _contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        _scrollRect = scrollRect;
    }
}
