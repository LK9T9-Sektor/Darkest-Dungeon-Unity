using System.Collections.Generic;
using System.Globalization;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>
    /// Parses the <c>Data/Mechanics/MapGenerator</c> DSL into a list of
    /// <see cref="DungeonGenerationData"/> entries. Each <c>map:</c> block defines one entry; every
    /// line is a <c>.key value</c> or <c>.key min max</c> token pair.
    /// </summary>
    public static class DungeonGenerationDataParser
    {
        /// <summary>Parses the map generator DSL text.</summary>
        /// <param name="content">The map generator text.</param>
        /// <returns>The parsed generation data entries.</returns>
        public static List<DungeonGenerationData> Parse(string content)
        {
            var result = new List<DungeonGenerationData>();
            if (string.IsNullOrEmpty(content))
                return result;

            DungeonGenerationData current = null;
            var blockTokens = new Dictionary<string, string>(System.StringComparer.Ordinal);
            foreach (var rawLine in content.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                if (line.StartsWith("map:", System.StringComparison.Ordinal))
                {
                    if (current != null)
                        ApplyTokens(current, blockTokens);
                    current = new DungeonGenerationData();
                    result.Add(current);
                    blockTokens.Clear();
                    continue;
                }
                if (current == null || !line.StartsWith(".", System.StringComparison.Ordinal))
                    continue;

                foreach (var pair in ParseTokens(line))
                    blockTokens[pair.Key] = pair.Value;
            }

            if (current != null)
                ApplyTokens(current, blockTokens);

            return result;
        }

        private static void ApplyTokens(DungeonGenerationData data, Dictionary<string, string> tokens)
        {
            data.Length = GetValue(tokens, "size");
            data.QuestType = GetValue(tokens, "quest_type");
            data.Dungeon = GetValue(tokens, "dungeon_type");
            data.BaseRoomNumber = ReadInt(tokens, "base_room_number");
            data.BaseCorridorNumber = ReadInt(tokens, "base_corridor_number");

            int[] grid = ReadPair(tokens, "gridsize");
            if (grid != null)
            {
                data.GridSizeX = grid[0];
                data.GridSizeY = grid[1];
            }

            data.Spacing = ReadInt(tokens, "spacing");
            data.GoalRoomNumber = ReadInt(tokens, "goal_room_number");
            data.MinFinalDistance = ReadInt(tokens, "min_final_distance");

            int hallwayBattleMin, hallwayBattleMax;
            ReadPairInto(tokens, "hallway_battle", out hallwayBattleMin, out hallwayBattleMax);
            data.HallwayBattleMin = hallwayBattleMin;
            data.HallwayBattleMax = hallwayBattleMax;

            int hallwayTrapMin, hallwayTrapMax;
            ReadPairInto(tokens, "hallway_trap", out hallwayTrapMin, out hallwayTrapMax);
            data.HallwayTrapMin = hallwayTrapMin;
            data.HallwayTrapMax = hallwayTrapMax;

            int hallwayObstacleMin, hallwayObstacleMax;
            ReadPairInto(tokens, "hallway_obstacle", out hallwayObstacleMin, out hallwayObstacleMax);
            data.HallwayObstacleMin = hallwayObstacleMin;
            data.HallwayObstacleMax = hallwayObstacleMax;

            int hallwayCurioMin, hallwayCurioMax;
            ReadPairInto(tokens, "hallway_curio", out hallwayCurioMin, out hallwayCurioMax);
            data.HallwayCurioMin = hallwayCurioMin;
            data.HallwayCurioMax = hallwayCurioMax;

            int hallwayHungerMin, hallwayHungerMax;
            ReadPairInto(tokens, "hallway_hunger", out hallwayHungerMin, out hallwayHungerMax);
            data.HallwayHungerMin = hallwayHungerMin;
            data.HallwayHungerMax = hallwayHungerMax;

            int totalRoomBattleMin, totalRoomBattleMax;
            ReadPairInto(tokens, "total_room_battles", out totalRoomBattleMin, out totalRoomBattleMax);
            data.TotalRoomBattleMin = totalRoomBattleMin;
            data.TotalRoomBattleMax = totalRoomBattleMax;

            int roomGuardedCurioMin, roomGuardedCurioMax;
            ReadPairInto(tokens, "room_guarded_curio", out roomGuardedCurioMin, out roomGuardedCurioMax);
            data.RoomGuardedCurioMin = roomGuardedCurioMin;
            data.RoomGuardedCurioMax = roomGuardedCurioMax;

            int roomGuardedTresureMin, roomGuardedTresureMax;
            ReadPairInto(tokens, "room_guarded_treasure", out roomGuardedTresureMin, out roomGuardedTresureMax);
            data.RoomGuardedTresureMin = roomGuardedTresureMin;
            data.RoomGuardedTresureMax = roomGuardedTresureMax;
        }

        private static Dictionary<string, string> ParseTokens(string part)
        {
            var tokens = new Dictionary<string, string>(System.StringComparer.Ordinal);
            string[] pieces = part.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i].Trim();
                if (!piece.StartsWith(".", System.StringComparison.Ordinal))
                    continue;

                string key = piece.Substring(1).ToLowerInvariant();
                var values = new List<string>();
                while (i + 1 < pieces.Length && !pieces[i + 1].StartsWith(".", System.StringComparison.Ordinal))
                {
                    values.Add(pieces[i + 1].Trim());
                    i++;
                }
                if (values.Count > 0)
                    tokens[key] = string.Join(" ", values);
            }
            return tokens;
        }

        private static string GetValue(Dictionary<string, string> tokens, string key)
        {
            string value;
            return tokens.TryGetValue(key, out value) ? value : null;
        }

        private static int ReadInt(Dictionary<string, string> tokens, string key)
        {
            string value = GetValue(tokens, key);
            int parsed;
            return value != null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : 0;
        }

        private static int[] ReadPair(Dictionary<string, string> tokens, string key)
        {
            string value = GetValue(tokens, key);
            if (value == null)
                return null;

            string[] parts = value.Split(' ');
            if (parts.Length < 2)
                return null;

            int a, b;
            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out a))
                return null;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out b))
                return null;
            return new[] { a, b };
        }

        private static void ReadPairInto(Dictionary<string, string> tokens, string key, out int min, out int max)
        {
            int[] pair = ReadPair(tokens, key);
            min = pair != null ? pair[0] : 0;
            max = pair != null ? pair[1] : 0;
        }
    }
}