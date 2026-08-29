using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Content.Trinket;
using Sektor.DarkestDungeon.Core.Content.Trinket;

namespace Sektor.DarkestDungeon.Core.Content.Trinket
{
    /// <summary>Loads trinket definitions from the campaign JsonTrinkets.json content into <see cref="Trinket"/> instances.</summary>
    public sealed class TrinketCatalog
    {
        private readonly Dictionary<string, Trinket> _byId = new Dictionary<string, Trinket>();

        private TrinketCatalog()
        {
        }

        /// <summary>Gets all trinkets ordered by their wire order.</summary>
        public IReadOnlyCollection<Trinket> All { get { return _byId.Values; } }

        /// <summary>Builds a trinket catalog from the parsed JsonTrinkets document.</summary>
        /// <param name="data">The parsed JsonTrinkets.json document.</param>
        /// <returns>The trinket catalog.</returns>
        public static TrinketCatalog Load(JsonTrinkets data)
        {
            var catalog = new TrinketCatalog();
            if (data?.trinkets == null)
                return catalog;

            foreach (JsonTrinket jsonTrinket in data.trinkets)
            {
                var trinket = new Trinket(
                    jsonTrinket.id,
                    CopyList(jsonTrinket.buffs),
                    CopyList(jsonTrinket.hero_class_requirements),
                    jsonTrinket.rarity,
                    jsonTrinket.price,
                    jsonTrinket.limit,
                    jsonTrinket.origin_dungeon ?? string.Empty);
                catalog._byId[trinket.Id] = trinket;
            }

            return catalog;
        }

        /// <summary>Gets a trinket by id, or null when the id is unknown.</summary>
        /// <param name="id">The trinket id.</param>
        /// <returns>The trinket or null.</returns>
        public Trinket Get(string id)
        {
            Trinket trinket;
            return _byId.TryGetValue(id, out trinket) ? trinket : null;
        }

        private static List<string> CopyList(List<string> source)
        {
            return source == null ? new List<string>() : new List<string>(source);
        }
    }
}