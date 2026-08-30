using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>A quest goal definition.</summary>
    public class JsonQuestGoal
    {
        /// <summary>Gets or sets the goal id.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets the goal type (tutorial_room, kill_monster...).</summary>
        public string type { get; set; }

        /// <summary>Gets or sets the starting items.</summary>
        public List<Dictionary<string, object>> starting_items { get; set; }

        /// <summary>Gets or sets whether the fog of war is ignored.</summary>
        public bool ignore_fog_of_war { get; set; }

        /// <summary>Gets or sets whether the goal is shown as a quest.</summary>
        public bool show_as_quest { get; set; }

        /// <summary>Gets or sets the type-specific goal data (opaque content data).</summary>
        public Dictionary<string, object> data { get; set; }
    }
}