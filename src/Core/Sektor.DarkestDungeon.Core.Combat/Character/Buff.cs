using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>A stat modifier buff applied to a character.</summary>
    public class Buff
    {
        /// <summary>Gets or sets the buff identifier (empty for custom buffs).</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the modifier value.</summary>
        public float ModifierValue { get; set; }

        /// <summary>Gets or sets the buff type.</summary>
        public BuffType Type { get; set; }

        /// <summary>Gets or sets the affected attribute type.</summary>
        public AttributeType AttributeType { get; set; }

        /// <summary>Gets or sets the buff duration type.</summary>
        public BuffDurationType DurationType { get; set; }

        /// <summary>Gets or sets the duration amount.</summary>
        public int DurationAmount { get; set; }

        /// <summary>Gets or sets the activation rule.</summary>
        public BuffRule RuleType { get; set; }

        /// <summary>Gets or sets a value indicating whether the rule is inverted.</summary>
        public bool IsFalseRule { get; set; }

        /// <summary>Gets or sets a single parameter for the rule.</summary>
        public float SingleParam { get; set; }

        /// <summary>Gets or sets a string parameter for the rule.</summary>
        public string StringParam { get; set; }

        /// <summary>Initializes a new instance of the <see cref="Buff"/> class.</summary>
        public Buff()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Buff"/> class.</summary>
        /// <param name="buffType">The buff type.</param>
        /// <param name="rule">The activation rule.</param>
        /// <param name="attributeType">The affected attribute.</param>
        /// <param name="modifierValue">The modifier value.</param>
        public Buff(BuffType buffType, BuffRule rule, AttributeType attributeType, float modifierValue)
        {
            Id = "";
            Type = buffType;
            RuleType = rule;
            AttributeType = attributeType;
            ModifierValue = modifierValue;
        }

        /// <summary>Initializes a new instance of the <see cref="Buff"/> class.</summary>
        /// <param name="buffType">The buff type.</param>
        /// <param name="attributeType">The affected attribute.</param>
        /// <param name="modifierValue">The modifier value.</param>
        public Buff(BuffType buffType, AttributeType attributeType, float modifierValue) :
            this(buffType, BuffRule.Always, attributeType, modifierValue)
        {
        }

        /// <summary>Determines whether this buff is positive (a buff rather than a debuff).</summary>
        /// <returns>True if the buff is positive.</returns>
        public bool IsPositive()
        {
            if (AttributeType == AttributeType.StressDmgPercent || AttributeType == AttributeType.StressDmgReceivedPercent)
            {
                if (ModifierValue > 0)
                    return false;
                return true;
            }
            else if (ModifierValue >= 0)
                return true;
            return false;
        }

        /// <summary>Determines whether two buffs affect the same attribute under the same rule.</summary>
        /// <param name="buff">The buff to compare with.</param>
        /// <returns>True if both buffs are equivalent.</returns>
        public bool IsSameBuff(Buff buff)
        {
            return AttributeType == buff.AttributeType && RuleType == buff.RuleType && IsFalseRule == buff.IsFalseRule;
        }
    }
}