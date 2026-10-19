using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>One node of the flat scene hierarchy (parent id instead of nesting).</summary>
    /// <param name="Id">The node id.</param>
    /// <param name="ParentId">The parent node id, or <c>null</c> for a root node.</param>
    /// <param name="Components">The logical component names on the node.</param>
    public sealed record SceneNode(string Id, string? ParentId, IReadOnlyList<string> Components);
}
