using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Content.Campaign;

namespace Sektor.DarkestDungeon.Core.Content.Database
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

            for (int i = 0; i < jsonPartyNames.PartyNames.Count; i++)
            {
                var entry = jsonPartyNames.PartyNames[i];
                var partyName = new PartyNameEntry
                {
                    Id = entry.Id,
                    ClassIds = entry.RequiredHeroClass
                };
                partyNames.Add(partyName);
            }

            return partyNames;
        }
    }
}
