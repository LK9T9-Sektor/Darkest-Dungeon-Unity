using System.Collections.Generic;

using Newtonsoft.Json;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>
    /// A single party name as loaded from the content file.
    /// </summary>
    public class JsonPartyNameEntry
    {
        /// <summary>Gets the unique identifier of the party name.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Gets the hero classes required for the party name to be available.</summary>
        [JsonProperty("required_hero_class")]
        public List<string> RequiredHeroClass { get; set; }
    }
}
