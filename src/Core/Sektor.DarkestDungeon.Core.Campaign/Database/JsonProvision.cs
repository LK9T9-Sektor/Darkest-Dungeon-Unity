using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>Root document of Data\Mechanics\Provision.json.</summary>
    public class JsonProvision
    {
        /// <summary>Gets or sets the default inventory lists per raid length.</summary>
        public List<List<JsonInventoryItem>> raid_starting_length_inventory_item_lists { get; set; }

        /// <summary>Gets or sets the hero-class specific starting inventories.</summary>
        public List<JsonHeroClassInventory> raid_starting_hero_class_item_lists { get; set; }

        /// <summary>Gets or sets the default store inventories by stage coach level.</summary>
        public List<List<JsonInventoryItem>> default_store_inventory_item_lists { get; set; }
    }
}