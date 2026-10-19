using SkiaSharp;
using Spine;

namespace Sektor.DarkestDungeon.Spine.Wpf;

/// <summary>Atlas texture loader decoding pages into <see cref="SKBitmap"/> instances.</summary>
internal sealed class SkiaTextureLoader : TextureLoader
{
    private readonly List<SKBitmap> textures;

    /// <summary>Initializes a new instance of the <see cref="SkiaTextureLoader"/> class.</summary>
    /// <param name="textures">The list collecting loaded bitmaps (owned by the content).</param>
    public SkiaTextureLoader(List<SKBitmap> textures)
    {
        this.textures = textures ?? throw new ArgumentNullException(nameof(textures));
    }

    /// <inheritdoc/>
    public void Load(AtlasPage page, string path)
    {
        SKBitmap bitmap = SKBitmap.Decode(path);
        if (bitmap == null)
            throw new InvalidDataException($"Failed to decode atlas texture '{path}'.");

        page.rendererObject = bitmap;
        page.width = bitmap.Width;
        page.height = bitmap.Height;
        textures.Add(bitmap);
    }

    /// <inheritdoc/>
    public void Unload(object texture)
    {
        (texture as SKBitmap)?.Dispose();
    }
}
