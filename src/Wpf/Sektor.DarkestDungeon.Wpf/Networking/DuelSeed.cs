using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>Deterministic session seed derived from ordered player ids (NETWORK.md §6).</summary>
    public static class DuelSeed
    {
        /// <summary>Computes the session seed from the ordered player ids.</summary>
        /// <param name="orderedPlayerIds">The player ids, local first.</param>
        /// <returns>The session seed.</returns>
        public static int ComputeSessionSeed(string[] orderedPlayerIds)
        {
            int sessionSeed = 0;
            foreach (var playerId in orderedPlayerIds)
            {
                RandomSolver.SetRandomSeed(StableHash(playerId));
                sessionSeed += RandomSolver.Next((int)System.Math.Pow(2, 16));
            }
            RandomSolver.SetRandomSeed(sessionSeed);
            return sessionSeed;
        }

        /// <summary>Computes a stable 32-bit hash of a string.</summary>
        /// <param name="text">The text.</param>
        /// <returns>The stable hash.</returns>
        public static int StableHash(string text)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in text)
                    hash = hash * 31 + c;
                return hash;
            }
        }
    }
}