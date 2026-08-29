namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>A currency cost with a resource type and amount.</summary>
    public class JsonCurrencyCost
    {
        /// <summary>Gets or sets the currency type (gold, deed, reliefs...).</summary>
        public string type { get; set; }

        /// <summary>Gets or sets the cost amount.</summary>
        public int amount { get; set; }
    }
}