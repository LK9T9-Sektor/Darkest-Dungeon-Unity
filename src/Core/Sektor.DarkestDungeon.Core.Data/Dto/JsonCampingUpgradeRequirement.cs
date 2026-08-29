using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>A camping skill upgrade requirement by level code.</summary>
    public class JsonCampingUpgradeRequirement
    {
        /// <summary>Gets or sets the level code.</summary>
        public string code { get; set; }

        /// <summary>Gets or sets the currency costs.</summary>
        public List<JsonCurrencyCost> currency_cost { get; set; }

        /// <summary>Gets or sets the prerequisite requirements (opaque content data).</summary>
        public List<Dictionary<string, object>> prerequisite_requirements { get; set; }
    }
}