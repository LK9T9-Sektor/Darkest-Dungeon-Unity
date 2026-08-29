using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>A single desire entry (skill/target/bonus) of a JsonAI.json monster brain.</summary>
    public class JsonSelectionDesire
    {
        /// <summary>Gets or sets the desire type key that selects its implementation.</summary>
        public string type { get; set; }

        /// <summary>Gets or sets the desire parameter data set.</summary>
        public Dictionary<string, object> data { get; set; }
    }
}