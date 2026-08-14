using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// Shared factory for the runtime-created UI overlays (session log, provider menu, lobby id
/// panel, sound settings). Centralizes canvas/text/button construction and the shared font so
/// the overlays stop duplicating the same wiring; styling comes from <see cref="UiStyle"/>.
/// </summary>
public static class RuntimeUiFactory
{
    private static Font _font;

    /// <summary>Gets the shared font loaded from <see cref="UiStyle.FontResource"/> (cached).</summary>
    public static Font Font
    {
        get
        {
            if (_font == null)
                _font = Resources.Load<Font>(UiStyle.FontResource);
            return _font;
        }
    }

    /// <summary>Ensures a single EventSystem exists for the runtime UI input.</summary>
    public static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject(nameof(EventSystem));
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }

    /// <summary>Creates a full-screen overlay canvas with the given sorting order.</summary>
    public static Canvas CreateCanvas(string name, Transform parent, int sortingOrder)
    {
        GameObject canvasObject = new GameObject(name);
        canvasObject.transform.SetParent(parent, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    /// <summary>Creates an empty UI object carrying a <see cref="RectTransform"/>.</summary>
    public static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name);
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
    }

    /// <summary>Creates a text label with the shared font and default style tokens.</summary>
    public static Text CreateText(string name, Transform parent, string text,
        Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size,
        int fontSize = UiStyle.Small, ArgbColor? color = null,
        TextAnchor alignment = TextAnchor.MiddleCenter)
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
        uiText.font = Font != null ? Font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        uiText.fontSize = fontSize;
        uiText.color = color.HasValue ? ToColor(color.Value) : ToColor(UiStyle.Label);
        uiText.alignment = alignment;
        uiText.raycastTarget = false;
        return uiText;
    }

    /// <summary>Creates a small square stepper/close button with the shared background style.</summary>
    public static Button CreateStepperButton(Transform parent, string name, Sprite sprite,
        Vector2 position, UnityEngine.Events.UnityAction onClick,
        ArgbColor? fallbackColor = null)
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
            background.color = fallbackColor.HasValue ? ToColor(fallbackColor.Value) : ToColor(UiStyle.ButtonBackground);
        }

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(onClick);
        return button;
    }

    /// <summary>Converts an engine-free <see cref="ArgbColor"/> into a Unity color value.</summary>
    public static Color ToColor(ArgbColor color)
    {
        return new Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }
}
