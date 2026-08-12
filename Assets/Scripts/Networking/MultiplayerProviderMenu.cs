using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runtime-created provider selection overlay shown on the campaign selection screen
/// when the player enters multiplayer. Presents the two providers (PHOTON / STEAM) as
/// large rows navigable with the up/down arrows (Enter confirms, Escape cancels) or by
/// mouse. Choosing a provider persists the selection, initializes the provider when
/// required (Steam session manager) and opens the shared room list, which reuses the
/// legacy hero selection for both providers. Created anew per visit.
/// </summary>
public class MultiplayerProviderMenu : MonoBehaviour
{
    private const string _campaignSelectionSceneName = "CampaignSelection";
    private const string _fontResourcePath = "Fonts/Deutsch";
    private const string _soundSettingsSpritesPath = "UI/SoundSettingsSprites";

    private static readonly Color _labelColor = new Color(0.9338235f, 0.7924933f, 0.4463127f);
    private static readonly Color _selectedRowColor = new Color(0.45f, 0.38f, 0.2f, 0.95f);
    private static readonly Color _idleRowColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
    private static readonly Color _idleRowTextColor = new Color(0.55f, 0.55f, 0.55f);

    private static MultiplayerProviderMenu _instanse;

    private readonly bool[] _providers = new bool[] { false, true };

    private Font _font;
    private GameObject _panel;
    private Image[] _rowBackgrounds;
    private Text[] _rowLabels;
    private int _selectedIndex;

    /// <summary>Creates the menu object once the campaign selection scene has loaded.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instanse != null)
            return;

        if (SceneManager.GetActiveScene().name != _campaignSelectionSceneName)
            return;

        GameObject menuObject = new GameObject(nameof(MultiplayerProviderMenu));
        menuObject.AddComponent<MultiplayerProviderMenu>();
    }

    /// <summary>
    /// Opens the provider selection menu. Creates the menu object on demand when absent,
    /// so the overlay works regardless of the scene the game started in.
    /// </summary>
    public static void Open()
    {
        EnsureMenu();

        if (_instanse == null)
        {
            Debug.LogError("[MULTIPLAYER] Provider menu unavailable.");
            return;
        }

        _instanse.SelectRow(MultiplayerSync.IsSteamProvider ? 1 : 0);
        _instanse._panel.SetActive(true);
    }

    /// <summary>Creates the menu object when absent.</summary>
    public static void EnsureMenu()
    {
        if (_instanse != null)
            return;

        GameObject menuObject = new GameObject(nameof(MultiplayerProviderMenu));
        menuObject.AddComponent<MultiplayerProviderMenu>();
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
        if (_panel == null || !_panel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            SelectRow((_selectedIndex + _providers.Length - 1) % _providers.Length);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            SelectRow((_selectedIndex + 1) % _providers.Length);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ConfirmSelection();

        if (Input.GetKeyUp(KeyCode.Escape))
            ClosePanel();
    }

    private void CreateUi()
    {
        _font = Resources.Load<Font>(_fontResourcePath);

        EnsureEventSystem();

        Canvas canvas = CreateCanvas();
        _panel = CreatePanel(canvas.transform);
        _panel.SetActive(false);
    }

    private void SelectRow(int index)
    {
        _selectedIndex = index;

        for (int i = 0; i < _rowBackgrounds.Length; i++)
        {
            _rowBackgrounds[i].color = i == _selectedIndex ? _selectedRowColor : _idleRowColor;
            _rowLabels[i].color = i == _selectedIndex ? _labelColor : _idleRowTextColor;
        }
    }

    private void ConfirmSelection()
    {
        bool steam = _providers[_selectedIndex];
        Debug.Log("[MULTIPLAYER] Provider selected: " + (steam ? "STEAM" : "PHOTON") + ".");
        MultiplayerSync.SetSteamProvider(steam);

        if (steam)
            MultiplayerSync.EnsureSteamSession();

        ClosePanel();

        RoomSelector roomSelector = FindObjectOfType<RoomSelector>();
        if (roomSelector != null)
            roomSelector.OpenRoomList();
    }

    private void ClosePanel()
    {
        if (_panel != null)
            _panel.SetActive(false);

        Debug.Log("[MULTIPLAYER] Provider menu closed.");
    }

    private void ConfirmRow(int index)
    {
        SelectRow(index);
        ConfirmSelection();
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
        GameObject canvasObject = new GameObject("MultiplayerProviderCanvas");
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

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panelObject = CreateUiObject("ProviderMenuPanel", parent);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(640, 420);

        Image background = panelObject.AddComponent<Image>();
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

        CreateTitle(panelObject.transform);
        CreateProviderRows(panelObject.transform);
        CreateHintLabel(panelObject.transform);

        return panelObject;
    }

    private void CreateTitle(Transform parent)
    {
        Text title = CreateText("ProviderTitle", parent, "MULTIPLAYER",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -24), new Vector2(600, 48));
        title.fontSize = 34;
    }

    private void CreateProviderRows(Transform parent)
    {
        _rowBackgrounds = new Image[_providers.Length];
        _rowLabels = new Text[_providers.Length];

        for (int i = 0; i < _providers.Length; i++)
        {
            bool steam = _providers[i];
            Vector2 position = new Vector2(0, -120 - i * 88);

            GameObject rowObject = CreateUiObject(steam ? "SteamRow" : "PhotonRow", parent);
            RectTransform rect = rowObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(560, 68);

            Image rowBackground = rowObject.AddComponent<Image>();
            rowBackground.color = _idleRowColor;
            _rowBackgrounds[i] = rowBackground;

            Button rowButton = rowObject.AddComponent<Button>();
            rowButton.targetGraphic = rowBackground;
            int capturedIndex = i;
            rowButton.onClick.AddListener(() => ConfirmRow(capturedIndex));

            EventTrigger trigger = rowObject.AddComponent<EventTrigger>();
            EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
            hoverEntry.eventID = EventTriggerType.PointerEnter;
            hoverEntry.callback.AddListener(delegate { SelectRow(capturedIndex); });
            trigger.triggers.Add(hoverEntry);

            Text rowLabel = CreateText(steam ? "SteamRowLabel" : "PhotonRowLabel", rowObject.transform,
                steam ? "STEAM" : "PHOTON",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(540, 56));
            rowLabel.fontSize = 40;
            _rowLabels[i] = rowLabel;
        }
    }

    private void CreateHintLabel(Transform parent)
    {
        Text hint = CreateText("ProviderHint", parent, "Use up / down arrows to choose, Enter to confirm, Esc to cancel",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 26), new Vector2(600, 30));
        hint.fontSize = 20;
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
