using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>An upgrade level requirement.</summary>
    public class JsonUpgradeRequirement
    {
        /// <summary>Gets or sets the level code.</summary>
        public string code { get; set; }

        /// <summary>Gets or sets the currency costs.</summary>
        public List<JsonCurrencyCost> currency_cost { get; set; }

        /// <summary>Gets or sets the prerequisite tree requirements.</summary>
        public List<JsonPrerequisiteRequirement> prerequisite_requirements { get; set; }

        /// <summary>Gets or sets the required hero resolve level.</summary>
        public int prerequisite_resolve_level { get; set; }
    }
}