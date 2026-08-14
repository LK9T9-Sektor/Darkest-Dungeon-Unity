using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>
    /// Raw party name data as loaded from the content file.
    /// </summary>
    public class JsonPartyNameDictionary
    {
        /// <summary>Gets the available party names.</summary>
        public List<JsonPartyNameEntry> party_names { get; set; }
    }
}
