namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>Abstraction of torch manipulation for global effects.</summary>
    public interface ITorchHandler
    {
        /// <summary>Decreases the torch by the specified amount.</summary>
        /// <param name="amount">The amount to decrease.</param>
        void DecreaseTorch(int amount);

        /// <summary>Increases the torch by the specified amount.</summary>
        /// <param name="amount">The amount to increase.</param>
        void IncreaseTorch(int amount);
    }
}
