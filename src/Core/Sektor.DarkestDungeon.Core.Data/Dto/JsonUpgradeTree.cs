using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>An upgrade tree (weapon, armour, skill...).</summary>
    public class JsonUpgradeTree
    {
        /// <summary>Gets or sets the tree id (class.slot).</summary>
        public string id { get; set; }

        /// <summary>Gets or sets whether the tree is instanced per hero.</summary>
        public bool is_instanced { get; set; }

        /// <summary>Gets or sets the tree tags.</summary>
        public List<string> tags { get; set; }

        /// <summary>Gets or sets the upgrade requirements by level code.</summary>
        public List<JsonUpgradeRequirement> requirements { get; set; }
    }
}