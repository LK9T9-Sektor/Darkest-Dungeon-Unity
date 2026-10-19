using System;
using System.Collections.Generic;

/// <summary>
/// One component of a captured node: concrete type, enabled state and the diagnostic properties
/// extracted by <see cref="SceneSnapshotPropertyReader"/>.
/// </summary>
[Serializable]
public class ComponentSnapshot
{
    /// <summary>The component type full name ("&lt;MissingScript&gt;" for a null component).</summary>
    public string Type;

    /// <summary>Whether the component is enabled (true for non-behaviour components).</summary>
    public bool Enabled = true;

    /// <summary>The extracted properties.</summary>
    public List<ComponentPropertySnapshot> Properties = new List<ComponentPropertySnapshot>();
}
