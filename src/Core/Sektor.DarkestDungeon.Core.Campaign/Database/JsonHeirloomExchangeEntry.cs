namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>
    /// A single heirloom exchange rate as loaded from the content file.
    /// </summary>
    public class JsonHeirloomExchangeEntry
    {
        /// <summary>Gets the currency type being exchanged.</summary>
        public string exchange_from_type { get; set; }

        /// <summary>Gets the amount of the source currency paid for the exchange.</summary>
        public int exchange_from_amount { get; set; }

        /// <summary>Gets the currency type received in exchange.</summary>
        public string exchange_to_type { get; set; }

        /// <summary>Gets the amount of the target currency received in exchange.</summary>
        public int exchange_to_amount { get; set; }
    }
}
