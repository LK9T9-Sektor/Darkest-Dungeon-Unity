using System;

/// <summary>
/// A single named diagnostic value of a component. Values are always strings formatted with the
/// invariant culture so JSON dumps do not depend on the editor locale.
/// </summary>
[Serializable]
public class ComponentPropertySnapshot
{
    /// <summary>The property name.</summary>
    public string Name;

    /// <summary>The formatted property value.</summary>
    public string Value;
}
