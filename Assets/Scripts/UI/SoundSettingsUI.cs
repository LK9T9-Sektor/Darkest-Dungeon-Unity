using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runtime-created in-game sound settings window that persists across all scenes.
/// Exposes two volume steppers (music and SFX) driving DarkestSoundManager setters,
/// framed with the game's menu background texture.
/// </summary>
public class SoundSettingsUI : MonoBehaviour
{
    private const string _titleLabelKey = "menu_options_title";
    private const string _musicVolumeLabelKey = "menu_options_element_music_volume";
    private const string _sfxVolumeLabelKey = "menu_options_element_sfx_volume";
    private const string _returnButtonLabelKey = "menu_base_element_exit_campaign";

    private const string _titleLabelFallback = "Settings";
    private const string _musicVolumeLabelFallback = "Music Volume";
    private const string _sfxVolumeLabelFallback = "SFX Volume";
    private const string _returnButtonLabelFallback = "Exit to Main Menu";
    private const string _campaignSelectionSceneName = "CampaignSelection";

    private const string _fontResourcePath = "Fonts/Deutsch";
    private const string _settingsButtonSpriteResourcePath = "UI/settings.button";
    private const string _soundSettingsSpritesResourcePath = "UI/SoundSettingsSprites";

    private const int _volumeSteps = 10;
    private const int _volumeStepSize = 32;
    private const float _volumeRowSpacing = 64f;
    private const float _volumeRowsTopY = 196f;
    private const int _titleFontSize = 28;

    private static readonly Color _labelColor = new Color(0.9338235f, 0.7924933f, 0.4463127f);

    private static SoundSettingsUI _instanse;

    private GameObject _panel;
    private Text _title;
    private Text _musicLabel;
    private Text _sfxLabel;
    private Text _musicValueText;
    private Text _sfxValueText;
    private Text _returnButtonLabel;
    private Font _font;
    private Sprite _settingsButtonSprite;
    private SoundSettingsSprites _sprites;
    private int _musicVolume;
    private int _sfxVolume;
    private bool _titleLocalized;
    private bool _musicLabelLocalized;
    private bool _sfxLabelLocalized;
    private bool _returnLabelLocalized;

    /// <summary>Creates the persistent settings object once the first scene has loaded.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_instanse != null)
            return;

        GameObject settingsObject = new GameObject(nameof(SoundSettingsUI));
        DontDestroyOnLoad(settingsObject);
        settingsObject.AddComponent<SoundSettingsUI>();
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

    private void Update()
    {
        if (!_titleLocalized)
            _titleLocalized = TryLocalize(_title, _titleLabelKey, _titleLabelFallback);
        if (!_musicLabelLocalized)
            _musicLabelLocalized = TryLocalize(_musicLabel, _musicVolumeLabelKey, _musicVolumeLabelFallback);
        if (!_sfxLabelLocalized)
            _sfxLabelLocalized = TryLocalize(_sfxLabel, _sfxVolumeLabelKey, _sfxVolumeLabelFallback);
        if (!_returnLabelLocalized)
            _returnLabelLocalized = TryLocalize(_returnButtonLabel, _returnButtonLabelKey, _returnButtonLabelFallback);
    }

