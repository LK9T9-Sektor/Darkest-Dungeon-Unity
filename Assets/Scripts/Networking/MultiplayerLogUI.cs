using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Persistent in-game chat/log button shown below the settings gear button. Opens a
/// regular window with the multiplayer log, whose lines mirror the Steam console
/// format of the transport implementation ("[HH:mm:ss.fff] [Category] message") and
/// are kept in <see cref="MultiplayerSync"/>. Created once per play session and lives
/// across scenes (DontDestroyOnLoad), like the sound settings window.
/// </summary>
public class MultiplayerLogUI : MonoBehaviour
{
    private const string _fontResourcePath = "Fonts/DwarvenAxe";
    private const string _soundSettingsSpritesPath = "UI/SoundSettingsSprites";

    private const string _chatButtonLabel = "SEND";
    private const string _windowTitleLabel = "SESSION LOG";

    private const int _sortingOrder = 25000;
    private const int _fontSize = 18;
    private const int _titleFontSize = 24;

    private static readonly Color _labelColor = new Color(0.78431374f, 0.7058824f, 0.43137255f);
    private static readonly Color _panelBackgroundColor = new Color(0, 0, 0, 0.95f);

    private static MultiplayerLogUI _instanse;

    private Font _font;
    private SoundSettingsSprites _sprites;
    private GameObject _panel;
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
        _font = Resources.Load<Font>(_fontResourcePath);
        _sprites = Resources.Load<SoundSettingsSprites>(_soundSettingsSpritesPath);

        EnsureEventSystem();

        Canvas canvas = CreateCanvas();
        CreateChatButton(canvas.transform);
        CreateWindow(canvas.transform);
        _window.SetActive(false);
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
        GameObject canvasObject = new GameObject("MultiplayerLogCanvas");
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

    private void CreateChatButton(Transform parent)
    {
        GameObject buttonObject = CreateUiObject("ChatButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-16, -80);
        rect.sizeDelta = new Vector2(56, 56);

        Image background = buttonObject.AddComponent<Image>();
        background.color = _panelBackgroundColor;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(ChatButtonClicked);

        Text buttonLabel = CreateText("ChatButtonLabel", buttonObject.transform, _chatButtonLabel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(54, 54));
        buttonLabel.fontSize = _fontSize;
        buttonLabel.color = _labelColor;
        buttonLabel.raycastTarget = false;
    }

    private void CreateWindow(Transform parent)
    {
        _window = CreateUiObject("LogWindow", parent);
        RectTransform rect = _window.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(760, 480);

        Image background = _window.AddComponent<Image>();
        background.color = _panelBackgroundColor;

        CreateTitle(_window.transform);
        CreateCloseButton(_window.transform);
        CreateLogScrollArea(_window.transform);
    }

    private void CreateTitle(Transform parent)
    {
        Text title = CreateText("LogWindowTitle", parent, _windowTitleLabel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(600, 40));
        title.fontSize = _titleFontSize;
        title.color = _labelColor;
    }

    private void CreateCloseButton(Transform parent)
    {
        Sprite closeIcon = _sprites != null ? _sprites.CloseIcon : null;
        CreateStepperButton(parent, "LogCloseButton", closeIcon, new Vector2(348, 216), CloseWindow);
    }

    private void CreateStepperButton(Transform parent, string name, Sprite sprite, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(32, 32);

        Image background = buttonObject.AddComponent<Image>();
        if (sprite != null)
        {
            background.sprite = sprite;
            background.color = Color.white;
        }
        else
        {
            background.color = new Color(0, 0, 0, 0.75f);
        }

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);
    }

    private void CloseWindow()
    {
        if (_window != null)
            _window.SetActive(false);
    }

    private void CreateLogScrollArea(Transform parent)
    {
        GameObject viewportObject = CreateUiObject("LogViewport", parent);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0.5f, 0.5f);
        viewportRect.anchorMax = new Vector2(0.5f, 0.5f);
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        viewportRect.anchoredPosition = new Vector2(0, -20);
        viewportRect.sizeDelta = new Vector2(720, 420);

        Image viewportMask = viewportObject.AddComponent<Image>();
        viewportMask.color = new Color(0, 0, 0, 0.35f);
        viewportMask.raycastTarget = true;
        viewportObject.AddComponent<RectMask2D>();

        GameObject contentObject = CreateUiObject("LogContent", viewportObject.transform);
        _contentRect = contentObject.GetComponent<RectTransform>();
        _contentRect.anchorMin = new Vector2(0, 1);
        _contentRect.anchorMax = new Vector2(1, 1);
        _contentRect.pivot = new Vector2(0.5f, 1);
        _contentRect.anchoredPosition = new Vector2(0, 0);
        _contentRect.sizeDelta = new Vector2(0, 200);

        _logText = CreateText("LogText", contentObject.transform, "",
            new Vector2(0, 1f), new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(700, 200));
        _logText.fontSize = 17;
        _logText.color = _labelColor;
        _logText.alignment = TextAnchor.UpperLeft;
        _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _logText.verticalOverflow = VerticalWrapMode.Overflow;
        _logText.raycastTarget = false;

        ScrollRect scrollRect = viewportObject.AddComponent<ScrollRect>();
        scrollRect.content = _contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        _scrollRect = scrollRect;
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
        uiText.fontSize = _fontSize;
        uiText.color = _labelColor;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.raycastTarget = false;
        return uiText;
    }
}
