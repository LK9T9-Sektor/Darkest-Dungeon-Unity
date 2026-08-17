using System;
using System.Collections.Generic;

/// <summary>A named content category that the TEST menu can browse.</summary>
public class TestEntitySource
{
    /// <summary>Gets the display name of the category.</summary>
    public string Category { get; private set; }

    /// <summary>Gets the entry id list of the category.</summary>
    public Func<List<string>> ListEntries { get; private set; }

    /// <summary>Gets the detail action for a selected entry.</summary>
    public Action<string, TestDetailView> ShowDetail { get; private set; }

    /// <summary>Initializes a new instance of the <see cref="TestEntitySource"/> class.</summary>
    /// <param name="category">The display name of the category.</param>
    /// <param name="listEntries">The entry id list of the category.</param>
    /// <param name="showDetail">The detail action for a selected entry.</param>
    public TestEntitySource(string category, Func<List<string>> listEntries, Action<string, TestDetailView> showDetail)
    {
        Category = category;
        ListEntries = listEntries;
        ShowDetail = showDetail;
    }
}
