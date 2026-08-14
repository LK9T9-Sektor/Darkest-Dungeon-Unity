using UnityEngine;
using UnityEngine.UI;

using Sektor.DarkestDungeon.Core.Ui;

/// <summary>
/// Single preview control of the TEST menu browser. Shows the clicked entry's image (when it has
/// one) and its text/source in one clipped area; sound entries play and show their path in the
/// same text. The content expands inside a fixed viewport and never escapes the panel bounds.
/// </summary>
public class TestDetailView
{
    private const float ImageHeight = 220f;
    private const float ImageSize = 200f;

    private readonly RectTransform _content;
    private readonly Image _image;
    private readonly Text _text;

    /// <summary>Initializes a new instance of the <see cref="TestDetailView"/> class and builds its UI.</summary>
    /// <param name="parent">The parent transform (panel).</param>
    /// <param name="position">The anchored position of the viewport (top-left anchored).</param>
    /// <param name="size">The viewport size.</param>
    public TestDetailView(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject viewportObject = RuntimeUiFactory.CreateUiObject("TestDetailViewport", parent);
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        viewport.anchorMin = new Vector2(0, 1f);
        viewport.anchorMax = new Vector2(0, 1f);
        viewport.pivot = new Vector2(0, 1f);
        viewport.anchoredPosition = position;
        viewport.sizeDelta = size;

        Image mask = viewportObject.AddComponent<Image>();
        mask.color = new Color(0, 0, 0, 0.35f);
        mask.raycastTarget = true;
        viewportObject.AddComponent<RectMask2D>();

        GameObject contentObject = RuntimeUiFactory.CreateUiObject("TestDetailContent", viewport);
        _content = contentObject.GetComponent<RectTransform>();
        _content.anchorMin = new Vector2(0, 1);
        _content.anchorMax = new Vector2(1, 1);
        _content.pivot = new Vector2(0.5f, 1);
        _content.anchoredPosition = new Vector2(0, 0);
        _content.sizeDelta = new Vector2(0, size.y);

        GameObject imageObject = RuntimeUiFactory.CreateUiObject("TestDetailImage", _content);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 1);
        imageRect.anchorMax = new Vector2(0.5f, 1);
        imageRect.pivot = new Vector2(0.5f, 1);
        imageRect.anchoredPosition = new Vector2(0, 0);
        imageRect.sizeDelta = new Vector2(ImageSize, ImageHeight);
        _image = imageObject.AddComponent<Image>();
        _image.color = new Color(0, 0, 0, 0.35f);

        _text = RuntimeUiFactory.CreateText("TestDetailText", _content, "",
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(0, 0), new Vector2(size.x, size.y),
            UiStyle.LogBody, UiStyle.Label, TextAnchor.UpperLeft);
        _text.horizontalOverflow = HorizontalWrapMode.Wrap;
        _text.verticalOverflow = VerticalWrapMode.Overflow;
        _text.raycastTarget = false;
    }

    /// <summary>Loads a sprite by resource path into the preview; empty path hides the image.</summary>
    /// <param name="resourcePath">The resources path of the sprite.</param>
    public void ShowImage(string resourcePath)
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            _image.sprite = null;
            _image.color = new Color(0, 0, 0, 0.35f);
        }
        else
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                _image.sprite = sprite;
                _image.color = Color.white;
            }
            else
            {
                _image.sprite = null;
                _image.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            }
        }

        Relayout();
    }

    /// <summary>Shows the detail text in the preview.</summary>
    /// <param name="text">The detail text.</param>
    public void ShowText(string text)
    {
        _text.text = text;
        Relayout();
    }

    /// <summary>Plays an FMOD event by path and shows its path in the preview text.</summary>
    /// <param name="eventPath">The FMOD event path.</param>
    public void PlaySound(string eventPath)
    {
        try
        {
            DarkestSoundManager.PlayOneShot(eventPath);
            ShowText("SOUND: " + eventPath);
        }
        catch (System.Exception ex)
        {
            ShowText("Play failed: " + ex.Message);
        }
    }

    private void Relayout()
    {
        bool hasImage = _image.sprite != null;
        _image.gameObject.SetActive(hasImage);

        RectTransform textRect = _text.rectTransform;
        if (hasImage)
        {
            textRect.anchoredPosition = new Vector2(0, -(ImageHeight + 10));
            _content.sizeDelta = new Vector2(0, ImageHeight + 10 + Mathf.Max(_text.preferredHeight, 40));
        }
        else
        {
            textRect.anchoredPosition = new Vector2(0, 0);
            _content.sizeDelta = new Vector2(0, Mathf.Max(_text.preferredHeight, 40));
        }
    }
}
