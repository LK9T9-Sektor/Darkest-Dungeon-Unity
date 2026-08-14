using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Maps raw loot content into domain loot data.</summary>
    public static class LootMapper
    {
        /// <summary>
        /// Converts the raw loot data into a <see cref="LootDatabase"/>.
        /// </summary>
        /// <param name="jsonLootDatabase">The raw loot data loaded from the content file.</param>
        /// <returns>The domain loot data.</returns>
        public static LootDatabase Parse(JsonLootDatabase jsonLootDatabase)
        {
            LootDatabase newLootDatabase = new LootDatabase();
            foreach (var bonusType in jsonLootDatabase.darkness_bonuses)
            {
                List<DarknessBonus> bonuses = new List<DarknessBonus>();
                foreach (var bonus in bonusType.bonuses)
                {
                    DarknessBonus darkBonus = new DarknessBonus();
                    darkBonus.DarknessLevel = bonus.darkness;
                    darkBonus.Chance = bonus.chance;
                    darkBonus.Codes = bonus.codes;
                    bonuses.Add(darkBonus);
                }

                newLootDatabase.DarknessLoot.Add(bonusType.type, bonuses);
            }

            foreach (var table in jsonLootDatabase.loot_tables)
            {
                LootTable lootTable = new LootTable();
                lootTable.Id = table.id;
                lootTable.Difficulty = table.difficulty;
                lootTable.Dungeon = table.dungeon;

                foreach (var entry in table.entries)
                {
                    switch (entry.type)
                    {
                        case "nothing":
                            LootEntry lootEntry = new LootEntry();
                            lootEntry.Chance = entry.chances;
                            lootTable.Entries.Add(lootEntry);
                            break;
                        case "table":
                            LootEntryTable lootEntryTable = new LootEntryTable();
                            lootEntryTable.Chance = entry.chances;
                            lootEntryTable.TableId = (string)entry.data["table"];
                            lootTable.Entries.Add(lootEntryTable);
                            break;
                        case "item":
                            LootEntryItem lootEntryItem = new LootEntryItem();
                            lootEntryItem.Chance = entry.chances;
                            lootEntryItem.ItemType = (string)entry.data["type"];
                            lootEntryItem.ItemId = (string)entry.data["id"];
                            lootEntryItem.ItemAmount = (int)(long)entry.data["amount"];
                            lootTable.Entries.Add(lootEntryItem);
                            break;
                        case "trinket":
                            LootEntryTrinket lootEntryTrinket = new LootEntryTrinket();
                            lootEntryTrinket.Chance = entry.chances;
                            lootEntryTrinket.Rarity = (string)entry.data["rarity"];
                            lootTable.Entries.Add(lootEntryTrinket);
                            break;
                        case "journal_page":
                            LootEntryJournal lootEntryJournal = new LootEntryJournal();
                            lootEntryJournal.Chance = entry.chances;
                            if (entry.data.ContainsKey("specific_page_index"))
                            {
                                lootEntryJournal.SpecificId = (int)(long)entry.data["specific_page_index"];
                            }
                            else
                            {
                                lootEntryJournal.MinIndex = (int)(long)entry.data["min_page_index"];
                                lootEntryJournal.MaxIndex = (int)(long)entry.data["max_page_index"];
                            }
                            lootTable.Entries.Add(lootEntryJournal);
                            break;
                        default:
                            break;
                    }
                }

                if (!newLootDatabase.LootTables.ContainsKey(lootTable.Id))
                    newLootDatabase.LootTables.Add(lootTable.Id, new List<LootTable>());

                newLootDatabase.LootTables[lootTable.Id].Add(lootTable);
            }

            return newLootDatabase;
        }
    }
}
