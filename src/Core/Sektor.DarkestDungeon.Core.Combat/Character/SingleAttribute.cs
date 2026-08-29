namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>A single-value attribute (speed, accuracy, dodge, ...).</summary>
    public class SingleAttribute : BaseAttribute, IAttribute
    {
        /// <summary>Gets the modified value computed from raw, flat and multiplier.</summary>
        public float ModifiedValue
        {
            get
            {
                if (IsModificationCurrent)
                    return ModifiedBaseValue;
                else
                {
                    ModifiedBaseValue = (RawValue + FlatAddition) * Multiplier;
                    IsModificationCurrent = true;
                    return ModifiedBaseValue;
                }
            }
        }

        /// <summary>Initializes a new instance of the <see cref="SingleAttribute"/> class.</summary>
        public SingleAttribute() : base(0)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SingleAttribute"/> class.</summary>
        /// <param name="initialValue">The raw base value.</param>
        public SingleAttribute(float initialValue) : base(initialValue)
        {
        }
    }
}