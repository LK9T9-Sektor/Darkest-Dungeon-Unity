using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Common;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics
{
    /// <summary>Randomness engine for weighted selection and deterministic combat rolls.</summary>
    public static class RandomSolver
    {
        private static System.Random random = new System.Random();

        /// <summary>Chooses a random item excluding the given one.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="collection">The collection to choose from.</param>
        /// <param name="except">The item to exclude.</param>
        /// <returns>A random item that is not the excluded one.</returns>
        public static T ChooseAnyExcept<T>(IEnumerable<T> collection, T except) where T : class
        {
            var enumerable = collection as IList<T> ?? collection.ToList();
            var rnd = random.Next(enumerable.Sum(item => item == except ? 0 : 1));
            foreach (var item in enumerable)
            {
                if (item == except)
                    continue;
                if (rnd < 1)
                    return item;
                rnd -= 1;
            }
            return null;
        }

        /// <summary>Chooses a random item by proportional chance.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="collection">The collection to choose from.</param>
        /// <returns>A random item weighted by chance.</returns>
        public static T ChooseByRandom<T>(IEnumerable<T> collection) where T : IProportionValue
        {
            var enumerable = collection as IList<T> ?? collection.ToList();
            var rnd = random.Next(enumerable.Sum(item => item.Chance > 0 ? item.Chance : 0));
            foreach (var item in enumerable)
            {
                if (rnd < item.Chance)
                    return item;
                rnd -= item.Chance;
            }
            return default(T);
        }

        /// <summary>Chooses a random item by single proportion chance.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="collection">The collection to choose from.</param>
        /// <returns>A random item weighted by chance.</returns>
        public static T ChooseBySingleRandom<T>(IEnumerable<T> collection) where T : ISingleProportion
        {
            var enumerable = collection as IList<T> ?? collection.ToList();
            var rnd = random.NextDouble() * enumerable.Sum(item => item.Chance > 0 ? item.Chance : 0);
            foreach (var item in enumerable)
            {
                if (rnd < item.Chance)
                    return item;
                rnd -= item.Chance;
            }
            return default(T);
        }

        /// <summary>Chooses a random index by float chances.</summary>
        /// <param name="chances">The list of chances.</param>
        /// <returns>The chosen index.</returns>
        public static int ChooseRandomIndex(List<float> chances)
        {
            var rnd = random.NextDouble() * chances.Sum(item => item);
            for (int i = 0; i < chances.Count; i++)
            {
                if (rnd < chances[i])
                    return i;
                rnd -= chances[i];
            }
            return 0;
        }

        /// <summary>Chooses a random index by integer chances.</summary>
        /// <param name="chances">The list of chances.</param>
        /// <returns>The chosen index.</returns>
        public static int ChooseRandomIndex(List<int> chances)
        {
            var rnd = random.Next(0, chances.Sum(item => item));
            for (int i = 0; i < chances.Count; i++)
            {
                if (rnd < chances[i])
                    return i;
                rnd -= chances[i];
            }
            return 0;
        }

        /// <summary>Checks whether a chance succeeds.</summary>
        /// <param name="chance">The chance in [0, 1].</param>
        /// <returns>True if the chance succeeds.</returns>
        public static bool CheckSuccess(float chance)
        {
            if (chance >= 1)
                return true;

            return random.NextDouble() < chance;
        }

        /// <summary>Returns a non-negative random integer below the given maximum.</summary>
        /// <param name="maxValue">The exclusive upper bound.</param>
        /// <returns>A random integer.</returns>
        public static int Next(int maxValue)
        {
            return random.Next(maxValue);
        }

        /// <summary>Returns a random integer in the given range.</summary>
        /// <param name="minValue">The inclusive lower bound.</param>
        /// <param name="maxValue">The exclusive upper bound.</param>
        /// <returns>A random integer.</returns>
        public static int Next(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        /// <summary>Returns a random double in [0, 1).</summary>
        /// <returns>A random double.</returns>
        public static double NextDouble()
        {
            return random.NextDouble();
        }

        /// <summary>Seeds the random stream for deterministic reproduction.</summary>
        /// <param name="seed">The seed value.</param>
        public static void SetRandomSeed(int seed)
        {
            random = new System.Random(seed);
        }
    }
}
