using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>Per-hero-class starting inventory lists.</summary>
    public class JsonHeroClassInventory
    {
        /// <summary>Gets or sets the hero class id.</summary>
        public string hero_class { get; set; }

        /// <summary>Gets or sets the class-specific starting inventory.</summary>
        public List<JsonInventoryItem> item_lists { get; set; }
    }
}