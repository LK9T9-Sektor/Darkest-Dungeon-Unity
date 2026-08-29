using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Root of the JsonTraits.json content file.</summary>
    public class JsonTraitData
    {
        /// <summary>Gets or sets the trait entries.</summary>
        public List<JsonTrait> traits { get; set; }
    }
}