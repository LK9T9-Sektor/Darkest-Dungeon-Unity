using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>A trinket definition.</summary>
    public class JsonTrinket
    {
        /// <summary>Gets or sets the trinket id.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets the buff ids granted while equipped.</summary>
        public List<string> buffs { get; set; }

        /// <summary>Gets or sets the required hero classes (empty = any).</summary>
        public List<string> hero_class_requirements { get; set; }

        /// <summary>Gets or sets the rarity tier.</summary>
        public string rarity { get; set; }

        /// <summary>Gets or sets the price.</summary>
        public int price { get; set; }

        /// <summary>Gets or sets the equip limit.</summary>
        public int limit { get; set; }

        /// <summary>Gets or sets the origin dungeon.</summary>
        public string origin_dungeon { get; set; }
    }
}