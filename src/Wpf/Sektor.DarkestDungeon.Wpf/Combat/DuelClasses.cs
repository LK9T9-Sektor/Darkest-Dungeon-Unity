using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Wpf.Combat
{
    /// <summary>Hero class source for duels, loaded from the bundled content files.</summary>
    public static class DuelClasses
    {
        /// <summary>Gets the effects catalog loaded from the bundled effects file (must precede the catalog).</summary>
        public static EffectCatalog Effects { get; } = LoadEffects();

        private static readonly HeroCatalog Catalog = LoadCatalog();

        /// <summary>Gets all known class ids in stable order.</summary>
        public static IReadOnlyList<string> AllClassIds { get { return Catalog.ClassIds; } }

        /// <summary>Gets a hero class by its string id.</summary>
        /// <param name="classId">The class id.</param>
        /// <returns>The hero class, or null if unknown.</returns>
        public static HeroClass? Get(string classId)
        {
            HeroClass heroClass;
            return Catalog.TryGet(classId, out heroClass) ? heroClass : null;
        }

        private static EffectCatalog LoadEffects()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Effects", "Effects.txt");
            return File.Exists(path) ? EffectCatalog.Load(File.ReadAllText(path)) : new EffectCatalog();
        }

        private static HeroCatalog LoadCatalog()
        {
            string directory = Path.Combine(AppContext.BaseDirectory, "Content", "Heroes");
            IEnumerable<string> contents = Directory.Exists(directory)
                ? Directory.GetFiles(directory, "*.bytes").OrderBy(path => path).Select(File.ReadAllText)
                : Enumerable.Empty<string>();
            return HeroCatalog.Load(contents, Effects);
        }
    }
}
