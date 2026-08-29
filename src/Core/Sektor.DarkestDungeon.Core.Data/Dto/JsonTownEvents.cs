using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>Root document of Data\Mechanics\TownEvents.json.</summary>
    public class JsonTownEvents
    {
        /// <summary>Gets or sets the town event settings.</summary>
        public List<JsonTownEventSetting> settings { get; set; }

        /// <summary>Gets or sets the town event definitions.</summary>
        public List<JsonTownEvent> events { get; set; }
    }
}