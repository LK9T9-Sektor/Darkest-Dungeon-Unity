namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of a character's stress meter.</summary>
    public interface IStress
    {
        /// <summary>Gets the current stress value.</summary>
        float CurrentValue { get; }

        /// <summary>Increases the stress value by the given amount.</summary>
        /// <param name="amount">The amount to increase.</param>
        void IncreaseValue(float amount);

        /// <summary>Decreases the stress value by the given amount.</summary>
        /// <param name="amount">The amount to decrease.</param>
        void DecreaseValue(float amount);
    }
}