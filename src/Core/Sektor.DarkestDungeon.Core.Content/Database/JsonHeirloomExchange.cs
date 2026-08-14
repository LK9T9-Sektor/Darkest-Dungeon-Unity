using System.Collections.Generic;

using Newtonsoft.Json;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>
    /// Raw heirloom exchange data as loaded from the content file.
    /// </summary>
    public class JsonHeirloomExchange
    {
        /// <summary>Gets the available exchange markets.</summary>
        [JsonProperty("markets")]
        public List<JsonHeirLoomMarket> Markets { get; set; }
    }
}
