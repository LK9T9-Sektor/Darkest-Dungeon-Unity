using System;
using System.Collections.Generic;
using System.Globalization;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>
    /// Parses a single <c>effect:</c> line of the Effects DSL into an <see cref="Effect"/>.
    /// Extracted from the catalog so the parsing rules are independently testable and the catalog
    /// stays a simple storage.
    /// </summary>
    public static class EffectParser
    {
        /// <summary>Parses the DSL tokens of one effect line.</summary>
        /// <param name="content">The text after the <c>effect:</c> prefix.</param>
        /// <returns>The parsed effect, or null when it has no sub-effects and no torch delta.</returns>
        public static Effect Parse(string content)
        {
            return ParseEffect(ParseTokens(content));
        }

        /// <summary>Parses effect tokens into an effect definition.</summary>
        /// <param name="tokens">The parsed key/value tokens.</param>
        /// <returns>The effect or null when empty.</returns>
        public static Effect ParseEffect(Dictionary<string, string> tokens)
        {
            var effect = new Effect
            {
                Name = GetValue(tokens, "name") ?? string.Empty,
                TargetType = MapTarget(GetValue(tokens, "target")),
            };

            int chance;
            if (int.TryParse(TrimPercent(GetValue(tokens, "chance")), out chance))
                effect.IntegerParams[EffectIntParams.Chance] = chance;

            int stress;
            if (int.TryParse(TrimPercent(GetValue(tokens, "stress")), out stress))
                effect.SubEffects.Add(new StressEffect(stress));

            int healStress;
            if (int.TryParse(TrimPercent(GetValue(tokens, "healstress")), out healStress))
                effect.SubEffects.Add(new StressHealEffect(healStress));

            int heal;
            if (int.TryParse(TrimPercent(GetValue(tokens, "heal")), out heal))
                effect.SubEffects.Add(new HealEffect(heal));

            if (tokens.ContainsKey("stun"))
                effect.SubEffects.Add(new StunEffect());

            int dotBleed;
            if (int.TryParse(TrimPercent(GetValue(tokens, "dotbleed")), out dotBleed))
                effect.SubEffects.Add(new BleedEffect(dotBleed));

            int dotPoison;
            if (int.TryParse(TrimPercent(GetValue(tokens, "dotpoison")), out dotPoison))
                effect.SubEffects.Add(new PoisonEffect(dotPoison));

            int pull;
            if (int.TryParse(TrimPercent(GetValue(tokens, "pull")), out pull))
                effect.SubEffects.Add(new PullEffect(pull));

            int push;
            if (int.TryParse(TrimPercent(GetValue(tokens, "push")), out push))
                effect.SubEffects.Add(new PushEffect(push));

            if (tokens.ContainsKey("cure"))
                effect.SubEffects.Add(new CureEffect());

            var statAdds = new Dictionary<AttributeType, float>();
            var statMults = new Dictionary<AttributeType, float>();
            TryAddFraction(tokens, "attack_rating_add", statAdds, AttributeType.AttackRating);
            TryAddFraction(tokens, "crit_chance_add", statAdds, AttributeType.CritChance);
            TryAddFraction(tokens, "critical_rating", statAdds, AttributeType.CritChance);
            TryAddFraction(tokens, "defense_rating_add", statAdds, AttributeType.DefenseRating);
            TryAddFraction(tokens, "protection_rating_add", statAdds, AttributeType.ProtectionRating);
            TryAddFlat(tokens, "speed_rating", statAdds, AttributeType.SpeedRating);
            TryAddFlat(tokens, "speed_rating_add", statAdds, AttributeType.SpeedRating);
            TryAddFraction(tokens, "damage_low_multiply", statMults, AttributeType.DamageLow);
            TryAddFraction(tokens, "damage_high_multiply", statMults, AttributeType.DamageHigh);

            if (tokens.ContainsKey("shuffleparty"))
                effect.SubEffects.Add(new ShuffleTargetEffect(true));

            if (tokens.ContainsKey("shuffletarget"))
                effect.SubEffects.Add(new ShuffleTargetEffect(false));

            if (tokens.ContainsKey("tag") || tokens.ContainsKey("mark"))
                effect.SubEffects.Add(new TagEffect());

            if (tokens.ContainsKey("immobilize"))
                effect.SubEffects.Add(new ImmobilizeEffect());

            if (tokens.ContainsKey("riposte"))
            {
                var riposte = new RiposteEffect();
                CopyInto(statAdds, riposte.StatAddBuffs);
                CopyInto(statMults, riposte.StatMultBuffs);
                effect.SubEffects.Add(riposte);
            }
            else if (tokens.ContainsKey("combat_stat_buff"))
            {
                var statBuff = new CombatStatBuffEffect();
                CopyInto(statAdds, statBuff.StatAddBuffs);
                CopyInto(statMults, statBuff.StatMultBuffs);
                if (statBuff.StatAddBuffs.Count > 0 || statBuff.StatMultBuffs.Count > 0)
                    effect.SubEffects.Add(statBuff);
            }

            if (tokens.ContainsKey("guard"))
            {
                bool swapTargets;
                if (!bool.TryParse(GetValue(tokens, "swap_source_and_target"), out swapTargets))
                    swapTargets = false;
                effect.SubEffects.Add(new GuardEffect(swapTargets));
            }

            bool clearGuarding = tokens.ContainsKey("clearguarding");
            bool clearGuarded = tokens.ContainsKey("clearguarded");
            if (clearGuarding || clearGuarded)
            {
                var clearGuard = new ClearGuardEffect();
                clearGuard.SetFlags(clearGuarding, clearGuarded);
                effect.SubEffects.Add(clearGuard);
            }

            if (tokens.ContainsKey("unstun"))
                effect.SubEffects.Add(new UnstunEffect());

            if (tokens.ContainsKey("unimmobilize"))
                effect.SubEffects.Add(new UnimmobilizeEffect());

            if (tokens.ContainsKey("untag"))
                effect.SubEffects.Add(new UntagEffect());

            if (tokens.ContainsKey("kill"))
                effect.SubEffects.Add(new KillEffect());

            string killEnemyType = GetValue(tokens, "kill_enemy_types");
            if (killEnemyType != null)
            {
                MonsterType monsterType = StringToMonsterType(killEnemyType);
                if (monsterType != MonsterType.None)
                    effect.SubEffects.Add(new KillEnemyTypeEffect(monsterType));
            }

            if (tokens.ContainsKey("performer_rank_target"))
                effect.SubEffects.Add(new PerformerRankTargetEffect());

            if (tokens.ContainsKey("clear_rank_target"))
                effect.SubEffects.Add(new ClearRankTargetEffect());

            string disease = GetValue(tokens, "disease");
            if (disease != null)
            {
                if (disease == "any")
                    effect.SubEffects.Add(new DiseaseEffect(null, true));
            }

            string firstBuffId = GetValue(tokens, "buff_ids");
            if (firstBuffId != null)
            {
                var buffEffect = new BuffEffect();
                buffEffect.BuffIds.Add(firstBuffId);
                int buffIndex = 2;
                while (true)
                {
                    string buffId = GetValue(tokens, "buff_ids#" + buffIndex);
                    if (buffId == null)
                        break;
                    buffEffect.BuffIds.Add(buffId);
                    buffIndex++;
                }
                effect.SubEffects.Add(buffEffect);
            }

            int duration;
            if (int.TryParse(TrimPercent(GetValue(tokens, "duration")), out duration))
                effect.IntegerParams[EffectIntParams.Duration] = duration;

            int torchDecrease;
            if (int.TryParse(TrimPercent(GetValue(tokens, "torch_decrease")), out torchDecrease))
                effect.IntegerParams[EffectIntParams.Torch] = -torchDecrease;

            int torchIncrease;
            if (int.TryParse(TrimPercent(GetValue(tokens, "torch_increase")), out torchIncrease))
            {
                int existing = effect.IntegerParams[EffectIntParams.Torch].HasValue
                    ? effect.IntegerParams[EffectIntParams.Torch].Value
                    : 0;
                effect.IntegerParams[EffectIntParams.Torch] = existing + torchIncrease;
            }

            string setMode = GetValue(tokens, "set_mode");
            if (setMode != null)
                effect.SubEffects.Add(new SetModeEffect(setMode));

            bool onMiss;
            if (bool.TryParse(GetValue(tokens, "on_miss"), out onMiss))
                effect.BooleanParams[EffectBoolParams.OnMiss] = onMiss;

            bool queue;
            if (bool.TryParse(GetValue(tokens, "queue"), out queue))
                effect.BooleanParams[EffectBoolParams.Queue] = queue;

            bool applyOnce;
            if (bool.TryParse(GetValue(tokens, "apply_once"), out applyOnce))
                effect.BooleanParams[EffectBoolParams.ApplyOnce] = applyOnce;

            return effect.SubEffects.Count == 0 && !effect.IntegerParams[EffectIntParams.Torch].HasValue
                ? null
                : effect;
        }

        /// <summary>Parses the raw DSL text into key/value tokens.</summary>
        /// <param name="part">The text after the <c>effect:</c> prefix.</param>
        /// <returns>The token dictionary.</returns>
        public static Dictionary<string, string> ParseTokens(string part)
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

        private static readonly Dictionary<string, EffectTargetType> TargetTypes =
            new Dictionary<string, EffectTargetType>(StringComparer.Ordinal)
            {
                { "performer", EffectTargetType.Performer },
                { "global", EffectTargetType.Global },
                { "performer_group_other", EffectTargetType.PerformersOther },
                { "performer_other_random", EffectTargetType.PerformersOther },
                { "performer_other", EffectTargetType.PerformersOther },
                { "target_group", EffectTargetType.TargetGroup },
                { "performer_group", EffectTargetType.TargetGroup },
            };

        private static EffectTargetType MapTarget(string target)
        {
            EffectTargetType mapped;
            if (target != null && TargetTypes.TryGetValue(target, out mapped))
                return mapped;
            return EffectTargetType.Target;
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

        private static string TrimPercent(string value)
        {
            return value == null ? null : value.TrimEnd('%');
        }

        private static void TryAddFraction(Dictionary<string, string> tokens, string key, Dictionary<AttributeType, float> target, AttributeType attribute)
        {
            float value;
            if (float.TryParse(TrimPercent(GetValue(tokens, key)), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                target[attribute] = value / 100f;
        }

        private static void TryAddFlat(Dictionary<string, string> tokens, string key, Dictionary<AttributeType, float> target, AttributeType attribute)
        {
            float value;
            if (float.TryParse(GetValue(tokens, key), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                target[attribute] = value;
        }

        private static void CopyInto(Dictionary<AttributeType, float> source, Dictionary<AttributeType, float> target)
        {
            foreach (var pair in source)
                target[pair.Key] = pair.Value;
        }

        private static MonsterType StringToMonsterType(string value)
        {
            switch (value)
            {
                case "unholy": return MonsterType.Unholy;
                case "man": return MonsterType.Man;
                case "beast": return MonsterType.Beast;
                case "eldritch": return MonsterType.Eldritch;
                case "corpse": return MonsterType.Corpse;
                default: return MonsterType.None;
            }
        }
    }
}