using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>
    /// Catalog of effect definitions parsed from the Data/Mechanics/Effects file (the legacy
    /// <c>effect:</c> DSL). Parsing is partial: only the stress keys are loaded for now
    /// (<c>.stress</c> → <see cref="StressEffect"/>, <c>.healstress</c> → <see cref="StressHealEffect"/>);
    /// the rest of the effect types are ignored until the full parser lands (see PLAN.md Phase 4).
    /// </summary>
    public sealed class EffectCatalog
    {
        private readonly Dictionary<string, Effect> effectsById =
            new Dictionary<string, Effect>(StringComparer.Ordinal);

        /// <summary>Gets the number of parsed effects.</summary>
        public int Count { get { return effectsById.Count; } }

        /// <summary>Gets an effect by its name, or null when unknown.</summary>
        /// <param name="id">The effect name.</param>
        /// <returns>The effect or null.</returns>
        public Effect Get(string id)
        {
            Effect effect;
            return effectsById.TryGetValue(id, out effect) ? effect : null;
        }

        /// <summary>Parses the effects DSL content into a catalog.</summary>
        /// <param name="content">The effects file content.</param>
        /// <returns>The catalog.</returns>
        public static EffectCatalog Load(string content)
        {
            var catalog = new EffectCatalog();
            if (string.IsNullOrEmpty(content))
                return catalog;

            foreach (var rawLine in content.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
                    continue;

                int colon = line.IndexOf(':');
                if (colon <= 0)
                    continue;

                if (line.Substring(0, colon).Trim().ToLowerInvariant() != "effect")
                    continue;

                var tokens = ParseTokens(line.Substring(colon + 1));
                var effect = ParseEffect(tokens);
                if (effect != null && effect.Name.Length > 0 && !catalog.effectsById.ContainsKey(effect.Name))
                    catalog.effectsById.Add(effect.Name, effect);
            }

            return catalog;
        }

        private static Effect ParseEffect(Dictionary<string, string> tokens)
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

            return effect.SubEffects.Count == 0 ? null : effect;
        }

        private static EffectTargetType MapTarget(string target)
        {
            if (target == "performer")
                return EffectTargetType.Performer;
            if (target == "global")
                return EffectTargetType.Global;
            if (target == "performer_group_other" || target == "performer_other_random" || target == "performer_other")
                return EffectTargetType.PerformersOther;
            if (target == "target_group" || target == "performer_group")
                return EffectTargetType.TargetGroup;
            return EffectTargetType.Target;
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

                string first = string.Empty;
                if (i + 1 < pieces.Length && !pieces[i + 1].StartsWith(".", StringComparison.Ordinal))
                {
                    first = pieces[i + 1].Trim('"');
                    i++;
                }

                tokens[key] = first;
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

        private static string TrimPercent(string value)
        {
            return value == null ? null : value.TrimEnd('%');
        }
    }
}