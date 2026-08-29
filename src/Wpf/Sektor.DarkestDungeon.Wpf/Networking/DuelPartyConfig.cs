using System;
using System.Collections.Generic;
using System.Linq;

namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>The hero party exchanged before a duel: class id, deterministic seed, active skill selection and quirks per slot.</summary>
    public class DuelPartyConfig
    {
        /// <summary>Gets the class ids (4 slots).</summary>
        public IReadOnlyList<string> ClassIds { get; }

        /// <summary>Gets the per-hero seeds (4 slots).</summary>
        public IReadOnlyList<int> Seeds { get; }

        /// <summary>Gets the per-hero active combat skill ids (empty for a slot means all class skills).</summary>
        public IReadOnlyList<IReadOnlyList<string>> SelectedSkillIds { get; }

        /// <summary>Gets the per-hero quirk ids.</summary>
        public IReadOnlyList<IReadOnlyList<string>> QuirkIds { get; }

        /// <summary>Initializes a new instance of the <see cref="DuelPartyConfig"/> class.</summary>
        /// <param name="classIds">The class ids.</param>
        /// <param name="seeds">The seeds.</param>
        /// <param name="selectedSkillIds">The per-slot active combat skill ids.</param>
        /// <param name="quirkIds">The per-slot quirk ids.</param>
        public DuelPartyConfig(IReadOnlyList<string> classIds, IReadOnlyList<int> seeds, IReadOnlyList<IReadOnlyList<string>>? selectedSkillIds = null, IReadOnlyList<IReadOnlyList<string>>? quirkIds = null)
        {
            ClassIds = classIds;
            Seeds = seeds;
            SelectedSkillIds = selectedSkillIds ?? classIds.Select(_ => Array.Empty<string>() as IReadOnlyList<string>).ToList();
            QuirkIds = quirkIds ?? classIds.Select(_ => Array.Empty<string>() as IReadOnlyList<string>).ToList();
        }

        /// <summary>Serializes the config into the wire text format ("class|seed|skills|quirks").</summary>
        /// <returns>The wire text.</returns>
        public string Serialize()
        {
            var slots = new List<string>();
            for (int i = 0; i < ClassIds.Count; i++)
            {
                string skills = i < SelectedSkillIds.Count ? string.Join(",", SelectedSkillIds[i]) : string.Empty;
                string quirks = i < QuirkIds.Count ? string.Join(",", QuirkIds[i]) : string.Empty;
                slots.Add(ClassIds[i] + "|" + Seeds[i] + "|" + skills + "|" + quirks);
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
            var quirkIds = new List<IReadOnlyList<string>>();
            foreach (var slot in slots)
            {
                var parts = slot.Split('|');
                if (parts.Length < 2)
                    continue;
                classIds.Add(parts[0]);
                seeds.Add(int.Parse(parts[1]));
                skillIds.Add(parts.Length >= 3 && parts[2].Length > 0 ? parts[2].Split(',') : Array.Empty<string>());
                quirkIds.Add(parts.Length >= 4 && parts[3].Length > 0 ? parts[3].Split(',') : Array.Empty<string>());
            }
            return new DuelPartyConfig(classIds, seeds, skillIds, quirkIds);
        }
    }
}