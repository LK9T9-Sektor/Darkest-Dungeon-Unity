using System.Collections.Generic;
using System.Linq;

/// <summary>TEST menu source for diseases: lists quirk ids flagged as diseases.</summary>
public static class TestDiseaseSource
{
    /// <summary>Creates the disease browse source.</summary>
    /// <returns>The registered entity source.</returns>
    public static TestEntitySource Create()
    {
        return new TestEntitySource("Diseases", ListEntries, ShowDetail);
    }

    private static List<string> ListEntries()
    {
        var data = DarkestDungeonManager.Data;
        if (data.Quirks != null)
            return data.Quirks.Values.Where(q => q.IsDisease).Select(q => q.Id).OrderBy(id => id).ToList();
        return new List<string>();
    }

    private static void ShowDetail(string entry, TestDetailView view)
    {
        view.ShowText("Diseases: " + entry);
    }
}
