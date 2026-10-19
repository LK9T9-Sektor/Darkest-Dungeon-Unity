using Spine;

namespace Sektor.DarkestDungeon.Spine.Wpf;

/// <summary>Geometry adapter for <see cref="MeshAttachment"/>.</summary>
internal sealed class MeshAttachmentGeometry : AttachmentGeometry
{
    /// <inheritdoc/>
    public override bool Matches(Attachment attachment) => attachment is MeshAttachment;

    /// <inheritdoc/>
    public override int GetVertexCount(Attachment attachment) => ((MeshAttachment)attachment).UVs.Length / 2;

    /// <inheritdoc/>
    public override void ComputeWorldVertices(Slot slot, Attachment attachment, float[] worldVertices)
        => ((MeshAttachment)attachment).ComputeWorldVertices(slot, worldVertices);

    /// <inheritdoc/>
    public override float[] GetUvs(Attachment attachment) => ((MeshAttachment)attachment).UVs;

    /// <inheritdoc/>
    public override int[] GetTriangles(Attachment attachment) => ((MeshAttachment)attachment).Triangles;

    /// <inheritdoc/>
    public override AttachmentTint GetTint(Attachment attachment)
    {
        var mesh = (MeshAttachment)attachment;
        return new AttachmentTint(mesh.R, mesh.G, mesh.B, mesh.A);
    }

    /// <inheritdoc/>
    protected override object? GetRendererObject(Attachment attachment) => ((MeshAttachment)attachment).RendererObject;
}
