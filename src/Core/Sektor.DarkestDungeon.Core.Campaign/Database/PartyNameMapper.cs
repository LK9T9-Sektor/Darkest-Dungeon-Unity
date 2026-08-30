using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Campaign;

namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>
    /// Maps raw party name content into domain party name entries.
    /// </summary>
    public static class PartyNameMapper
    {
        /// <summary>
        /// Converts the raw party name data into a list of <see cref="PartyNameEntry"/> entries.
        /// </summary>
        /// <param name="jsonPartyNames">The raw party name data loaded from the content file.</param>
        /// <returns>The domain party name entries.</returns>
        public static List<PartyNameEntry> Parse(JsonPartyNameDictionary jsonPartyNames)
        {
            var partyNames = new List<PartyNameEntry>();

            for (int i = 0; i < jsonPartyNames.party_names.Count; i++)
            {
                var entry = jsonPartyNames.party_names[i];
                var partyName = new PartyNameEntry
                {
                    Id = entry.id,
                    ClassIds = entry.required_hero_class
                };
                partyNames.Add(partyName);
            }

            return partyNames;
        }
    }
}
