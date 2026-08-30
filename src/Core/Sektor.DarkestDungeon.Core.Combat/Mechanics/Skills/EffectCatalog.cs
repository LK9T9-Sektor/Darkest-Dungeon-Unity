using System;
using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>
    /// Catalog of effect definitions parsed from the Data/Mechanics/Effects file (the legacy
    /// <c>effect:</c> DSL). Parsing lives in <see cref="EffectParser"/>; this class only stores and
    /// resolves effects by name.
    /// </summary>
    public sealed class EffectCatalog
    {
        private readonly Dictionary<string, Effect> effectsById =
            new Dictionary<string, Effect>(StringComparer.OrdinalIgnoreCase);

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

                var effect = EffectParser.Parse(line.Substring(colon + 1));
                if (effect != null && effect.Name.Length > 0 && !catalog.effectsById.ContainsKey(effect.Name))
                    catalog.effectsById.Add(effect.Name, effect);
            }

            return catalog;
        }
    }
}