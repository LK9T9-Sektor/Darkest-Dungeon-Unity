using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Content.Character;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Maps raw quirk content entries into domain quirks.</summary>
    public static class QuirkMapper
    {
        /// <summary>Converts the raw quirk entries into domain quirks.</summary>
        /// <param name="jsonQuirks">The raw quirk entries.</param>
        /// <returns>The domain quirks.</returns>
        public static List<Quirk> Parse(List<JsonQuirk> jsonQuirks)
        {
            var quirks = new List<Quirk>();
            if (jsonQuirks == null)
                return quirks;

            foreach (var json in jsonQuirks)
            {
                var quirk = new Quirk
                {
                    Id = json.id,
                    Classification = json.classification,
                    ShowExplicitDescription = json.show_explicit_description,
                    IsPositive = json.is_positive,
                    IsDisease = json.is_disease,
                    KeepLoot = json.keep_loot,
                    CurioTag = json.curio_tag,
                    CurioTagChance = json.curio_tag_chance,
                };
                if (json.incompatible_quirks != null)
                    quirk.IncompatibleQuirks.AddRange(json.incompatible_quirks);
                if (json.buffs != null)
                    quirk.Buffs.AddRange(json.buffs);
                quirks.Add(quirk);
            }

            return quirks;
        }
    }
}