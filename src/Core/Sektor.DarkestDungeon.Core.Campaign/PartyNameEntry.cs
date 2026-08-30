using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Campaign
{
    /// <summary>
    /// Describes a candidate party name together with the hero classes that unlock it.
    /// </summary>
    public class PartyNameEntry
    {
        /// <summary>Gets the unique identifier of the party name.</summary>
        public string Id { get; set; }

        /// <summary>Gets the hero classes required for the party name to be available.</summary>
        public List<string> ClassIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyNameEntry"/> class.
        /// </summary>
        public PartyNameEntry()
        {
            ClassIds = new List<string>();
        }
    }
}
