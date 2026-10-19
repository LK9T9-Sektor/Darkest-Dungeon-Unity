using SkiaSharp;
using Spine;

namespace Sektor.Spine.Skia;

/// <summary>Geometry adapter of one Spine attachment kind (region, mesh or skinned mesh).</summary>
internal abstract class AttachmentGeometry
{
    /// <summary>Gets a value indicating whether this adapter handles the given attachment.</summary>
    /// <param name="attachment">The attachment to test.</param>
    /// <returns><c>true</c> when the attachment can be rendered by this adapter.</returns>
    public abstract bool Matches(Attachment attachment);

    /// <summary>Gets the number of vertices of the attachment.</summary>
    /// <param name="attachment">The attachment.</param>
    /// <returns>The vertex count.</returns>
    public abstract int GetVertexCount(Attachment attachment);

    /// <summary>Fills the world-space vertex positions of the attachment (skeleton units, Y up).</summary>
    /// <param name="slot">The owning slot (carries bone and deform data).</param>
    /// <param name="attachment">The attachment.</param>
    /// <param name="worldVertices">Destination buffer of at least <see cref="GetVertexCount"/> * 2 floats.</param>
    public abstract void ComputeWorldVertices(Slot slot, Attachment attachment, float[] worldVertices);

    /// <summary>Gets the interleaved texture coordinates (u, v per vertex).</summary>
    /// <param name="attachment">The attachment.</param>
    /// <returns>The UV array.</returns>
    public abstract float[] GetUvs(Attachment attachment);

    /// <summary>Gets the triangle indices.</summary>
    /// <param name="attachment">The attachment.</param>
    /// <returns>The index array.</returns>
    public abstract int[] GetTriangles(Attachment attachment);

    /// <summary>Gets the attachment tint.</summary>
    /// <param name="attachment">The attachment.</param>
    /// <returns>The tint.</returns>
    public abstract AttachmentTint GetTint(Attachment attachment);

    /// <summary>Gets the atlas page texture of the attachment.</summary>
    /// <param name="attachment">The attachment.</param>
    /// <returns>The bitmap, or <c>null</c> when the attachment has no atlas region.</returns>
    public SKBitmap? GetTexture(Attachment attachment)
    {
        var region = GetRendererObject(attachment) as AtlasRegion;
        return region?.page?.rendererObject as SKBitmap;
    }

    /// <summary>Gets the attachment's renderer object (atlas region).</summary>
    /// <param name="attachment">The attachment.</param>
    /// <returns>The renderer object.</returns>
    protected abstract object? GetRendererObject(Attachment attachment);
}
