namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>An inventory slot of supply/provision items.</summary>
    public class JsonInventoryItem
    {
        /// <summary>Gets or sets the item category (supply, provision...).</summary>
        public string type { get; set; }

        /// <summary>Gets or sets the item id.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets the starting amount.</summary>
        public int amount { get; set; }
    }
}