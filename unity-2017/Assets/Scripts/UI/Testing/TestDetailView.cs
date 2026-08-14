using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The detail area of the TEST menu browser: a preview image, a text line (name/source) and a
/// sound line with playback.
/// </summary>
public class TestDetailView
{
    private readonly Image _image;
    private readonly Text _detailText;
    private readonly Text _soundText;

    /// <summary>Initializes a new instance of the <see cref="TestDetailView"/> class.</summary>
    /// <param name="image">The preview image.</param>
    /// <param name="detailText">The detail text.</param>
    /// <param name="soundText">The sound text.</param>
    public TestDetailView(Image image, Text detailText, Text soundText)
    {
        _image = image;
        _detailText = detailText;
        _soundText = soundText;
    }

    /// <summary>Shows a text detail line.</summary>
    /// <param name="detail">The detail text.</param>
    public void ShowText(string detail)
    {
        _detailText.text = detail;
    }

    /// <summary>Loads a sprite by resource path into the preview image.</summary>
    /// <param name="resourcePath">The resources path of the sprite; empty hides the sprite.</param>
    public void ShowImage(string resourcePath)
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            _image.sprite = null;
            _image.color = new Color(0, 0, 0, 0.35f);
            return;
        }

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

    /// <summary>Plays an FMOD event by path and shows its name.</summary>
    /// <param name="eventPath">The FMOD event path.</param>
    public void PlaySound(string eventPath)
    {
        try
        {
            DarkestSoundManager.PlayOneShot(eventPath);
            _soundText.text = eventPath;
        }
        catch (System.Exception ex)
        {
            _soundText.text = "Play failed: " + ex.Message;
        }
    }
}
