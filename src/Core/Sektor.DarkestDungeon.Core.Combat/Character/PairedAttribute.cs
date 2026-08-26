namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>A paired attribute with current and max value (hit points, stress, ...).</summary>
    public class PairedAttribute : BaseAttribute
    {
        private bool PreservePercentage { get; set; }
        private float currentValue;

        /// <summary>Gets or sets the current value clamped to [0, modified max].</summary>
        public float CurrentValue
        {
            get
            {
                if (IsModificationCurrent)
                    return currentValue;
                else
                {
                    UpdateValue();
                    return currentValue;
                }
            }
            set
            {
                if (IsModificationCurrent)
                    currentValue = Clamp(value, 0, ModifiedBaseValue);
                else
                {
                    UpdateValue();
                    currentValue = Clamp(value, 0, ModifiedBaseValue);
                }
            }
        }

        /// <summary>Gets the modified max value.</summary>
        public float ModifiedValue
        {
            get
            {
                if (IsModificationCurrent)
                    return ModifiedBaseValue;
                else
                {
                    UpdateValue();
                    return ModifiedBaseValue;
                }
            }
        }

        /// <summary>Gets or sets the current-to-max ratio clamped to [0, 1].</summary>
        public float ValueRatio
        {
            get
            {
                if (!IsModificationCurrent)
                    UpdateValue();

                if (ModifiedBaseValue == 0)
                    return 0;

                return currentValue / ModifiedBaseValue;
            }
            set
            {
                if (!IsModificationCurrent)
                    UpdateValue();

                value = Clamp(value, 0, 1);
                currentValue = value * ModifiedBaseValue;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="PairedAttribute"/> class.</summary>
        public PairedAttribute() : base(0)
        {
            PreservePercentage = true;
        }

        /// <summary>Initializes a new instance of the <see cref="PairedAttribute"/> class.</summary>
        /// <param name="initialValue">The initial current value.</param>
        /// <param name="initialMaxValue">The initial max value.</param>
        /// <param name="preservePercentage">Whether the ratio is preserved on max changes.</param>
        public PairedAttribute(float initialValue, float initialMaxValue, bool preservePercentage) : base(initialMaxValue)
        {
            PreservePercentage = preservePercentage;
            currentValue = initialValue > initialMaxValue ? initialMaxValue : initialValue;
        }

        /// <summary>Increases the current value.</summary>
        /// <param name="amount">The amount to increase.</param>
        public void IncreaseValue(float amount)
        {
            if (!IsModificationCurrent)
                UpdateValue();

            currentValue = Clamp(currentValue + amount, 0, ModifiedBaseValue);
        }

        /// <summary>Decreases the current value.</summary>
        /// <param name="amount">The amount to decrease.</param>
        public void DecreaseValue(float amount)
        {
            if (!IsModificationCurrent)
                UpdateValue();

            currentValue = Clamp(currentValue - amount, 0, ModifiedBaseValue);
        }

        private void UpdateValue()
        {
            float newModifiedValue = (RawValue + FlatAddition) * Multiplier;
            if (ModifiedBaseValue == newModifiedValue)
                return;
            else if (newModifiedValue > ModifiedBaseValue)
            {
                if (PreservePercentage)
                {
                    float ratio;
                    if (ModifiedBaseValue == 0)
                        ratio = 1;
                    else
                        ratio = currentValue / ModifiedBaseValue;
                    ModifiedBaseValue = newModifiedValue;
                    currentValue = ModifiedBaseValue * ratio;
                }
                else
                    ModifiedBaseValue = newModifiedValue;
            }
            else
            {
                if (PreservePercentage)
                {
                    float ratio = currentValue / ModifiedBaseValue;
                    ModifiedBaseValue = newModifiedValue;
                    currentValue = ModifiedBaseValue * ratio;
                }
                else
                {
                    ModifiedBaseValue = newModifiedValue;
                    if (currentValue > ModifiedBaseValue)
                        currentValue = ModifiedBaseValue;
                }
            }
            IsModificationCurrent = true;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}