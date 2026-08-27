using System;
using System.Collections.Generic;
using System.Linq;

namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>The hero party exchanged before a duel: class id, deterministic seed and active skill selection per slot.</summary>
    public class DuelPartyConfig
    {
        /// <summary>Gets the class ids (4 slots).</summary>
        public IReadOnlyList<string> ClassIds { get; }

        /// <summary>Gets the per-hero seeds (4 slots).</summary>
        public IReadOnlyList<int> Seeds { get; }

        /// <summary>Gets the per-hero active combat skill ids (empty for a slot means all class skills).</summary>
        public IReadOnlyList<IReadOnlyList<string>> SelectedSkillIds { get; }

        /// <summary>Initializes a new instance of the <see cref="DuelPartyConfig"/> class.</summary>
        /// <param name="classIds">The class ids.</param>
        /// <param name="seeds">The seeds.</param>
        /// <param name="selectedSkillIds">The per-slot active combat skill ids.</param>
        public DuelPartyConfig(IReadOnlyList<string> classIds, IReadOnlyList<int> seeds, IReadOnlyList<IReadOnlyList<string>>? selectedSkillIds = null)
        {
            ClassIds = classIds;
            Seeds = seeds;
            SelectedSkillIds = selectedSkillIds ?? classIds.Select(_ => Array.Empty<string>() as IReadOnlyList<string>).ToList();
        }

        /// <summary>Serializes the config into the wire text format ("class|seed|s1,s2;class|seed;...").</summary>
        /// <returns>The wire text.</returns>
        public string Serialize()
        {
            var slots = new List<string>();
            for (int i = 0; i < ClassIds.Count; i++)
            {
                string slot = ClassIds[i] + "|" + Seeds[i];
                if (i < SelectedSkillIds.Count && SelectedSkillIds[i].Count > 0)
                    slot += "|" + string.Join(",", SelectedSkillIds[i]);
                slots.Add(slot);
            }
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
            var skillIds = new List<IReadOnlyList<string>>();
            foreach (var slot in slots)
            {
                var parts = slot.Split('|');
                if (parts.Length < 2)
                    continue;
                classIds.Add(parts[0]);
                seeds.Add(int.Parse(parts[1]));
                skillIds.Add(parts.Length > 2 ? parts[2].Split(',') : Array.Empty<string>());
            }
            return new DuelPartyConfig(classIds, seeds, skillIds);
        }
    }
}