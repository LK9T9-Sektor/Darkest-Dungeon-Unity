using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>
    /// Raw heirloom exchange data as loaded from the content file. Member names mirror the
    /// legacy snake_case JSON keys so that any Newtonsoft.Json version can deserialize
    /// without attributes (Newtonsoft 13.0.x is incompatible with the Unity 2017.4 compiler).
    /// </summary>
    public class JsonHeirloomExchange
    {
        /// <summary>Gets the available exchange markets.</summary>
        public List<JsonHeirLoomMarket> markets { get; set; }
    }
}
