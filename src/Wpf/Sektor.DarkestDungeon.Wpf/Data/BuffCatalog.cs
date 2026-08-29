using System;
using System.IO;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Data.Catalogs;
using Sektor.DarkestDungeon.Core.Data.Readers;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Loads the buff definitions from the bundled content file into core <see cref="Buff"/> objects.</summary>
    public static class BuffCatalog
    {
        private static readonly Core.Data.Catalogs.BuffCatalog Inner = LoadInner();

        private static Core.Data.Catalogs.BuffCatalog LoadInner()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Buffs", "JsonBuffs.json");
            if (!File.Exists(path))
                return Core.Data.Catalogs.BuffCatalog.Empty;

            return GameDataReader.ReadBuffs(File.ReadAllText(path));
        }

        /// <summary>Gets a buff by id, or null when the id is unknown.</summary>
        /// <param name="id">The buff id.</param>
        /// <returns>The buff or null.</returns>
        public static Buff? Get(string id)
        {
            return Inner.Get(id);
        }
    }
}