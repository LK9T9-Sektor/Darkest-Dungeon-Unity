namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Wire values of <see cref="BattleEnded.Winner"/>.</summary>
    public static class BattleWinners
    {
        /// <summary>The hero party won.</summary>
        public const string Heroes = "heroes";

        /// <summary>The monster party won.</summary>
        public const string Monsters = "monsters";

        /// <summary>The winner could not be determined.</summary>
        public const string Unknown = "unknown";
    }
}