    private void CreateUi()
    {
        _font = Resources.Load<Font>(_fontResourcePath);
        _settingsButtonSprite = Resources.Load<Sprite>(_settingsButtonSpriteResourcePath);
        _sprites = Resources.Load<SoundSettingsSprites>(_soundSettingsSpritesResourcePath);

        EnsureEventSystem();

        Canvas canvas = CreateCanvas();
        _panel = CreatePanel(canvas.transform);
        _panel.SetActive(false);

        CreateTitle(_panel.transform);

        _musicVolume = Mathf.RoundToInt(DarkestSoundManager.MusicVolume * _volumeSteps);
        _sfxVolume = Mathf.RoundToInt(DarkestSoundManager.SfxVolume * _volumeSteps);

        CreateVolumeRow(_panel.transform, 0, _musicVolumeLabelFallback,
            out _musicLabel, out _musicValueText, DecreaseMusic, IncreaseMusic);
        CreateVolumeRow(_panel.transform, 1, _sfxVolumeLabelFallback,
            out _sfxLabel, out _sfxValueText, DecreaseSfx, IncreaseSfx);

        _musicValueText.text = _musicVolume.ToString();
        _sfxValueText.text = _sfxVolume.ToString();

        CreateCloseButton(_panel.transform);
        CreateReturnButton(_panel.transform);
        CreateButton(canvas.transform);
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
        GameObject canvasObject = new GameObject("SoundSettingsCanvas");
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void CreateButton(Transform parent)
    {
        GameObject buttonObject = CreateUiObject("AudioButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-16, -16);
        rect.sizeDelta = new Vector2(56, 56);

        Image background = buttonObject.AddComponent<Image>();
        background.sprite = _settingsButtonSprite;
        background.preserveAspect = true;
        background.color = _settingsButtonSprite != null ? Color.white : new Color(0, 0, 0, 0.75f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(TogglePanel);
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panelObject = CreateUiObject("AudioPanel", parent);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(1024, 740);

        Image background = panelObject.AddComponent<Image>();
        if (_sprites != null && _sprites.WindowFrame != null)
        {
            background.sprite = _sprites.WindowFrame;
            background.color = Color.white;
        }
        else
        {
            background.color = new Color(0, 0, 0, 0.85f);
        }
        return panelObject;
    }

    private void CreateTitle(Transform parent)
    {
        _title = CreateText("SettingsTitle", parent, _titleLabelFallback,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 300), new Vector2(600, 48));
        _title.fontSize = _titleFontSize;
    }

    private void CreateCloseButton(Transform parent)
    {
        Sprite closeIcon = _sprites != null ? _sprites.CloseIcon : null;
        CreateStepperButton(parent, "CloseButton", closeIcon, new Vector2(476, 334), TogglePanel);
    }

    private void CreateReturnButton(Transform parent)
    {
        GameObject buttonObject = CreateUiObject("ReturnButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, -250);
        rect.sizeDelta = new Vector2(466, 97);

        Image background = buttonObject.AddComponent<Image>();
        Sprite exitOverlay = _sprites != null ? _sprites.ExitButtonOverlay : null;
        if (exitOverlay != null)
        {
            background.sprite = exitOverlay;
            background.color = Color.white;
        }
        else
        {
            background.color = new Color(0, 0, 0, 0.75f);
        }

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(ReturnToMainMenu);

        _returnButtonLabel = CreateText("ReturnLabel", buttonObject.transform, _returnButtonLabelFallback,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 3), new Vector2(236, 43));
        _returnButtonLabel.fontSize = 30;
    }

    private void CreateVolumeRow(Transform parent, int index, string labelFallback,
        out Text label, out Text valueText, UnityAction decrease, UnityAction increase)
    {
        float rowY = _volumeRowsTopY - index * _volumeRowSpacing;

        label = CreateText("VolumeLabel", parent, labelFallback,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-120, rowY), new Vector2(200, 28));

        valueText = CreateText("VolumeValue", parent, "0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(115, rowY), new Vector2(44, 32));
        valueText.fontSize = 22;

        Sprite minusArrow = _sprites != null ? _sprites.MinusArrow : null;
        Sprite plusArrow = _sprites != null ? _sprites.PlusArrow : null;
        CreateStepperButton(parent, "VolumeMinus", minusArrow, new Vector2(77, rowY), decrease);
        CreateStepperButton(parent, "VolumePlus", plusArrow, new Vector2(153, rowY), increase);
    }

    private void CreateStepperButton(Transform parent, string name, Sprite sprite, Vector2 position, UnityAction onClick)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(_volumeStepSize, _volumeStepSize);

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

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name);
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
    }

    private void DecreaseMusic()
    {
        ChangeVolume(-1, ref _musicVolume, _musicValueText, DarkestSoundManager.SetMusicVolume);
    }

    private void IncreaseMusic()
    {
        ChangeVolume(1, ref _musicVolume, _musicValueText, DarkestSoundManager.SetMusicVolume);
    }

    private void DecreaseSfx()
    {
        ChangeVolume(-1, ref _sfxVolume, _sfxValueText, DarkestSoundManager.SetSfxVolume);
    }

    private void IncreaseSfx()
    {
        ChangeVolume(1, ref _sfxVolume, _sfxValueText, DarkestSoundManager.SetSfxVolume);
    }

    private static void ChangeVolume(int delta, ref int value, Text valueText, UnityAction<float> setter)
    {
        value = Mathf.Clamp(value + delta, 0, _volumeSteps);
        setter(value / (float)_volumeSteps);
        if (valueText != null)
            valueText.text = value.ToString();
    }

    private void TogglePanel()
    {
        if (_panel == null)
            return;

        bool isActive = !_panel.activeSelf;
        _panel.SetActive(isActive);
        if (isActive)
            SyncValuesFromManager();
    }

    private void ReturnToMainMenu()
    {
        Debug.Log("[SOUNDSETTINGS] EXIT TO MAIN MENU CLICKED. CLOSING SETTINGS PANEL.");
        ClosePanel();
        if (DarkestDungeonManager.Instanse != null && DarkestDungeonManager.MainMenu != null)
            DarkestDungeonManager.MainMenu.ReturnToCampaignSelection();
        else
            SceneManager.LoadScene(_campaignSelectionSceneName);
    }

    private void ClosePanel()
    {
        if (_panel == null)
            return;

        _panel.SetActive(false);
    }

    private void SyncValuesFromManager()
    {
        _musicVolume = Mathf.RoundToInt(DarkestSoundManager.MusicVolume * _volumeSteps);
        _sfxVolume = Mathf.RoundToInt(DarkestSoundManager.SfxVolume * _volumeSteps);
        if (_musicValueText != null)
            _musicValueText.text = _musicVolume.ToString();
        if (_sfxValueText != null)
            _sfxValueText.text = _sfxVolume.ToString();
    }

    private static bool TryLocalize(Text label, string key, string fallback)
    {
        if (label == null)
            return false;

        try
        {
            label.text = LocalizationManager.GetString(key);
            return true;
        }
        catch
        {
            label.text = fallback;
            return false;
        }
    }
}
