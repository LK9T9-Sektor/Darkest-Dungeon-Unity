namespace Sektor.DarkestDungeon.Core.Content.Character
{
    /// <summary>A raw buff definition loaded from the JsonBuffs.json content file.</summary>
    public class BuffContent
    {
        /// <summary>Gets or sets the buff id.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the stat type token ("combat_stat_add"/"combat_stat_multiply").</summary>
        public string StatType { get; set; }

        /// <summary>Gets or sets the attribute token (e.g. "max_hp").</summary>
        public string AttributeTypeName { get; set; }

        /// <summary>Gets or sets the modifier amount.</summary>
        public float Amount { get; set; }

        /// <summary>Gets or sets a value indicating whether the buff is removed when inactive.</summary>
        public bool RemoveIfNotActive { get; set; }

        /// <summary>Gets or sets the rule token (e.g. "always").</summary>
        public string RuleTypeName { get; set; }

        /// <summary>Gets or sets a value indicating whether the rule is negated.</summary>
        public bool IsFalseRule { get; set; }

        /// <summary>Gets or sets the rule threshold.</summary>
        public float RuleFloat { get; set; }

        /// <summary>Gets or sets the rule string parameter.</summary>
        public string RuleString { get; set; }
    }
}