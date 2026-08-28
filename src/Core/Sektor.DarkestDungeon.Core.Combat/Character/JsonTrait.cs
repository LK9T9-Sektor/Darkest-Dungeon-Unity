using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Raw trait entry from the JsonTraits.json content file.</summary>
    public class JsonTrait
    {
        /// <summary>Gets or sets the trait identifier.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets the overstress type ("affliction" or "virtue").</summary>
        public string overstress_type { get; set; }

        /// <summary>Gets or sets the buff ids applied while the trait is active.</summary>
        public List<string> buff_ids { get; set; }
    }
}