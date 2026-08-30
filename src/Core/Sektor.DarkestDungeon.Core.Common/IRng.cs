namespace Sektor.DarkestDungeon.Core.Common
{
    /// <summary>
    /// Deterministic random source used by pure core logic (generation, combat, duel). Implemented
    /// by the client boundary; the core never creates its own global random state.
    /// </summary>
    public interface IRng
    {
        /// <summary>Returns a non-negative random integer below the given maximum.</summary>
        /// <param name="maxValue">The exclusive upper bound.</param>
        /// <returns>A random integer.</returns>
        int Next(int maxValue);

        /// <summary>Returns a random integer in the given range.</summary>
        /// <param name="minValue">The inclusive lower bound.</param>
        /// <param name="maxValue">The exclusive upper bound.</param>
        /// <returns>A random integer.</returns>
        int Next(int minValue, int maxValue);

        /// <summary>Returns a random double in [0, 1).</summary>
        /// <returns>A random double.</returns>
        double NextDouble();
    }
}