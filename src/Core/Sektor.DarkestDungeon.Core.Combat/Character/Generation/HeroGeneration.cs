using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Deterministic hero generation: same seed produces the same hero on both clients.</summary>
    public static class HeroGeneration
    {
        private static readonly string[] FirstNames =
        {
            "Reynauld", "Dismas", "Paracelsus", "Junia", "Alhazred",
            "Aram", "Baldwin", "Barristan", "Bonnie", "Boudica",
        };

        /// <summary>Generates a hero of the given class from a deterministic seed.</summary>
        /// <param name="heroClass">The hero class.</param>
        /// <param name="seed">The per-hero seed.</param>
        /// <returns>A new hero.</returns>
        public static Hero GenerateHero(HeroClass heroClass, int seed)
        {
            RandomSolver.SetRandomSeed(seed);
            string name = FirstNames[RandomSolver.Next(FirstNames.Length)];
            return new Hero(heroClass, 0, name);
        }
    }
}