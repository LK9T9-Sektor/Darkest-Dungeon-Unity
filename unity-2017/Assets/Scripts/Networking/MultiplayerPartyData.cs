using System.Text;
using UnityEngine;

/// <summary>
/// Plain party composition DTO exchanged between Steam session participants.
/// Carries the four selected heroes (class id, name, generation seed and the
/// selected combat skill flags) mirroring the Photon custom properties used by
/// the legacy multiplayer path. Exempt from the rich model rule: pure data shift.
/// </summary>
public class MultiplayerPartyData
{
    private const int HeroCount = 4;
    private const char FieldSeparator = '|';

    /// <summary>Gets the hero class ids (one per party slot).</summary>
    public string[] ClassIds { get; private set; }

    /// <summary>Gets the hero names (one per party slot).</summary>
    public string[] Names { get; private set; }

    /// <summary>Gets the hero generation seeds (one per party slot).</summary>
    public int[] Seeds { get; private set; }

    /// <summary>Gets the selected combat skill flags (one per party slot).</summary>
    public int[] SkillFlags { get; private set; }

    private MultiplayerPartyData(string[] classIds, string[] names, int[] seeds, int[] skillFlags)
    {
        ClassIds = classIds;
        Names = names;
        Seeds = seeds;
        SkillFlags = skillFlags;
    }

    /// <summary>
    /// Captures the current party composition from the multiplayer lobby panel.
    /// Returns null when the panel or the shared hero pool is not available.
    /// </summary>
    public static MultiplayerPartyData CaptureFromPanel()
    {
        MultiplayerPartyPanel panel = UnityEngine.Object.FindObjectOfType<MultiplayerPartyPanel>();
        if (panel == null || DarkestPhotonLauncher.HeroPool == null || DarkestPhotonLauncher.HeroSeeds == null)
            return null;

        if (panel.PartySlots.Count != HeroCount)
            return null;

        string[] classIds = new string[HeroCount];
        string[] names = new string[HeroCount];
        int[] seeds = new int[HeroCount];
        int[] skillFlags = new int[HeroCount];

        for (int i = 0; i < HeroCount; i++)
        {
            var hero = panel.PartySlots[i].SelectedHero;
            classIds[i] = hero.ClassStringId;
            names[i] = hero.Name;
            seeds[i] = DarkestPhotonLauncher.HeroSeeds[DarkestPhotonLauncher.HeroPool.IndexOf(hero)];

            var flags = PlayerSkillFlags.Empty;
            for (int j = 0; j < hero.CurrentCombatSkills.Length; j++)
                if (hero.CurrentCombatSkills[j] != null && hero.SelectedCombatSkills.Contains(hero.CurrentCombatSkills[j]))
                    flags |= (PlayerSkillFlags)Mathf.Pow(2, j + 1);
            skillFlags[i] = (int)flags;
        }

        return new MultiplayerPartyData(classIds, names, seeds, skillFlags);
    }

    /// <summary>Serializes the party into the wire text format.</summary>
    public string Serialize()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < HeroCount; i++)
        {
            if (i > 0)
                builder.Append(FieldSeparator);
            builder.Append(ClassIds[i]).Append(FieldSeparator);
            builder.Append(Names[i]).Append(FieldSeparator);
            builder.Append(Seeds[i]).Append(FieldSeparator);
            builder.Append(SkillFlags[i]);
        }

        return builder.ToString();
    }

    /// <summary>Parses the wire text format back into a party; returns null on malformed input.</summary>
    public static MultiplayerPartyData Deserialize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        string[] fields = text.Split(FieldSeparator);
        if (fields.Length != HeroCount * 4)
            return null;

        string[] classIds = new string[HeroCount];
        string[] names = new string[HeroCount];
        int[] seeds = new int[HeroCount];
        int[] skillFlags = new int[HeroCount];

        for (int i = 0; i < HeroCount; i++)
        {
            int offset = i * 4;
            classIds[i] = fields[offset];
            names[i] = fields[offset + 1];

            int seed;
            if (!int.TryParse(fields[offset + 2], System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out seed))
                return null;

            int flags;
            if (!int.TryParse(fields[offset + 3], System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out flags))
                return null;

            seeds[i] = seed;
            skillFlags[i] = flags;
        }

        return new MultiplayerPartyData(classIds, names, seeds, skillFlags);
    }
}
