using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>
    /// A single party name as loaded from the content file.
    /// </summary>
    public class JsonPartyNameEntry
    {
        /// <summary>Gets the unique identifier of the party name.</summary>
        public string id { get; set; }

        /// <summary>Gets the hero classes required for the party name to be available.</summary>
        public List<string> required_hero_class { get; set; }
    }
}
