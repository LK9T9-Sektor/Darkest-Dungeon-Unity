using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Content.Campaign;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>
    /// Maps raw heirloom exchange content into domain exchange rates.
    /// </summary>
    public static class HeirloomExchangeMapper
    {
        /// <summary>
        /// Converts the raw exchange data into a list of <see cref="HeirloomExchange"/> entries.
        /// </summary>
        /// <param name="jsonExchange">The raw exchange data loaded from the content file.</param>
        /// <returns>The domain exchange rates of the first market.</returns>
        public static List<HeirloomExchange> Parse(JsonHeirloomExchange jsonExchange)
        {
            var exchanges = new List<HeirloomExchange>();

            for (int i = 0; i < jsonExchange.markets[0].exchange_rates.Count; i++)
            {
                var entry = jsonExchange.markets[0].exchange_rates[i];
                var exchange = new HeirloomExchange
                {
                    FromType = entry.exchange_from_type,
                    FromAmount = entry.exchange_from_amount,
                    ToType = entry.exchange_to_type,
                    ToAmount = entry.exchange_to_amount
                };
                exchanges.Add(exchange);
            }

            return exchanges;
        }
    }
}
