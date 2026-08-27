namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Raw buff entry from the JsonBuffs.json content file.</summary>
    public class JsonBuff
    {
        /// <summary>Gets or sets the buff id.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets the stat type token.</summary>
        public string stat_type { get; set; }

        /// <summary>Gets or sets the attribute token.</summary>
        public string stat_sub_type { get; set; }

        /// <summary>Gets or sets the modifier amount.</summary>
        public float amount { get; set; }

        /// <summary>Gets or sets a value indicating whether the buff is removed when inactive.</summary>
        public bool remove_if_not_active { get; set; }

        /// <summary>Gets or sets the rule token.</summary>
        public string rule_type { get; set; }

        /// <summary>Gets or sets a value indicating whether the rule is negated.</summary>
        public bool is_false_rule { get; set; }

        /// <summary>Gets or sets the rule parameters.</summary>
        public JsonBuffRuleData rule_data { get; set; }
    }
}