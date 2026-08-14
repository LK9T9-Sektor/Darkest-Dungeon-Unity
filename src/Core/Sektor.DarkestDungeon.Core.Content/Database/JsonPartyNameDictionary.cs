using System.Collections.Generic;

using Newtonsoft.Json;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>
    /// Raw party name data as loaded from the content file.
    /// </summary>
    public class JsonPartyNameDictionary
    {
        /// <summary>Gets the available party names.</summary>
        [JsonProperty("party_names")]
        public List<JsonPartyNameEntry> PartyNames { get; set; }
    }
}
