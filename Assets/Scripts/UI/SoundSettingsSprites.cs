using UnityEngine;

/// <summary>
/// Serialized sprite references for the sound settings window, pointing at original game art.
/// </summary>
public class SoundSettingsSprites : ScriptableObject
{
    /// <summary>The framed background used as the window's base panel.</summary>
    public Sprite WindowFrame;

    /// <summary>The decrement arrow shown on the volume decrease button.</summary>
    public Sprite MinusArrow;

    /// <summary>The increment arrow shown on the volume increase button.</summary>
    public Sprite PlusArrow;

    /// <summary>The close X icon shown in the window's top-right corner.</summary>
    public Sprite CloseIcon;
}
