using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>Root document of Data\Upgrades\*\*.upgrades.json.</summary>
    public class JsonUpgrades
    {
        /// <summary>Gets or sets the upgrade trees.</summary>
        public List<JsonUpgradeTree> trees { get; set; }
    }
}