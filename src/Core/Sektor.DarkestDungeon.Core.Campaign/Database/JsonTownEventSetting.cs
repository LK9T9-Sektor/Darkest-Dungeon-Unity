using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Campaign.Database
{
    /// <summary>A town event frequency setting.</summary>
    public class JsonTownEventSetting
    {
        /// <summary>Gets or sets the setting id.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets the event chance per town visit.</summary>
        public List<double> event_chance_per_town_visits { get; set; }
    }
}