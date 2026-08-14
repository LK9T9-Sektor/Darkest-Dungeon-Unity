using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Raw loot data as loaded from the content file.</summary>
    public class JsonLootDatabase
    {
        /// <summary>Gets the darkness bonus sets.</summary>
        public List<JsonDarknessBonusSet> darkness_bonuses { get; set; }

        /// <summary>Gets the loot tables.</summary>
        public List<JsonLootTable> loot_tables { get; set; }
    }

    /// <summary>A raw darkness bonus set as loaded from the content file.</summary>
    public class JsonDarknessBonusSet
    {
        /// <summary>Gets the bonus type.</summary>
        public string type { get; set; }

        /// <summary>Gets the darkness bonuses.</summary>
        public List<JsonDarknessBonus> bonuses { get; set; }
    }

    /// <summary>A raw darkness bonus as loaded from the content file.</summary>
    public class JsonDarknessBonus
    {
        /// <summary>Gets the darkness level threshold.</summary>
        public int darkness { get; set; }

        /// <summary>Gets the chance of the bonus.</summary>
        public float chance { get; set; }

        /// <summary>Gets the loot codes.</summary>
        public List<string> codes { get; set; }
    }

    /// <summary>A raw loot table as loaded from the content file.</summary>
    public class JsonLootTable
    {
        /// <summary>Gets the identifier of the loot table.</summary>
        public string id { get; set; }

        /// <summary>Gets the dungeon difficulty.</summary>
        public int difficulty { get; set; }

        /// <summary>Gets the dungeon.</summary>
        public string dungeon { get; set; }

        /// <summary>Gets the loot entries.</summary>
        public List<JsonLootEntry> entries { get; set; }
    }

    /// <summary>A raw loot entry as loaded from the content file.</summary>
    public class JsonLootEntry
    {
        /// <summary>Gets the entry type.</summary>
        public string type { get; set; }

        /// <summary>Gets the selection chance.</summary>
        public float chances { get; set; }

        /// <summary>Gets the entry specific data keyed by field name.</summary>
        public Dictionary<string, object> data { get; set; }
    }
}
