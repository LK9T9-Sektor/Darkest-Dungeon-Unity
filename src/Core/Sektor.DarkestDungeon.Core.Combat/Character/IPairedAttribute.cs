namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of a paired attribute (current + max value).</summary>
    public interface IPairedAttribute : IAttribute
    {
        /// <summary>Gets or sets the current value.</summary>
        float CurrentValue { get; set; }

        /// <summary>Gets or sets the current-to-max ratio.</summary>
        float ValueRatio { get; set; }

        /// <summary>Increases the current value.</summary>
        /// <param name="amount">The amount to increase.</param>
        void IncreaseValue(float amount);

        /// <summary>Decreases the current value.</summary>
        /// <param name="amount">The amount to decrease.</param>
        void DecreaseValue(float amount);
    }
}