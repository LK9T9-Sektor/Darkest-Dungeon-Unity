using System.Collections.Generic;
using System.Text;

using Sektor.DarkestDungeon.Core.Content.Database;
using Sektor.DarkestDungeon.Core.Content.Raid;

/// <summary>
/// The set of in-game test checks shown in the TEST menu. Each check reads the loaded content
/// (mostly from the pure core assembly) and returns a short log line, so regressions in content
/// loading and in the core mappers/parsers are visible right in the main menu.
/// </summary>
public static class TestActions
{
    private static readonly List<TestActionDefinition> _actions = new List<TestActionDefinition>
    {
        new TestActionDefinition("Database counts", DatabaseCounts),
        new TestActionDefinition("Loot tables", LootTables),
        new TestActionDefinition("Loot roll", LootRoll),
        new TestActionDefinition("Curio", Curio),
        new TestActionDefinition("Narration", Narration),
        new TestActionDefinition("Heirloom exchange", HeirloomExchange),
        new TestActionDefinition("Party names", PartyNames),
    };

    /// <summary>Gets the available test actions in display order.</summary>
    public static IReadOnlyList<TestActionDefinition> Actions { get { return _actions; } }

    private static string DatabaseCounts()
    {
        var data = DarkestDungeonManager.Data;
        var sb = new StringBuilder();
        sb.Append("HeroClasses=").Append(data.HeroClasses.Count)
            .Append(" Buffs=").Append(data.Buffs.Count)
            .Append(" Quirks=").Append(data.Quirks.Count)
            .Append(" Curios=").Append(data.Curios.Count)
            .Append(" Obstacles=").Append(data.Obstacles.Count)
            .Append(" Traps=").Append(data.Traps.Count)
            .Append(" Items=").Append(data.Items.Count)
            .Append(" Monsters=").Append(data.Monsters.Count)
            .Append(" Narration=").Append(data.Narration.Count)
            .Append(" PartyNames=").Append(data.PartyNames.Count)
            .Append(" HeirloomExchanges=").Append(data.HeirloomExchanges.Count)
            .Append(" LootTables=").Append(data.LootDatabase.LootTables.Count);
        return sb.ToString();
    }

    private static string LootTables()
    {
        var sb = new StringBuilder();
        int tables = 0;
        int entries = 0;
        foreach (var pair in DarkestDungeonManager.Data.LootDatabase.LootTables)
        {
            foreach (var table in pair.Value)
            {
                tables++;
                entries += table.Entries.Count;
                sb.AppendLine(pair.Key + " diff=" + table.Difficulty + " dungeon='" + table.Dungeon
                    + "' entries=" + table.Entries.Count);
            }
        }
        return "tables=" + tables + " entries=" + entries + "\n" + sb.ToString().TrimEnd();
    }

    private static string LootRoll()
    {
        var loot = DarkestDungeonManager.Data.LootDatabase;
        LootTable table = null;
        foreach (var list in loot.LootTables.Values)
            foreach (var candidate in list)
                if (candidate.Difficulty == 0 && string.IsNullOrEmpty(candidate.Dungeon))
                {
                    table = candidate;
                    break;
                }

        if (table == null)
            foreach (var list in loot.LootTables.Values)
                if (list.Count > 0)
                {
                    table = list[0];
                    break;
                }

        if (table == null || table.Entries.Count == 0)
            return "no loot table to roll";

        LootEntry entry = RandomSolver.ChooseBySingleRandom(table.Entries);
        return "table=" + table.Id + " rolled=" + (entry != null ? entry.Type.ToString() : "null")
            + " chance=" + (entry != null ? entry.Chance.ToString() : "?");
    }

    private static string Curio()
    {
        var curios = DarkestDungeonManager.Data.Curios;
        if (curios.Count == 0)
            return "curios=0";

        var enumerator = curios.Values.GetEnumerator();
        enumerator.MoveNext();
        var curio = enumerator.Current;
        return "curios=" + curios.Count + " sample='" + curio.StringId
            + "' results=" + curio.Results.Count + " itemInteractions=" + curio.ItemInteractions.Count;
    }

    private static string Narration()
    {
        var narration = DarkestDungeonManager.Data.Narration;
        if (narration.Count == 0)
            return "narration=0";

        var enumerator = narration.Values.GetEnumerator();
        enumerator.MoveNext();
        var entry = enumerator.Current;
        return "entries=" + narration.Count + " sample='" + entry.Id
            + "' audioEvents=" + entry.AudioEvents.Count;
    }

    private static string HeirloomExchange()
    {
        var exchanges = DarkestDungeonManager.Data.HeirloomExchanges;
        if (exchanges.Count == 0)
            return "exchanges=0";

        var first = exchanges[0];
        return "exchanges=" + exchanges.Count + " first='" + first.FromType + "->" + first.ToType
            + "' amounts=" + first.FromAmount + "/" + first.ToAmount;
    }

    private static string PartyNames()
    {
        var names = DarkestDungeonManager.Data.PartyNames;
        if (names == null || names.Count == 0)
            return "partyNames=0";

        return "partyNames=" + names.Count + " first id='" + names[0].Id + "' classes="
            + (names[0].ClassIds != null ? names[0].ClassIds.Count : 0);
    }
}
