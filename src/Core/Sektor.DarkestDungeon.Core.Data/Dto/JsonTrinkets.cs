using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>Root document of Data\JsonTrinkets.json.</summary>
    public class JsonTrinkets
    {
        /// <summary>Gets or sets the rarity tiers in drop order.</summary>
        public List<string> rarities { get; set; }

        /// <summary>Gets or sets the trinket definitions.</summary>
        public List<JsonTrinket> trinkets { get; set; }
    }
}