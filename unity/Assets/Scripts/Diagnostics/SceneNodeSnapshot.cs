using System;
using System.Collections.Generic;

/// <summary>
/// One node of the captured scene hierarchy (always a GameObject): its identity, active chain,
/// components and children. Inactive objects are captured as well.
/// </summary>
[Serializable]
public class SceneNodeSnapshot
{
    /// <summary>The GameObject name.</summary>
    public string Name;

    /// <summary>The hierarchy path from the scene root ("/Root/Child").</summary>
    public string Path;

    /// <summary>The index among the parent's children.</summary>
    public int SiblingIndex;

    /// <summary>The GameObject's own active flag.</summary>
    public bool ActiveSelf;

    /// <summary>Whether the GameObject is active through the whole ancestor chain.</summary>
    public bool ActiveInHierarchy;

    /// <summary>The tag.</summary>
    public string Tag;

    /// <summary>The layer index.</summary>
    public int Layer;

    /// <summary>The layer name (empty for unnamed layers).</summary>
    public string LayerName;

    /// <summary>The components of this GameObject in inspector order.</summary>
    public List<ComponentSnapshot> Components = new List<ComponentSnapshot>();

    /// <summary>The child nodes in sibling order.</summary>
    public List<SceneNodeSnapshot> Children = new List<SceneNodeSnapshot>();
}
