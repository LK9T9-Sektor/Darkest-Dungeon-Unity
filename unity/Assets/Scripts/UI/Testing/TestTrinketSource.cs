using System.Collections.Generic;
using System.Linq;

/// <summary>TEST menu source for trinkets: lists trinket ids and shows their icon + name.</summary>
public static class TestTrinketSource
{
    /// <summary>Creates the trinket browse source.</summary>
    /// <returns>The registered entity source.</returns>
    public static TestEntitySource Create()
    {
        return new TestEntitySource("Trinkets", ListEntries, ShowDetail);
    }

    private static List<string> ListEntries()
    {
        var data = DarkestDungeonManager.Data;
        if (data.Items != null && data.Items.ContainsKey("trinket"))
            return data.Items["trinket"].Keys.OrderBy(id => id).ToList();
        return new List<string>();
    }

    private static void ShowDetail(string entry, TestDetailView view)
    {
        string path = "Sprites/Shared/Inventory/Trinket/inv_trinket+" + entry;
        view.ShowImage(path);
        view.ShowText("Trinkets: " + entry + "\nfile: " + path);
    }
}
