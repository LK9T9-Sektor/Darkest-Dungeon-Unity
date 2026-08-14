using System.Collections.Generic;
using System.Linq;

/// <summary>TEST menu source for curios: lists curio ids and shows name + result counts.</summary>
public static class TestCurioSource
{
    /// <summary>Creates the curio browse source.</summary>
    /// <returns>The registered entity source.</returns>
    public static TestEntitySource Create()
    {
        return new TestEntitySource("Curios", ListEntries, ShowDetail);
    }

    private static List<string> ListEntries()
    {
        var data = DarkestDungeonManager.Data;
        if (data.Curios != null)
            return data.Curios.Keys.OrderBy(id => id).ToList();
        return new List<string>();
    }

    private static void ShowDetail(string entry, TestDetailView view)
    {
        var curio = DarkestDungeonManager.Data.Curios[entry];
        view.ShowText("Curios: " + entry + "\nresults=" + curio.Results.Count
            + " itemInteractions=" + curio.ItemInteractions.Count);
    }
}
