using System;

namespace Sektor.DarkestDungeon.Core.Common
{
    /// <summary>A deterministic <see cref="IRng"/> backed by a seeded <see cref="System.Random"/>.</summary>
    public sealed class SystemRandomRng : IRng
    {
        private readonly Random random;

        /// <summary>Initializes a new instance of the <see cref="SystemRandomRng"/> class.</summary>
        /// <param name="seed">The deterministic seed.</param>
        public SystemRandomRng(int seed)
        {
            random = new Random(seed);
        }

        /// <inheritdoc/>
        public int Next(int maxValue)
        {
            return random.Next(maxValue);
        }

        /// <inheritdoc/>
        public int Next(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        /// <inheritdoc/>
        public double NextDouble()
        {
            return random.NextDouble();
        }
    }
}