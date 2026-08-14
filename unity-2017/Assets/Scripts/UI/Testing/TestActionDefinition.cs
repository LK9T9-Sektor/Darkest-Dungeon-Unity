using System;

/// <summary>A named test action producing a log line.</summary>
public class TestActionDefinition
{
    /// <summary>Gets the display name of the action.</summary>
    public string Name { get; private set; }

    /// <summary>Gets the action that produces the result log line.</summary>
    public Func<string> Run { get; private set; }

    /// <summary>Initializes a new instance of the <see cref="TestActionDefinition"/> class.</summary>
    /// <param name="name">The display name of the action.</param>
    /// <param name="run">The action producing the result log line.</param>
    public TestActionDefinition(string name, Func<string> run)
    {
        Name = name;
        Run = run;
    }
}
