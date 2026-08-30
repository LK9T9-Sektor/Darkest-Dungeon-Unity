using System.Collections.Generic;
using System.Globalization;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>
    /// Parses the <c>Data/Dungeons/*</c> DSL into a <see cref="DungeonEnviromentData"/>. The text is
    /// split into two sections: <c>mash:</c> (battle encounter pools per difficulty) and
    /// <c>props:</c> (weighted prop pools). Encounter lines use <c>.chance N .types a b c</c>.
    /// </summary>
    public static class DungeonEnviromentDataParser
    {
        /// <summary>Parses the dungeon environment DSL text.</summary>
        /// <param name="content">The environment text.</param>
        /// <returns>The parsed environment data.</returns>
        public static DungeonEnviromentData Parse(string content)
        {
            var result = new DungeonEnviromentData();
            if (string.IsNullOrEmpty(content))
                return result;

            DungeonBattleMash currentMash = null;
            bool inMash = false;
            bool inProps = false;

            foreach (var rawLine in content.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                int colon = line.IndexOf(':');
                string prefix = colon > 0 ? line.Substring(0, colon).Trim().ToLowerInvariant() : null;

                if (prefix == "mash")
                {
                    inMash = true;
                    inProps = false;
                    continue;
                }
                if (prefix == "props")
                {
                    inMash = false;
                    inProps = true;
                    continue;
                }

                if (prefix == "id" && inMash)
                {
                    currentMash = new DungeonBattleMash { MashId = ReadInt(line.Substring(colon + 1)) };
                    result.BattleMashes.Add(currentMash);
                    continue;
                }
                if (prefix == "id" && !inMash && !inProps)
                {
                    continue;
                }

                if (inMash)
                {
                    ApplyMashLine(currentMash, prefix, line.Substring(colon + 1));
                    continue;
                }
                if (inProps)
                {
                    ApplyPropLine(result, prefix, line.Substring(colon + 1));
                    continue;
                }

                if (prefix == "hall_variants")
                    result.HallVariations = ReadInt(line.Substring(colon + 1));
                else if (prefix == "room_variants")
                    result.RoomVariations = ReadWords(line.Substring(colon + 1));
            }

            return result;
        }

        private static void ApplyMashLine(DungeonBattleMash mash, string prefix, string body)
        {
            if (mash == null)
                return;

            var encounter = ParseEncounter(body);
            if (encounter == null)
                return;

            switch (prefix)
            {
                case "hall":
                    mash.HallEncounters.Add(encounter);
                    break;
                case "room":
                    mash.RoomEncounters.Add(encounter);
                    break;
                case "boss":
                    mash.BossEncounters.Add(encounter);
                    break;
                case "stall":
                    mash.StallEncounters.Add(encounter);
                    break;
                case "named":
                    string name = GetToken(body, "name");
                    if (name != null)
                    {
                        List<DungeonBattleEncounter> pool;
                        if (!mash.NamedEncounters.TryGetValue(name, out pool))
                        {
                            pool = new List<DungeonBattleEncounter>();
                            mash.NamedEncounters[name] = pool;
                        }
                        pool.Add(encounter);
                    }
                    break;
            }
        }

        private static void ApplyPropLine(DungeonEnviromentData data, string prefix, string body)
        {
            var prop = ParseProp(body);
            if (prop == null)
                return;

            switch (prefix)
            {
                case "hall_curios":
                    data.HallCurios.Add(prop);
                    break;
                case "room_curios":
                    data.RoomCurios.Add(prop);
                    break;
                case "room_treasures":
                    data.RoomTresures.Add(prop);
                    break;
                case "traps":
                    data.Traps.Add(prop);
                    break;
                case "obstacles":
                    data.Obstacles.Add(prop);
                    break;
                case "secret_room_treasures":
                    data.SecretTresures.Add(prop);
                    break;
            }
        }

        private static DungeonBattleEncounter ParseEncounter(string body)
        {
            int chance;
            if (!TryReadChance(body, out chance))
                return null;

            var types = ReadTypeTokens(body);
            return new DungeonBattleEncounter(chance, types);
        }

        private static DungeonPropsEncounter ParseProp(string body)
        {
            int chance;
            if (!TryReadChance(body, out chance))
                return null;

            var words = ReadWords(body);
            string propName = words.Count > 0 ? words[0] : null;
            return propName == null ? null : new DungeonPropsEncounter(chance, propName);
        }

        private static bool TryReadChance(string body, out int chance)
        {
            chance = 0;
            string token = GetToken(body, "chance");
            return token != null && int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out chance);
        }

        private static List<string> ReadTypeTokens(string body)
        {
            string token = GetToken(body, "types");
            return token == null ? new List<string>() : ReadWords(token);
        }

        private static List<string> ReadWords(string text)
        {
            var words = new List<string>();
            if (text == null)
                return words;

            foreach (var piece in text.Trim().Split(' '))
            {
                string word = piece.Trim();
                if (word.Length > 0)
                    words.Add(word);
            }
            return words;
        }

        private static string GetToken(string body, string key)
        {
            string[] pieces = body.Trim().Split(' ');
            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i].Trim();
                if (piece == "." + key && i + 1 < pieces.Length)
                    return pieces[i + 1].Trim();
            }
            return null;
        }

        private static int ReadInt(string text)
        {
            int value;
            return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
        }
    }
}