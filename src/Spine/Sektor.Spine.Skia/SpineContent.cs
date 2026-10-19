using SkiaSharp;
using Spine;

namespace Sektor.Spine.Skia;

/// <summary>Loaded Spine content: skeleton data plus the atlas textures it references.</summary>
public sealed class SpineContent : IDisposable
{
    private readonly IReadOnlyList<SKBitmap> textures;
    private bool disposed;

    /// <summary>Initializes a new instance of the <see cref="SpineContent"/> class.</summary>
    /// <param name="skeletonData">The parsed skeleton data.</param>
    /// <param name="textures">The atlas page textures.</param>
    public SpineContent(SkeletonData skeletonData, IReadOnlyList<SKBitmap> textures)
    {
        SkeletonData = skeletonData ?? throw new ArgumentNullException(nameof(skeletonData));
        this.textures = textures ?? throw new ArgumentNullException(nameof(textures));
    }

    /// <summary>Gets the parsed skeleton data.</summary>
    public SkeletonData SkeletonData { get; }

    /// <summary>Gets the name of the first animation, or an empty string when the skeleton has none.</summary>
    public string DefaultAnimationName => SkeletonData.Animations.Count > 0 ? SkeletonData.Animations.Items[0].Name : string.Empty;

    /// <summary>Creates a player instance over this content.</summary>
    /// <returns>The player.</returns>
    public SpinePlayer CreatePlayer() => new SpinePlayer(SkeletonData);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        foreach (SKBitmap texture in textures)
            texture.Dispose();
    }
}
