using System.Collections.Generic;

using Newtonsoft.Json;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>
    /// A named group of heirloom exchange rates as loaded from the content file.
    /// </summary>
    public class JsonHeirLoomMarket
    {
        /// <summary>Gets the identifier of the market.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Gets the exchange rates available in this market.</summary>
        [JsonProperty("exchange_rates")]
        public List<JsonHeirloomExchangeEntry> ExchangeRates { get; set; }
    }
}
