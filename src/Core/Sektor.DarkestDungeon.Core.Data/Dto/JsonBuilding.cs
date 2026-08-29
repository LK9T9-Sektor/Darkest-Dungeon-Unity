using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>
    /// Opaque building definition of Data\Buildings\*.building.json. Building activity blocks are
    /// deeply nested heterogeneous content data and are carried as loose dictionaries.
    /// </summary>
    public class JsonBuilding
    {
        /// <summary>Gets or sets the town visit priority.</summary>
        public int on_start_town_visit_priority { get; set; }

        /// <summary>Gets or sets the number of finished quests to unlock.</summary>
        public int number_of_quests_finished { get; set; }

        /// <summary>Gets or sets the highest dungeon level to unlock.</summary>
        public int highest_dungeon_level { get; set; }

        /// <summary>Gets or sets the remaining activity blocks keyed by activity name.</summary>
        public Dictionary<string, object> activities { get; set; }
    }
}