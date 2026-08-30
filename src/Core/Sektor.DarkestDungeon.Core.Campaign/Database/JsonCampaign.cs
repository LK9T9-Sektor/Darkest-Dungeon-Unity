using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>Root document of Data\Mechanics\Campaign.json.</summary>
    public class JsonCampaign
    {
        /// <summary>Gets or sets the quest completion xp table.</summary>
        public List<int> quest_completion_xp_table { get; set; }

        /// <summary>Gets or sets the dungeon level threshold table.</summary>
        public List<int> level_threshold_table { get; set; }

        /// <summary>Gets or sets the resolve level thresholds.</summary>
        public List<int> resolve_level_thresholds { get; set; }

        /// <summary>Gets or sets the gold icon thresholds.</summary>
        public List<int> gold_icon_thresholds { get; set; }

        /// <summary>Gets or sets the provision icon thresholds.</summary>
        public List<int> provision_icon_thresholds { get; set; }
    }
}