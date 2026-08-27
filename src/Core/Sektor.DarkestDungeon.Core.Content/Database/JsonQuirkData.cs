using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Root object of the JsonQuirks.json content file.</summary>
    public class JsonQuirkData
    {
        /// <summary>Gets or sets the raw quirk entries.</summary>
        public List<JsonQuirk> quirks { get; set; }
    }
}