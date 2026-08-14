using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Raw narration data as loaded from the content file.</summary>
    public class JsonNarration
    {
        /// <summary>Gets the filters of the narration data.</summary>
        public List<string> filters { get; set; }

        /// <summary>Gets the narration entries.</summary>
        public List<JsonNarrationEntry> entries { get; set; }
    }
}
