using System;
using System.Collections.Generic;
using System.IO;

using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Data.Catalogs;
using Sektor.DarkestDungeon.Core.Data.Readers;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Loads the hero quirk catalog from the bundled content file.</summary>
    public static class QuirkCatalog
    {
        private static readonly Core.Data.Catalogs.QuirkCatalog Inner = LoadInner();

        private static Core.Data.Catalogs.QuirkCatalog LoadInner()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Quirks", "JsonQuirks.json");
            if (!File.Exists(path))
                return Core.Data.Catalogs.QuirkCatalog.Empty;

            return GameDataReader.ReadQuirks(File.ReadAllText(path));
        }

        /// <summary>Gets the positive quirks.</summary>
        public static List<Quirk> Positive { get { return Inner.Positive; } }

        /// <summary>Gets the negative quirks.</summary>
        public static List<Quirk> Negative { get { return Inner.Negative; } }

        /// <summary>Gets a quirk by id, or null when the id is unknown.</summary>
        /// <param name="id">The quirk id.</param>
        /// <returns>The quirk or null.</returns>
        public static Quirk? Get(string id)
        {
            return Inner.Get(id);
        }
    }
}