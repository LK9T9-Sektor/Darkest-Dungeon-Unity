using System;
using System.Collections.Generic;
using System.Linq;

namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>The hero party exchanged before a duel: class id + deterministic seed per slot.</summary>
    public class DuelPartyConfig
    {
        /// <summary>Gets the class ids (4 slots).</summary>
        public IReadOnlyList<string> ClassIds { get; }

        /// <summary>Gets the per-hero seeds (4 slots).</summary>
        public IReadOnlyList<int> Seeds { get; }

        /// <summary>Initializes a new instance of the <see cref="DuelPartyConfig"/> class.</summary>
        /// <param name="classIds">The class ids.</param>
        /// <param name="seeds">The seeds.</param>
        public DuelPartyConfig(IReadOnlyList<string> classIds, IReadOnlyList<int> seeds)
        {
            ClassIds = classIds;
            Seeds = seeds;
        }

        /// <summary>Serializes the config into the wire text format ("class|seed;class|seed;...").</summary>
        /// <returns>The wire text.</returns>
        public string Serialize()
        {
            var slots = new List<string>();
            for (int i = 0; i < ClassIds.Count; i++)
                slots.Add(ClassIds[i] + "|" + Seeds[i]);
            return string.Join(";", slots);
        }

        /// <summary>Parses a config from wire text.</summary>
        /// <param name="text">The wire text.</param>
        /// <returns>The parsed config.</returns>
        public static DuelPartyConfig Deserialize(string text)
        {
            var slots = text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var classIds = new List<string>();
            var seeds = new List<int>();
            foreach (var slot in slots)
            {
                var parts = slot.Split('|');
                if (parts.Length != 2)
                    continue;
                classIds.Add(parts[0]);
                seeds.Add(int.Parse(parts[1]));
            }
            return new DuelPartyConfig(classIds, seeds);
        }
    }
}