using Spine;

namespace Sektor.DarkestDungeon.Spine.Wpf;

/// <summary>Geometry adapter for <see cref="SkinnedMeshAttachment"/>.</summary>
internal sealed class SkinnedMeshAttachmentGeometry : AttachmentGeometry
{
    /// <inheritdoc/>
    public override bool Matches(Attachment attachment) => attachment is SkinnedMeshAttachment;

    /// <inheritdoc/>
    public override int GetVertexCount(Attachment attachment) => ((SkinnedMeshAttachment)attachment).UVs.Length / 2;

    /// <inheritdoc/>
    public override void ComputeWorldVertices(Slot slot, Attachment attachment, float[] worldVertices)
        => ((SkinnedMeshAttachment)attachment).ComputeWorldVertices(slot, worldVertices);

    /// <inheritdoc/>
    public override float[] GetUvs(Attachment attachment) => ((SkinnedMeshAttachment)attachment).UVs;

    /// <inheritdoc/>
    public override int[] GetTriangles(Attachment attachment) => ((SkinnedMeshAttachment)attachment).Triangles;

    /// <inheritdoc/>
    public override AttachmentTint GetTint(Attachment attachment)
    {
        var mesh = (SkinnedMeshAttachment)attachment;
        return new AttachmentTint(mesh.R, mesh.G, mesh.B, mesh.A);
    }

    /// <inheritdoc/>
    protected override object? GetRendererObject(Attachment attachment) => ((SkinnedMeshAttachment)attachment).RendererObject;
}
