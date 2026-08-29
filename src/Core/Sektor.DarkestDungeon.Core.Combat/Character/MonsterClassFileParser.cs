using System;
using System.Collections.Generic;
using System.Globalization;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>
    /// Parses a single Data\Monsters\*.txt file into a <see cref="MonsterClass"/> mirroring the
    /// legacy Unity loader behavior.
    /// </summary>
    public static class MonsterClassFileParser
    {
        private const string EndMarker = ".end";
        private const string NamePrefix = "name";
        private const string TypePrefix = "type";
        private const string ArtSectionMarker = "art";
        private const string InfoSectionMarker = "info";

        /// <summary>Parses a monster class from file content.</summary>
        /// <param name="content">The monster .txt file content.</param>
        /// <param name="effects">The effect catalog used to resolve skill effects (optional).</param>
        /// <returns>The parsed monster class, or null when the content has no name/type.</returns>
        public static MonsterClass Parse(string content, EffectCatalog effects = null)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            var result = new MonsterClass();
            bool inInfo = false;

            foreach (var rawLine in content.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                int colon = line.IndexOf(':');
                if (colon <= 0)
                    continue;

                string prefix = line.Substring(0, colon).Trim().ToLowerInvariant();
                var tokens = ParseTokens(line.Substring(colon + 1));

                if (prefix == EndMarker)
                {
                    inInfo = false;
                    continue;
                }
                if (prefix == InfoSectionMarker || prefix == ArtSectionMarker)
                {
                    inInfo = prefix == InfoSectionMarker;
                    continue;
                }
                if (!inInfo)
                {
                    if (prefix == NamePrefix)
                        result.StringId = line.Substring(colon + 1).Trim();
                    if (prefix == TypePrefix)
                        result.TypeId = line.Substring(colon + 1).Trim();
                    continue;
                }

                switch (prefix)
                {
                    case "display":
                        float size;
                        if (TryParseNumber(tokens, "size", out size))
                            result.Size = (int)size;
                        break;
                    case "enemy_type":
                        string enemyTypeId = GetValue(tokens, "id");
                        if (enemyTypeId != null)
                        {
                            MonsterType type = StringToMonsterType(enemyTypeId);
                            if (type != MonsterType.None && !result.EnemyTypes.Contains(type))
                                result.EnemyTypes.Add(type);
                        }
                        break;
                    case "stats":
                        ApplyStats(result, tokens);
                        break;
                    case "skill":
                        ApplyCombatSkill(result, tokens, effects);
                        break;
                    case "personality":
                        float preferredSkill;
                        if (TryParseNumber(tokens, "prefskill", out preferredSkill))
                            result.PreferableSkill = (int)preferredSkill;
                        break;
                    case "initiative":
                        float initiativeTurns;
                        if (TryParseNumber(tokens, "number_of_turns_per_round", out initiativeTurns))
                            result.InitiativeTurns = (int)initiativeTurns;
                        break;
                    case "monster_brain":
                        string brainId = GetValue(tokens, "id");
                        if (brainId != null)
                            result.MonsterBrainId = brainId;
                        break;
                    case "battle_modifier":
                        result.Modifiers = new BattleModifier(
                            IsTrue(GetValue(tokens, "disable_stall_penalty")),
                            ReadBoolOr(GetValue(tokens, "can_surprise"), true),
                            ReadBoolOr(GetValue(tokens, "can_be_surprised"), true),
                            IsTrue(GetValue(tokens, "always_surprise")),
                            IsTrue(GetValue(tokens, "always_be_surprised")));
                        break;
                }
            }

            if (string.IsNullOrEmpty(result.StringId) || string.IsNullOrEmpty(result.TypeId))
                return null;

            if (result.Modifiers == null)
                result.Modifiers = new BattleModifier(false, true, true, false, false);

            return result;
        }

        private static void ApplyStats(MonsterClass result, Dictionary<string, string> tokens)
        {
            float hitPoints;
            float defense;
            float protection;
            float speed;
            if (TryParseNumber(tokens, "hp", out hitPoints)
                && tokens.ContainsKey("def")
                && tokens.ContainsKey("prot")
                && tokens.ContainsKey("spd"))
            {
                result.Attributes[AttributeType.HitPoints] = hitPoints;
                result.Attributes[AttributeType.DefenseRating] = ParsePercent(tokens, "def");
                result.Attributes[AttributeType.ProtectionRating] = ParsePlain(tokens, "prot");
                result.Attributes[AttributeType.SpeedRating] = ParsePlain(tokens, "spd");
                result.Attributes[AttributeType.Stun] = ParsePercent(tokens, "stun_resist");
                result.Attributes[AttributeType.Poison] = ParsePercent(tokens, "poison_resist");
                result.Attributes[AttributeType.Bleed] = ParsePercent(tokens, "bleed_resist");
                result.Attributes[AttributeType.Debuff] = ParsePercent(tokens, "debuff_resist");
                result.Attributes[AttributeType.Move] = ParsePercent(tokens, "move_resist");
            }
        }

        private static void ApplyCombatSkill(MonsterClass result, Dictionary<string, string> tokens, EffectCatalog effects)
        {
            string id = GetValue(tokens, "id");
            if (id == null)
                return;

            float damageLow;
            float damageHigh;
            if (!TryParseNumber(tokens, "dmg", out damageLow))
                return;
            if (!TryParseSecondNumber(tokens, "dmg", out damageHigh))
                damageHigh = damageLow;

            float accuracy;
            TryParseFraction(tokens, "atk", out accuracy);
            float critMod;
            TryParseFraction(tokens, "crit", out critMod);
            float extraTargetsChance;
            TryParseFraction(tokens, "extra_targets_chance", out extraTargetsChance);

            var skill = new CombatSkill
            {
                Id = id,
                Level = 0,
                Type = GetValue(tokens, "type") ?? "melee",
                Accuracy = ClampAccuracy(accuracy),
                DamageMin = damageLow,
                DamageMax = damageHigh,
                CritMod = critMod,
                IsCritValid = IsTrue(GetValue(tokens, "is_crit_valid")),
                ExtraTargetsChance = extraTargetsChance,
                LaunchRanks = new FormationSet(GetValue(tokens, "launch") ?? string.Empty),
                TargetRanks = new FormationSet(GetValue(tokens, "target") ?? string.Empty),
                Category = SkillCategory.Damage,
            };

            float push;
            float pull;
            if (TryParseNumber(tokens, "move", out push))
            {
                if (!TryParseSecondNumber(tokens, "move", out pull))
                    pull = 0;
                skill.Move = new MoveComponent((int)push, (int)pull);
            }

            float healLow;
            if (TryParseNumber(tokens, "heal", out healLow))
            {
                float healHigh;
                if (!TryParseSecondNumber(tokens, "heal", out healHigh))
                    healHigh = healLow;
                skill.Category = SkillCategory.Heal;
                skill.Heal = new HealComponent((int)healLow, (int)healHigh);
            }

            if (effects != null)
            {
                int effectIndex = 1;
                while (true)
                {
                    string effectId = effectIndex == 1
                        ? GetValue(tokens, "effect")
                        : GetValue(tokens, "effect#" + effectIndex);
                    if (effectId == null)
                        break;

                    Effect effect = effects.Get(effectId);
                    if (effect != null)
                        skill.Effects.Add(effect);
                    effectIndex++;
                }
            }

            result.CombatSkills.RemoveAll(existing => existing.Id == id);
            result.CombatSkills.Add(skill);
        }

        private static MonsterType StringToMonsterType(string value)
        {
            switch (value)
            {
                case "unholy": return MonsterType.Unholy;
                case "man": return MonsterType.Man;
                case "eldritch": return MonsterType.Eldritch;
                case "beast": return MonsterType.Beast;
                case "corpse": return MonsterType.Corpse;
                case "carpentry": return MonsterType.Carpentry;
                case "ironwork": return MonsterType.Ironwork;
                case "cauldron": return MonsterType.Cauldron;
                case "cosmic": return MonsterType.Cosmic;
                default: return MonsterType.None;
            }
        }

        private static Dictionary<string, string> ParseTokens(string part)
        {
            var tokens = new Dictionary<string, string>(StringComparer.Ordinal);
            string[] pieces = SplitRespectingQuotes(part);
            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i];
                if (!piece.StartsWith(".", StringComparison.Ordinal))
                    continue;

                string key = piece.Substring(1).ToLowerInvariant();
                if (tokens.ContainsKey(key))
                    continue;

                int valueIndex = 1;
                while (i + 1 < pieces.Length && !pieces[i + 1].StartsWith(".", StringComparison.Ordinal))
                {
                    string suffix = valueIndex == 1 ? key : key + "#" + valueIndex;
                    tokens[suffix] = pieces[i + 1].Trim('"');
                    valueIndex++;
                    i++;
                }
            }
            return tokens;
        }

        private static string[] SplitRespectingQuotes(string text)
        {
            var pieces = new List<string>();
            var current = new List<char>();
            bool inQuotes = false;
            foreach (char c in text)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Add(c);
                    continue;
                }
                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Count > 0)
                    {
                        pieces.Add(new string(current.ToArray()));
                        current.Clear();
                    }
                    continue;
                }
                current.Add(c);
            }
            if (current.Count > 0)
                pieces.Add(new string(current.ToArray()));
            return pieces.ToArray();
        }

        private static string GetValue(Dictionary<string, string> tokens, string key)
        {
            string value;
            return tokens.TryGetValue(key, out value) ? value : null;
        }

        private static bool ReadBoolOr(string value, bool fallback)
        {
            return value == null ? fallback : IsTrue(value.ToLowerInvariant());
        }

        private static bool IsTrue(string value)
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static float ParsePercent(Dictionary<string, string> tokens, string key)
        {
            float value;
            TryParseFraction(tokens, key, out value);
            return value;
        }

        private static float ParsePlain(Dictionary<string, string> tokens, string key)
        {
            float value;
            TryParseNumber(tokens, key, out value);
            return value;
        }

        private static bool TryParseFraction(Dictionary<string, string> tokens, string key, out float fraction)
        {
            fraction = 0f;
            string value = GetValue(tokens, key);
            if (value == null)
                return false;
            float percent;
            if (!float.TryParse(value.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out percent))
                return false;
            fraction = percent / 100f;
            return true;
        }

        private static bool TryParseNumber(Dictionary<string, string> tokens, string key, out float value)
        {
            value = 0f;
            string raw = GetValue(tokens, key);
            if (raw == null)
                return false;
            return float.TryParse(raw.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseSecondNumber(Dictionary<string, string> tokens, string key, out float value)
        {
            value = 0f;
            string raw;
            if (!tokens.TryGetValue(key + "#2", out raw))
                return false;
            return float.TryParse(raw.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static float ClampAccuracy(float accuracy)
        {
            return Math.Max(0.1f, Math.Min(accuracy, 0.95f));
        }
    }
}