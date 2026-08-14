using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Ui;

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
    private const string _soundSettingsSpritesPath = "UI/SoundSettingsSprites";

    private const int _sortingOrder = 10000;

    private static MultiplayerProviderMenu _instanse;

    private readonly bool[] _providers = new bool[] { false, true };

    private SoundSettingsSprites _sprites;
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
            MultiplayerSync.WriteError("MULTIPLAYER", "Provider menu unavailable.");
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
        _sprites = Resources.Load<SoundSettingsSprites>(_soundSettingsSpritesPath);

        RuntimeUiFactory.EnsureEventSystem();

        Canvas canvas = RuntimeUiFactory.CreateCanvas("MultiplayerProviderCanvas", transform, _sortingOrder);
        _panel = CreatePanel(canvas.transform);
        _panel.SetActive(false);
    }

    private void SelectRow(int index)
    {
        _selectedIndex = index;

        for (int i = 0; i < _rowBackgrounds.Length; i++)
        {
            _rowBackgrounds[i].color = i == _selectedIndex
                ? RuntimeUiFactory.ToColor(UiStyle.SelectedRow)
                : RuntimeUiFactory.ToColor(UiStyle.IdleRow);
            _rowLabels[i].color = i == _selectedIndex
                ? RuntimeUiFactory.ToColor(UiStyle.Label)
                : RuntimeUiFactory.ToColor(UiStyle.Label);
        }
    }

    private void ConfirmSelection()
    {
        bool steam = _providers[_selectedIndex];
        MultiplayerSync.WriteLog("MULTIPLAYER", "Provider selected: " + (steam ? "STEAM" : "PHOTON") + ".");
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

        MultiplayerSync.WriteLog("MULTIPLAYER", "Provider menu closed.");
    }

    private void ConfirmRow(int index)
    {
        SelectRow(index);
        ConfirmSelection();
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panelObject = RuntimeUiFactory.CreateUiObject("ProviderMenuPanel", parent);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(640, 420);

        Image background = panelObject.AddComponent<Image>();
        background.color = RuntimeUiFactory.ToColor(UiStyle.PanelBackground);

        CreateTitle(panelObject.transform);
        CreateProviderRows(panelObject.transform);
        CreateHintLabel(panelObject.transform);
        CreateCloseButton(panelObject.transform);

        return panelObject;
    }

    private void CreateCloseButton(Transform parent)
    {
        Sprite closeIcon = _sprites != null ? _sprites.CloseIcon : null;
        RuntimeUiFactory.CreateStepperButton(parent, "CloseButton", closeIcon, new Vector2(296, 186), ClosePanel);
    }

    private void CreateTitle(Transform parent)
    {
        RuntimeUiFactory.CreateText("ProviderTitle", parent, "MULTIPLAYER",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -24), new Vector2(600, 48),
            UiStyle.LargeTitle, UiStyle.Label);
    }

    private void CreateProviderRows(Transform parent)
    {
        _rowBackgrounds = new Image[_providers.Length];
        _rowLabels = new Text[_providers.Length];

        for (int i = 0; i < _providers.Length; i++)
        {
            bool steam = _providers[i];
            Vector2 position = new Vector2(0, -120 - i * 88);

            GameObject rowObject = RuntimeUiFactory.CreateUiObject(steam ? "SteamRow" : "PhotonRow", parent);
            RectTransform rect = rowObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(560, 68);

            Image rowBackground = rowObject.AddComponent<Image>();
            rowBackground.color = RuntimeUiFactory.ToColor(UiStyle.IdleRow);
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

            Text rowLabel = RuntimeUiFactory.CreateText(steam ? "SteamRowLabel" : "PhotonRowLabel", rowObject.transform,
                steam ? "STEAM" : "PHOTON",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(540, 56),
                UiStyle.RowLabel, UiStyle.Label);
            _rowLabels[i] = rowLabel;
        }
    }

    private void CreateHintLabel(Transform parent)
    {
        Text hint = RuntimeUiFactory.CreateText("ProviderHint", parent, "Use up / down arrows to choose, Enter to confirm, Esc to cancel",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 26), new Vector2(800, 34),
            UiStyle.Small, UiStyle.Label);
        hint.horizontalOverflow = HorizontalWrapMode.Wrap;
    }
}
