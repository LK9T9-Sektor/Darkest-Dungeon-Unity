using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>Root document of Data\JsonQuests.json.</summary>
    public class JsonQuests
    {
        /// <summary>Gets or sets the stress damage multiplier on quest fail.</summary>
        public int stress_damage { get; set; }

        /// <summary>Gets or sets the quest goals.</summary>
        public List<JsonQuestGoal> goals { get; set; }

        /// <summary>Gets or sets the town progression goal ids.</summary>
        public List<string> town_progression_goal_ids { get; set; }

        /// <summary>Gets or sets the quest type definitions (opaque content data).</summary>
        public object types { get; set; }

        /// <summary>Gets or sets the plot quest definitions (opaque content data).</summary>
        public object plot_quests { get; set; }

        /// <summary>Gets or sets the quest generation settings (opaque content data).</summary>
        public object generation { get; set; }

        /// <summary>Gets or sets the quest restriction settings (opaque content data).</summary>
        public object restriction { get; set; }
    }
}