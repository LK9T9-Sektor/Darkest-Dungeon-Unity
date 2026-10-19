using Spine;

namespace Sektor.DarkestDungeon.Spine.Wpf;

/// <summary>Geometry adapter for <see cref="RegionAttachment"/>.</summary>
internal sealed class RegionAttachmentGeometry : AttachmentGeometry
{
    private static readonly int[] QuadTriangles = { 0, 1, 2, 2, 3, 0 };

    /// <inheritdoc/>
    public override bool Matches(Attachment attachment) => attachment is RegionAttachment;

    /// <inheritdoc/>
    public override int GetVertexCount(Attachment attachment) => 4;

    /// <inheritdoc/>
    public override void ComputeWorldVertices(Slot slot, Attachment attachment, float[] worldVertices)
        => ((RegionAttachment)attachment).ComputeWorldVertices(slot.Bone, worldVertices);

    /// <inheritdoc/>
    public override float[] GetUvs(Attachment attachment) => ((RegionAttachment)attachment).UVs;

    /// <inheritdoc/>
    public override int[] GetTriangles(Attachment attachment) => QuadTriangles;

    /// <inheritdoc/>
    public override AttachmentTint GetTint(Attachment attachment)
    {
        var region = (RegionAttachment)attachment;
        return new AttachmentTint(region.R, region.G, region.B, region.A);
    }

    /// <inheritdoc/>
    protected override object? GetRendererObject(Attachment attachment) => ((RegionAttachment)attachment).RendererObject;
}
