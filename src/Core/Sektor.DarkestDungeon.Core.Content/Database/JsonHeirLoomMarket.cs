using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>
    /// A named group of heirloom exchange rates as loaded from the content file.
    /// </summary>
    public class JsonHeirLoomMarket
    {
        /// <summary>Gets the identifier of the market.</summary>
        public string id { get; set; }

        /// <summary>Gets the exchange rates available in this market.</summary>
        public List<JsonHeirloomExchangeEntry> exchange_rates { get; set; }
    }
}
