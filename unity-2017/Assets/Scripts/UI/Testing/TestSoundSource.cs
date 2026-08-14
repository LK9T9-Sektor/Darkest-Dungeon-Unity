using System.Collections.Generic;
using System.Linq;

/// <summary>TEST menu source for narration sounds: lists FMOD event paths and plays the selection.</summary>
public static class TestSoundSource
{
    /// <summary>Creates the sound browse source.</summary>
    /// <returns>The registered entity source.</returns>
    public static TestEntitySource Create()
    {
        return new TestEntitySource("Sounds", ListEntries, ShowDetail);
    }

    private static List<string> ListEntries()
    {
        var data = DarkestDungeonManager.Data;
        if (data.Narration != null)
            return data.Narration.Values.SelectMany(e => e.AudioEvents)
                .Select(a => a.AudioEvent).Where(p => !string.IsNullOrEmpty(p))
                .Distinct().OrderBy(p => p).ToList();
        return new List<string>();
    }

    private static void ShowDetail(string entry, TestDetailView view)
    {
        view.PlaySound(entry);
    }
}
