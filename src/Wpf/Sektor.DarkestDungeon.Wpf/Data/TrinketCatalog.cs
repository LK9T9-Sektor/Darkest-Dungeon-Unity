using System;
using System.IO;

using Sektor.DarkestDungeon.Clients.Content;
using Sektor.DarkestDungeon.Core.Content.Trinket;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Loads the trinket definitions from the bundled content file into core <see cref="Trinket"/> objects.</summary>
    public static class TrinketCatalog
    {
        private static readonly Sektor.DarkestDungeon.Core.Content.Trinket.TrinketCatalog Inner = LoadInner();

        private static Sektor.DarkestDungeon.Core.Content.Trinket.TrinketCatalog LoadInner()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Trinkets", "JsonTrinkets.json");
            return GameDataReader.ReadTrinketCatalog(File.Exists(path) ? File.ReadAllText(path) : string.Empty);
        }

        /// <summary>Gets a trinket by id, or null when the id is unknown.</summary>
        /// <param name="id">The trinket id.</param>
        /// <returns>The trinket or null.</returns>
        public static Trinket? Get(string id)
        {
            return Inner.Get(id);
        }

        /// <summary>Gets all trinkets ordered by their wire order.</summary>
        public static System.Collections.Generic.IReadOnlyCollection<Trinket> All
        {
            get { return Inner.All; }
        }
    }
}