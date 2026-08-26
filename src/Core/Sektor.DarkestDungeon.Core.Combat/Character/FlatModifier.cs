using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>A flat (additive) modifier applied to a character attribute.</summary>
    public class FlatModifier
    {
        /// <summary>Gets or sets the target attribute type.</summary>
        public AttributeType TargetAttribute { get; set; }

        /// <summary>Gets or sets the modifier value.</summary>
        public float ModifierValue { get; set; }

        /// <summary>Gets or sets a value indicating whether the target is a paired attribute.</summary>
        public bool IsPaired { get; set; }

        /// <summary>Initializes a new instance of the <see cref="FlatModifier"/> class.</summary>
        /// <param name="attributeType">The target attribute type.</param>
        /// <param name="value">The modifier value.</param>
        /// <param name="isPaired">Whether the target is paired.</param>
        public FlatModifier(AttributeType attributeType, float value, bool isPaired)
        {
            TargetAttribute = attributeType;
            ModifierValue = value;
            IsPaired = isPaired;
        }

        /// <summary>Applies the modifier to a character.</summary>
        /// <param name="character">The character.</param>
        public void ApplyModifier(ICharacter character)
        {
            if (IsPaired)
                character.GetPairedAttribute(TargetAttribute).FlatAddition += ModifierValue;
            else
                character.GetSingleAttribute(TargetAttribute).FlatAddition += ModifierValue;
        }

        /// <summary>Reverts the modifier from a character.</summary>
        /// <param name="character">The character.</param>
        public void RevertModifier(ICharacter character)
        {
            if (IsPaired)
                character.GetPairedAttribute(TargetAttribute).FlatAddition -= ModifierValue;
            else
                character.GetSingleAttribute(TargetAttribute).FlatAddition -= ModifierValue;
        }
    }
}