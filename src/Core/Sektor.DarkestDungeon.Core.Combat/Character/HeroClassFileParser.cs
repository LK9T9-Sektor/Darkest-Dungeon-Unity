using System;
using System.Collections.Generic;
using System.Globalization;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>
    /// Parses legacy hero class definition files (Data/Heroes/Info format) into core hero classes.
    /// Loads the base upgrade rank only: level-0 weapon/armour stats and level-0 combat skills.
    /// Skill effects (<c>.effect</c> ids) are resolved from the supplied effects catalog.
    /// </summary>
    public static class HeroClassFileParser
    {
        private const string NamePrefix = "name";
        private const string InfoSectionMarker = "info";
        private const string EndMarker = ".end";

        /// <summary>Parses a hero class definition file content.</summary>
        /// <param name="content">The full text content of the definition file.</param>
        /// <param name="effects">The effects catalog used to resolve skill <c>.effect</c> ids (optional).</param>
        /// <returns>The parsed hero class, or null when name/weapon/armour sections are missing.</returns>
        public static HeroClass Parse(string content, EffectCatalog effects = null)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            var result = new HeroClass();
            var weaponStats = new Dictionary<AttributeType, float>();
            bool hasWeapon = false;
            bool hasArmour = false;
            string name = null;
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
                if (prefix == InfoSectionMarker)
                {
                    inInfo = true;
                    continue;
                }
                if (prefix == NamePrefix && !inInfo)
                {
                    name = line.Substring(colon + 1).Trim().ToLowerInvariant();
                    continue;
                }
                if (!inInfo)
                    continue;

                switch (prefix)
                {
                    case "resistances":
                        ApplyResistances(result, tokens);
                        break;
                    case "weapon":
                        if (!hasWeapon)
                            hasWeapon = ApplyWeapon(weaponStats, tokens);
                        break;
                    case "armour":
                        if (!hasArmour)
                            hasArmour = ApplyArmour(result, weaponStats, hasWeapon, tokens);
                        break;
                    case "combat_skill":
                        float weaponAccuracy = hasWeapon ? GetOrZero(weaponStats, AttributeType.AttackRating) : 0f;
                        ApplyCombatSkill(result.CombatSkills, tokens, weaponAccuracy, effects);
                        break;
                    case "riposte_skill":
                        float riposteWeaponAccuracy = hasWeapon ? GetOrZero(weaponStats, AttributeType.AttackRating) : 0f;
                        var riposteSkills = new List<CombatSkill>();
                        ApplyCombatSkill(riposteSkills, tokens, riposteWeaponAccuracy, effects);
                        if (riposteSkills.Count > 0)
                            result.RiposteSkill = riposteSkills[0];
                        break;
                    case "skill_selection":
                        result.CanSelectCombatSkills = "true" == GetValue(tokens, "can_select_combat_skills");
                        int maxSelected;
                        if (int.TryParse(GetValue(tokens, "number_of_selected_combat_skills_max"), NumberStyles.Integer, CultureInfo.InvariantCulture, out maxSelected))
                            result.NumberOfSelectedCombatSkills = maxSelected;
                        break;
                    case "tag":
                        string tagId = GetValue(tokens, "id");
                        if (tagId != null && !result.Tags.Contains(tagId))
                            result.Tags.Add(tagId);
                        break;
                    case "id_index":
                        int index;
                        if (int.TryParse(GetValue(tokens, "index"), NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                            result.IndexId = index;
                        break;
                    case "mode":
                        string modeId = GetValue(tokens, "id");
                        if (modeId != null)
                        {
                            result.Modes.Add(new CharacterMode
                            {
                                Id = modeId,
                                IsRaidDefault = IsTrue(GetValue(tokens, "is_raid_default")),
                            });
                        }
                        break;
                }
            }

            if (name == null || name.Length == 0 || !hasWeapon || !hasArmour)
                return null;

            result.StringId = name;
            return result;
        }

        /// <summary>Splits the value part of a line into ".key", ".key=value", ".key=value2" tokens.</summary>
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
                    tokens[suffix] = pieces[i + 1].Trim('"').ToLowerInvariant();
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

        private static void ApplyResistances(HeroClass result, Dictionary<string, string> tokens)
        {
            SetResistance(result, tokens, "stun", AttributeType.Stun);
            SetResistance(result, tokens, "poison", AttributeType.Poison);
            SetResistance(result, tokens, "bleed", AttributeType.Bleed);
            SetResistance(result, tokens, "disease", AttributeType.Disease);
            SetResistance(result, tokens, "move", AttributeType.Move);
            SetResistance(result, tokens, "debuff", AttributeType.Debuff);
            SetResistance(result, tokens, "death_blow", AttributeType.DeathBlow);
            SetResistance(result, tokens, "trap", AttributeType.Trap);
        }

        private static void SetResistance(HeroClass result, Dictionary<string, string> tokens, string key, AttributeType type)
        {
            float fraction;
            if (TryParseFraction(tokens, key, out fraction))
                result.Resistances[type] = fraction;
        }

        private static bool ApplyWeapon(
            Dictionary<AttributeType, float> weaponStats,
            Dictionary<string, string> tokens)
        {
            float damageLow;
            float damageHigh;
            float critChance;
            float speed;
            float accuracy;
            if (!TryParseNumber(tokens, "dmg", out damageLow) || !TryParseSecondNumber(tokens, "dmg", out damageHigh))
                return false;

            TryParseFraction(tokens, "crit", out critChance);
            TryParseNumber(tokens, "spd", out speed);
            TryParseFraction(tokens, "atk", out accuracy);

            weaponStats[AttributeType.DamageLow] = damageLow;
            weaponStats[AttributeType.DamageHigh] = damageHigh;
            weaponStats[AttributeType.CritChance] = critChance;
            weaponStats[AttributeType.SpeedRating] = speed;
            weaponStats[AttributeType.AttackRating] = accuracy;
            return true;
        }

        private static bool ApplyArmour(
            HeroClass result,
            Dictionary<AttributeType, float> weaponStats,
            bool hasWeapon,
            Dictionary<string, string> tokens)
        {
            float hitPoints;
            float protection;
            float defense;
            float speed;
            if (!TryParseNumber(tokens, "hp", out hitPoints))
                return false;

            TryParseNumber(tokens, "prot", out protection);
            TryParseFraction(tokens, "def", out defense);
            TryParseNumber(tokens, "spd", out speed);

            result.Attributes[AttributeType.HitPoints] = hitPoints;
            result.Attributes[AttributeType.ProtectionRating] = protection;
            result.Attributes[AttributeType.DefenseRating] = defense;
            result.Attributes[AttributeType.SpeedRating] =
                (hasWeapon ? GetOrZero(weaponStats, AttributeType.SpeedRating) : 0f) + speed;

            if (!hasWeapon)
                return true;

            result.Attributes[AttributeType.DamageLow] = GetOrZero(weaponStats, AttributeType.DamageLow);
            result.Attributes[AttributeType.DamageHigh] = GetOrZero(weaponStats, AttributeType.DamageHigh);
            result.Attributes[AttributeType.CritChance] = GetOrZero(weaponStats, AttributeType.CritChance);
            result.Attributes[AttributeType.AttackRating] = GetOrZero(weaponStats, AttributeType.AttackRating);
            return true;
        }

        private static float GetOrZero(Dictionary<AttributeType, float> source, AttributeType type)
        {
            float value;
            return source.TryGetValue(type, out value) ? value : 0f;
        }

        private static void ApplyCombatSkill(List<CombatSkill> skills, Dictionary<string, string> tokens, float weaponAccuracy, EffectCatalog effects)
        {
            int level;
            if (!int.TryParse(GetValue(tokens, "level"), NumberStyles.Integer, CultureInfo.InvariantCulture, out level) || level != 0)
                return;

            string id = GetValue(tokens, "id");
            if (id == null)
                return;

            float skillAccuracy;
            float damageMod;
            float critMod;
            float healLow;
            float healHigh;
            TryParseFraction(tokens, "atk", out skillAccuracy);
            TryParseFraction(tokens, "dmg", out damageMod);
            TryParseFraction(tokens, "crit", out critMod);

            var skill = new CombatSkill
            {
                Id = id,
                Level = 0,
                Type = GetValue(tokens, "type") ?? "melee",
                Accuracy = ClampAccuracy(weaponAccuracy + skillAccuracy),
                DamageMod = damageMod,
                CritMod = critMod,
                IsCritValid = IsTrue(GetValue(tokens, "is_crit_valid")),
                IsContinueTurn = IsTrue(GetValue(tokens, "is_continue_turn")),
                LaunchRanks = new FormationSet(GetValue(tokens, "launch") ?? string.Empty),
                TargetRanks = new FormationSet(GetValue(tokens, "target") ?? string.Empty),
            };

            int perTurnLimit;
            if (int.TryParse(GetValue(tokens, "per_turn_limit"), NumberStyles.Integer, CultureInfo.InvariantCulture, out perTurnLimit))
                skill.LimitPerTurn = perTurnLimit;

            int perBattleLimit;
            if (int.TryParse(GetValue(tokens, "per_battle_limit"), NumberStyles.Integer, CultureInfo.InvariantCulture, out perBattleLimit))
                skill.LimitPerBattle = perBattleLimit;

            if (TryParseNumber(tokens, "heal", out healLow))
            {
                if (!TryParseSecondNumber(tokens, "heal", out healHigh))
                    healHigh = healLow;
                skill.Category = SkillCategory.Heal;
                skill.Heal = new HealComponent((int)healLow, (int)healHigh);
            }
            else
            {
                skill.Category = SkillCategory.Damage;
            }

            // Legacy rule: skills with no accuracy or self-targeting never roll to hit.
            float rawAccuracy = weaponAccuracy + skillAccuracy;
            if (skill.Category != SkillCategory.Heal &&
                (rawAccuracy == 0 || skill.TargetRanks.IsSelfTarget || skill.TargetRanks.IsSelfFormation))
                skill.Category = SkillCategory.Support;

            if (effects != null)
            {
                int effectIndex = 1;
                while (true)
                {
                    string effectId = effectIndex == 1 ? GetValue(tokens, "effect") : GetValue(tokens, "effect#" + effectIndex);
                    if (effectId == null)
                        break;

                    var effect = effects.Get(effectId);
                    if (effect != null)
                        skill.Effects.Add(effect);
                    effectIndex++;
                }

                foreach (var modeKey in tokens.Keys)
                {
                    if (!modeKey.EndsWith("effects", StringComparison.Ordinal))
                        continue;

                    string modeId = modeKey.Substring(0, modeKey.Length - "effects".Length).TrimEnd('_');
                    if (modeId.Length == 0)
                        continue;

                    if (!skill.ModeEffects.ContainsKey(modeId))
                        skill.ModeEffects[modeId] = new List<Effect>();

                    int modeEffectIndex = 1;
                    while (true)
                    {
                        string modeEffectId = modeEffectIndex == 1
                            ? GetValue(tokens, modeKey)
                            : GetValue(tokens, modeKey + "#" + modeEffectIndex);
                        if (modeEffectId == null)
                            break;
                        var modeEffect = effects.Get(modeEffectId);
                        if (modeEffect != null)
                            skill.ModeEffects[modeId].Add(modeEffect);
                        modeEffectIndex++;
                    }
                }
            }

            int validModeIndex = 1;
            while (true)
            {
                string validMode = validModeIndex == 1 ? GetValue(tokens, "valid_modes") : GetValue(tokens, "valid_modes#" + validModeIndex);
                if (validMode == null)
                    break;
                skill.ValidModes.Add(validMode);
                validModeIndex++;
            }

            skills.RemoveAll(existing => existing.Id == id);
            skills.Add(skill);
        }

        private static float ClampAccuracy(float accuracy)
        {
            return Math.Max(BattleConstants.MinAccuracy, Math.Min(accuracy, BattleConstants.MaxChance));
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

        private static bool IsTrue(string value)
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}

