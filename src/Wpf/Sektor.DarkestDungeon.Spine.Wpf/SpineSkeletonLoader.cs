using SkiaSharp;
using Spine;

namespace Sektor.DarkestDungeon.Spine.Wpf;

/// <summary>Loads Spine 2.1 skeleton data (binary or JSON) plus its atlas and page textures.</summary>
public sealed class SpineSkeletonLoader
{
    /// <summary>Loads skeleton data from a binary skeleton file and an atlas file.</summary>
    /// <param name="skeletonFile">Path to the skeleton file (<c>.skel</c>, <c>.skel.bytes</c> or <c>.json</c>).</param>
    /// <param name="atlasFile">Path to the atlas file; page textures are resolved next to it by the names inside the atlas.</param>
    /// <returns>The loaded content; the caller owns and must dispose it.</returns>
    public SpineContent Load(string skeletonFile, string atlasFile)
    {
        if (skeletonFile == null)
            throw new ArgumentNullException(nameof(skeletonFile));
        if (atlasFile == null)
            throw new ArgumentNullException(nameof(atlasFile));
        if (!File.Exists(skeletonFile))
            throw new FileNotFoundException($"Skeleton file not found: '{skeletonFile}'.", skeletonFile);
        if (!File.Exists(atlasFile))
            throw new FileNotFoundException($"Atlas file not found: '{atlasFile}'.", atlasFile);

        string directory = Path.GetDirectoryName(Path.GetFullPath(atlasFile)) ?? string.Empty;
        var textures = new List<SKBitmap>();
        try
        {
            var textureLoader = new SkiaTextureLoader(textures);
            var atlas = new Atlas(new StringReader(File.ReadAllText(atlasFile)), directory, textureLoader);
            SkeletonData data = ReadSkeletonData(skeletonFile, atlas);
            return new SpineContent(data, textures);
        }
        catch
        {
            foreach (SKBitmap texture in textures)
                texture.Dispose();
            throw;
        }
    }

    private static SkeletonData ReadSkeletonData(string skeletonFile, Atlas atlas)
    {
        var attachmentLoader = new AtlasAttachmentLoader(atlas);
        using var stream = File.OpenRead(skeletonFile);
        if (Path.GetExtension(skeletonFile).Equals(".json", StringComparison.OrdinalIgnoreCase))
            return new SkeletonJson(attachmentLoader).ReadSkeletonData(new StreamReader(stream));

        return new SkeletonBinary(attachmentLoader).ReadSkeletonData(stream);
    }
}
