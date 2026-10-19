namespace Sektor.Spine.Skia;

/// <summary>Straight (non-premultiplied) tint of a Spine attachment.</summary>
internal readonly struct AttachmentTint
{
    /// <summary>Initializes a new instance of the <see cref="AttachmentTint"/> struct.</summary>
    /// <param name="r">Red component.</param>
    /// <param name="g">Green component.</param>
    /// <param name="b">Blue component.</param>
    /// <param name="a">Alpha component.</param>
    public AttachmentTint(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>Gets the red component.</summary>
    public float R { get; }

    /// <summary>Gets the green component.</summary>
    public float G { get; }

    /// <summary>Gets the blue component.</summary>
    public float B { get; }

    /// <summary>Gets the alpha component.</summary>
    public float A { get; }
}
