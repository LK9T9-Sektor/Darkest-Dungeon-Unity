namespace Sektor.DarkestDungeon.Core.Campaign
{
    /// <summary>
    /// Represents a single heirloom exchange rate between two currency types.
    /// </summary>
    public class HeirloomExchange
    {
        /// <summary>Gets the currency type being exchanged.</summary>
        public string FromType { get; set; }

        /// <summary>Gets the amount of the source currency paid for the exchange.</summary>
        public int FromAmount { get; set; }

        /// <summary>Gets the currency type received in exchange.</summary>
        public string ToType { get; set; }

        /// <summary>Gets the amount of the target currency received in exchange.</summary>
        public int ToAmount { get; set; }
    }
}
