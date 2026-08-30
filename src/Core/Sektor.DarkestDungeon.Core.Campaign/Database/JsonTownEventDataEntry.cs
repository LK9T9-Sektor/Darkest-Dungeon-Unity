namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>A single town event effect instruction.</summary>
    public class JsonTownEventDataEntry
    {
        /// <summary>Gets or sets the effect type (embark_party_buff...).</summary>
        public string type { get; set; }

        /// <summary>Gets or sets the string parameter.</summary>
        public string string_data { get; set; }

        /// <summary>Gets or sets the numeric parameter.</summary>
        public double number_data { get; set; }
    }
}