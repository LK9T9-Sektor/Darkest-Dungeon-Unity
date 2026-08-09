using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Runtime-created in-game sound settings window that persists across all scenes.
/// Exposes two volume sliders (music and SFX) that drive DarkestSoundManager setters.
/// </summary>
public class SoundSettingsUI : MonoBehaviour
{
    private const string _musicVolumeLabelKey = "menu_options_element_music_volume";
    private const string _sfxVolumeLabelKey = "menu_options_element_sfx_volume";
    private const string _audioCategoryLabelKey = "options_category_audio";

    private const string _musicVolumeLabelFallback = "Music Volume";
    private const string _sfxVolumeLabelFallback = "SFX Volume";
    private const string _audioCategoryLabelFallback = "Audio";

    private static SoundSettingsUI _instanse;

    private GameObject _panel;
    private Slider _musicSlider;
    private Slider _sfxSlider;
    private Text _musicLabel;
    private Text _sfxLabel;
    private Text _buttonLabel;
    private bool _musicLabelLocalized;
    private bool _sfxLabelLocalized;
    private bool _buttonLabelLocalized;
    private bool _slidersSynced;

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
        if (!_slidersSynced && DarkestSoundManager.Instanse != null)
        {
            _musicSlider.value = DarkestSoundManager.MusicVolume;
            _sfxSlider.value = DarkestSoundManager.SfxVolume;
            _slidersSynced = true;
        }

        if (!_musicLabelLocalized)
            _musicLabelLocalized = TryLocalize(_musicLabel, _musicVolumeLabelKey, _musicVolumeLabelFallback);
        if (!_sfxLabelLocalized)
            _sfxLabelLocalized = TryLocalize(_sfxLabel, _sfxVolumeLabelKey, _sfxVolumeLabelFallback);
        if (!_buttonLabelLocalized)
            _buttonLabelLocalized = TryLocalize(_buttonLabel, _audioCategoryLabelKey, _audioCategoryLabelFallback);
    }

    private void CreateUi()
    {
        EnsureEventSystem();

        Canvas canvas = CreateCanvas();
        _panel = CreatePanel(canvas.transform);
        _panel.SetActive(false);

        _musicSlider = CreateVolumeRow(_panel.transform, 0, _musicVolumeLabelFallback,
            DarkestSoundManager.MusicVolume, out _musicLabel, DarkestSoundManager.SetMusicVolume);
        _sfxSlider = CreateVolumeRow(_panel.transform, 1, _sfxVolumeLabelFallback,
            DarkestSoundManager.SfxVolume, out _sfxLabel, DarkestSoundManager.SetSfxVolume);

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

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("SoundSettingsCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void CreateButton(Transform parent)
    {
        GameObject buttonObject = CreateUiObject("AudioButton", parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-16, 16);
        rect.sizeDelta = new Vector2(140, 44);

        Image background = buttonObject.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.75f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(TogglePanel);

        _buttonLabel = CreateText("AudioLabel", buttonObject.transform, _audioCategoryLabelFallback,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140, 44));
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panelObject = CreateUiObject("AudioPanel", parent);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-16, 72);
        rect.sizeDelta = new Vector2(320, 176);

        Image background = panelObject.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.85f);
        return panelObject;
    }

    private Slider CreateVolumeRow(Transform parent, int index, string labelFallback,
        float initialValue, out Text label, UnityEngine.Events.UnityAction<float> onValueChanged)
    {
        float rowY = -(24 + index * 64);

        label = CreateText("VolumeLabel", parent, labelFallback,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-110, rowY), new Vector2(200, 28));

        return CreateVolumeSlider(parent, rowY, initialValue, onValueChanged);
    }

    private static Slider CreateVolumeSlider(Transform parent, float rowY, float initialValue,
        UnityEngine.Events.UnityAction<float> onValueChanged)
    {
        GameObject sliderObject = CreateUiObject("VolumeSlider", parent);
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(60, rowY);
        sliderRect.sizeDelta = new Vector2(200, 18);

        Image background = sliderObject.AddComponent<Image>();
        background.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.targetGraphic = background;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = initialValue;
        slider.onValueChanged.AddListener(onValueChanged);

        GameObject fillObject = CreateUiObject("Fill", sliderObject.transform);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fill = fillObject.AddComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.color = new Color(0.75f, 0.62f, 0.25f, 1f);
        slider.fillRect = fillRect;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private static Text CreateText(string name, Transform parent, string text,
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
        uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        uiText.fontSize = 18;
        uiText.color = Color.white;
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

    private void TogglePanel()
    {
        _panel.SetActive(!_panel.activeSelf);
    }

    private static bool TryLocalize(Text label, string key, string fallback)
    {
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
