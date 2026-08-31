using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Deterministic hero generation: the same class and seed produce the same hero on both clients.</summary>
    public static class HeroGeneration
    {
        private static readonly string[] FirstNames =
        {
            "Reynauld", "Dismas", "Paracelsus", "Junia", "Alhazred",
            "Aram", "Baldwin", "Barristan", "Bonnie", "Boudica",
        };

        private static readonly IReadOnlyDictionary<string, string> CanonicalNames =
            new Dictionary<string, string>
            {
                { "plague_doctor", "Paracelsus" },
                { "highwayman", "Dismas" },
                { "crusader", "Reynauld" },
                { "vestal", "Junia" },
                { "occultist", "Alhazred" },
                { "man_at_arms", "Barristan" },
                { "hellion", "Boudica" },
                { "leper", "Baldwin" },
                { "bounty_hunter", "Bonnie" },
                { "grave_robber", "Auda" },
                { "jester", "Sarmenti" },
                { "houndmaster", "William" },
                { "abomination", "Vincent" },
                { "arbalest", "Eleanor" },
                { "antiquarian", "Iona" },
            };

        /// <summary>Generates a hero of the given class from a deterministic seed.</summary>
        /// <param name="heroClass">The hero class.</param>
        /// <param name="seed">The per-hero seed.</param>
        /// <returns>A new hero.</returns>
        public static Hero GenerateHero(HeroClass heroClass, int seed)
        {
            RandomSolver.SetRandomSeed(seed);
            string name = FirstNames[RandomSolver.Next(FirstNames.Length)];
            CanonicalNames.TryGetValue(heroClass.StringId, out string canonicalName);
            return new Hero(heroClass, 0, canonicalName ?? name);
        }
    }
}