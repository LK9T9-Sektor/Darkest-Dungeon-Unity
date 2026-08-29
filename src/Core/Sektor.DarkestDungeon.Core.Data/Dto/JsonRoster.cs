using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>Root document of Data\Mechanics\Roster.json.</summary>
    public class JsonRoster
    {
        /// <summary>Gets or sets the generated hero name format.</summary>
        public string name_id_format { get; set; }

        /// <summary>Gets or sets the resolve level thresholds.</summary>
        public List<int> resolve_level_thresholds { get; set; }

        /// <summary>Gets or sets the town progression idle stress heal.</summary>
        public Dictionary<string, object> town_visit_town_progression { get; set; }

        /// <summary>Gets or sets the non-town progression idle stress heal.</summary>
        public Dictionary<string, object> town_visit_non_town_progression { get; set; }
    }
}