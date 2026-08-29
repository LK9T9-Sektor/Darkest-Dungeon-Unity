using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Catalog of all parsed hero classes, keyed by class string id.</summary>
    public sealed class HeroCatalog
    {
        private readonly List<HeroClass> classes = new List<HeroClass>();
        private readonly Dictionary<string, HeroClass> classesById =
            new Dictionary<string, HeroClass>(StringComparer.Ordinal);

        private HeroCatalog()
        {
        }

        /// <summary>Gets the known class ids in file order (deterministic across machines).</summary>
        public IReadOnlyList<string> ClassIds
        {
            get
            {
                var ids = new List<string>(classesById.Count);
                foreach (var heroClass in classes)
                    ids.Add(heroClass.StringId);
                return ids;
            }
        }

        /// <summary>Attempts to get a hero class by its string id.</summary>
        /// <param name="classId">The class id.</param>
        /// <param name="heroClass">The found hero class.</param>
        /// <returns>True when found.</returns>
        public bool TryGet(string classId, out HeroClass heroClass)
        {
            return classesById.TryGetValue(classId ?? string.Empty, out heroClass);
        }

        /// <summary>Parses all files and builds a catalog; unparsable entries are skipped.</summary>
        /// <param name="fileContents">The text contents of hero definition files.</param>
        /// <param name="effects">The effects catalog used to resolve skill effects (optional).</param>
        /// <returns>The loaded catalog.</returns>
        public static HeroCatalog Load(IEnumerable<string> fileContents, EffectCatalog effects = null)
        {
            var catalog = new HeroCatalog();
            if (fileContents == null)
                return catalog;

            foreach (string content in fileContents)
            {
                var heroClass = HeroClassFileParser.Parse(content, effects);
                if (heroClass != null && !catalog.classesById.ContainsKey(heroClass.StringId))
                {
                    catalog.classes.Add(heroClass);
                    catalog.classesById.Add(heroClass.StringId, heroClass);
                }
            }
            return catalog;
        }
    }
}
