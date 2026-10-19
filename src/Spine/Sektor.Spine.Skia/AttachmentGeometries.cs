using Spine;

namespace Sektor.Spine.Skia;

/// <summary>Registry resolving the geometry adapter of a Spine attachment.</summary>
internal sealed class AttachmentGeometries
{
    private readonly AttachmentGeometry[] geometries;

    /// <summary>Initializes a new instance of the <see cref="AttachmentGeometries"/> class.</summary>
    /// <param name="geometries">The known attachment adapters.</param>
    public AttachmentGeometries(AttachmentGeometry[] geometries)
    {
        this.geometries = geometries ?? throw new ArgumentNullException(nameof(geometries));
    }

    /// <summary>Creates the registry with all supported attachment kinds.</summary>
    /// <returns>The registry.</returns>
    public static AttachmentGeometries CreateDefault() => new AttachmentGeometries(new AttachmentGeometry[]
    {
        new RegionAttachmentGeometry(),
        new MeshAttachmentGeometry(),
        new SkinnedMeshAttachmentGeometry(),
    });

    /// <summary>Finds the adapter of the given attachment.</summary>
    /// <param name="attachment">The attachment.</param>
    /// <returns>The adapter, or <c>null</c> when the kind is not renderable.</returns>
    public AttachmentGeometry? Find(Attachment attachment)
    {
        foreach (AttachmentGeometry geometry in geometries)
        {
            if (geometry.Matches(attachment))
                return geometry;
        }

        return null;
    }
}
