namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Base of a character attribute with raw value, flat addition and multiplier.</summary>
    public abstract class BaseAttribute
    {
        /// <summary>Gets the current modified base value.</summary>
        protected float ModifiedBaseValue;

        private float rawValue;
        private float flatAddition;
        private float multiplier;

        /// <summary>Gets or sets the raw base value.</summary>
        public float RawValue
        {
            get { return rawValue; }
            set
            {
                rawValue = value;
                IsModificationCurrent = false;
            }
        }

        /// <summary>Gets or sets the flat addition.</summary>
        public float FlatAddition
        {
            get { return flatAddition; }
            set
            {
                flatAddition = value;
                IsModificationCurrent = false;
            }
        }

        /// <summary>Gets or sets the multiplier.</summary>
        public float Multiplier
        {
            get { return multiplier; }
            set
            {
                multiplier = value;
                IsModificationCurrent = false;
            }
        }

        /// <summary>Gets a value indicating whether the modified value is up to date.</summary>
        protected bool IsModificationCurrent;

        /// <summary>Initializes a new instance of the <see cref="BaseAttribute"/> class.</summary>
        /// <param name="initialValue">The raw base value.</param>
        protected BaseAttribute(float initialValue)
        {
            rawValue = initialValue;
            flatAddition = 0;
            multiplier = 1;
            ModifiedBaseValue = rawValue;

            IsModificationCurrent = true;
        }
    }
}