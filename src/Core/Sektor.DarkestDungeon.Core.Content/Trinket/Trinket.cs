using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Trinket
{
    /// <summary>A hero trinket with its granted buffs and class restrictions.</summary>
    public sealed class Trinket
    {
        /// <summary>Initializes a new instance of the <see cref="Trinket"/> class.</summary>
        /// <param name="id">The trinket id.</param>
        /// <param name="buffIds">The buff ids granted while equipped.</param>
        /// <param name="heroClassRequirements">The required hero classes (empty = any).</param>
        /// <param name="rarity">The rarity tier.</param>
        /// <param name="price">The price.</param>
        /// <param name="limit">The equip limit.</param>
        /// <param name="originDungeon">The origin dungeon.</param>
        public Trinket(
            string id,
            IReadOnlyList<string> buffIds,
            IReadOnlyList<string> heroClassRequirements,
            string rarity,
            int price,
            int limit,
            string originDungeon)
        {
            Id = id;
            BuffIds = buffIds;
            HeroClassRequirements = heroClassRequirements;
            Rarity = rarity;
            Price = price;
            Limit = limit;
            OriginDungeon = originDungeon;
        }

        /// <summary>Gets the trinket id.</summary>
        public string Id { get; }

        /// <summary>Gets the buff ids granted while equipped.</summary>
        public IReadOnlyList<string> BuffIds { get; }

        /// <summary>Gets the required hero classes (empty = any).</summary>
        public IReadOnlyList<string> HeroClassRequirements { get; }

        /// <summary>Gets the rarity tier.</summary>
        public string Rarity { get; }

        /// <summary>Gets the price.</summary>
        public int Price { get; }

        /// <summary>Gets the equip limit.</summary>
        public int Limit { get; }

        /// <summary>Gets the origin dungeon.</summary>
        public string OriginDungeon { get; }
    }
}