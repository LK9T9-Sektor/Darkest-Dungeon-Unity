using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Content.Raid;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>The kind of loot a loot entry grants.</summary>
    public enum LootType
    {
        /// <summary>No loot.</summary>
        Nothing,
        /// <summary>A reference to another loot table.</summary>
        Table,
        /// <summary>A concrete item.</summary>
        Item,
        /// <summary>A trinket.</summary>
        Trinket,
        /// <summary>A journal page.</summary>
        Journal,
    }

    /// <summary>The raid loot data: darkness bonuses and loot tables.</summary>
    public class LootDatabase
    {
        /// <summary>Gets the darkness loot bonuses keyed by bonus type.</summary>
        public Dictionary<string, List<DarknessBonus>> DarknessLoot { get; set; }

        /// <summary>Gets the loot tables keyed by table id.</summary>
        public Dictionary<string, List<LootTable>> LootTables { get; set; }

        /// <summary>Initializes a new instance of the <see cref="LootDatabase"/> class.</summary>
        public LootDatabase()
        {
            DarknessLoot = new Dictionary<string, List<DarknessBonus>>();
            LootTables = new Dictionary<string, List<LootTable>>();
        }
    }

    /// <summary>A darkness-level based loot bonus.</summary>
    public class DarknessBonus
    {
        /// <summary>Gets or sets the darkness level threshold.</summary>
        public int DarknessLevel { get; set; }

        /// <summary>Gets or sets the chance of the bonus.</summary>
        public float Chance { get; set; }

        /// <summary>Gets or sets the loot codes of the bonus.</summary>
        public List<string> Codes { get; set; }
    }

    /// <summary>A weighted loot table for a dungeon and difficulty.</summary>
    public class LootTable
    {
        /// <summary>Gets or sets the identifier of the loot table.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the dungeon difficulty.</summary>
        public int Difficulty { get; set; }

        /// <summary>Gets or sets the dungeon.</summary>
        public string Dungeon { get; set; }

        /// <summary>Gets the loot entries of the table.</summary>
        public List<LootEntry> Entries { get; set; }

        /// <summary>Initializes a new instance of the <see cref="LootTable"/> class.</summary>
        public LootTable()
        {
            Entries = new List<LootEntry>();
        }
    }

    /// <summary>A single weighted loot entry.</summary>
    public class LootEntry : ISingleProportion
    {
        /// <summary>Gets the kind of loot the entry grants.</summary>
        public LootType Type { get; private set; }

        /// <summary>Gets or sets the selection chance of the entry.</summary>
        public float Chance { get; set; }

        /// <summary>Initializes a new instance of the <see cref="LootEntry"/> class.</summary>
        /// <param name="type">The kind of loot the entry grants.</param>
        public LootEntry(LootType type = LootType.Nothing)
        {
            Type = type;
        }
    }

    /// <summary>A loot entry referencing another loot table.</summary>
    public class LootEntryTable : LootEntry
    {
        /// <summary>Gets or sets the referenced table id.</summary>
        public string TableId { get; set; }

        /// <summary>Initializes a new instance of the <see cref="LootEntryTable"/> class.</summary>
        public LootEntryTable()
            : base(LootType.Table)
        {
        }
    }

    /// <summary>A loot entry granting a concrete item.</summary>
    public class LootEntryItem : LootEntry
    {
        /// <summary>Gets or sets the item type.</summary>
        public string ItemType { get; set; }

        /// <summary>Gets or sets the item id.</summary>
        public string ItemId { get; set; }

        /// <summary>Gets or sets the item amount.</summary>
        public int ItemAmount { get; set; }

        /// <summary>Initializes a new instance of the <see cref="LootEntryItem"/> class.</summary>
        public LootEntryItem()
            : base(LootType.Item)
        {
        }
    }

    /// <summary>A loot entry granting a journal page.</summary>
    public class LootEntryJournal : LootEntry
    {
        /// <summary>Gets or sets the minimum journal page index.</summary>
        public int MinIndex { get; set; }

        /// <summary>Gets or sets the maximum journal page index.</summary>
        public int MaxIndex { get; set; }

        /// <summary>Gets or sets a specific journal page index.</summary>
        public int? SpecificId { get; set; }

        /// <summary>Initializes a new instance of the <see cref="LootEntryJournal"/> class.</summary>
        public LootEntryJournal()
            : base(LootType.Journal)
        {
        }
    }

    /// <summary>A loot entry granting a trinket.</summary>
    public class LootEntryTrinket : LootEntry
    {
        /// <summary>Gets or sets the trinket rarity.</summary>
        public string Rarity { get; set; }

        /// <summary>Initializes a new instance of the <see cref="LootEntryTrinket"/> class.</summary>
        public LootEntryTrinket()
            : base(LootType.Trinket)
        {
        }
    }
}
