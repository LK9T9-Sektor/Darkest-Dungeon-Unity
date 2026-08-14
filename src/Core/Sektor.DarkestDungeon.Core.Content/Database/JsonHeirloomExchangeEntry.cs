using Newtonsoft.Json;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>
    /// A single heirloom exchange rate as loaded from the content file.
    /// </summary>
    public class JsonHeirloomExchangeEntry
    {
        /// <summary>Gets the currency type being exchanged.</summary>
        [JsonProperty("exchange_from_type")]
        public string ExchangeFromType { get; set; }

        /// <summary>Gets the amount of the source currency paid for the exchange.</summary>
        [JsonProperty("exchange_from_amount")]
        public int ExchangeFromAmount { get; set; }

        /// <summary>Gets the currency type received in exchange.</summary>
        [JsonProperty("exchange_to_type")]
        public string ExchangeToType { get; set; }

        /// <summary>Gets the amount of the target currency received in exchange.</summary>
        [JsonProperty("exchange_to_amount")]
        public int ExchangeToAmount { get; set; }
    }
}
