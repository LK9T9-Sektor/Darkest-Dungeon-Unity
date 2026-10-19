namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Wire values of <see cref="GameInput.Kind"/>.</summary>
    public static class GameInputKinds
    {
        /// <summary>The player clicked or selected something.</summary>
        public const string PlayerClick = "playerClick";

        /// <summary>An AI made the decision.</summary>
        public const string AiDecision = "aiDecision";

        /// <summary>A timer/tick caused it.</summary>
        public const string Tick = "tick";

        /// <summary>The game system caused it (no direct actor).</summary>
        public const string System = "system";
    }
}
