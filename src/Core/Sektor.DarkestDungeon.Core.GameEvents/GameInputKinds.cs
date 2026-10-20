namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Wire values of <see cref="GameInput.Kind"/>.</summary>
    public static class GameInputKinds
    {
        /// <summary>The player clicked or selected something.</summary>
        public const string PlayerClick = "playerClick";

        /// <summary>An AI made the decision.</summary>
        public const string AiDecision = "aiDecision";

        /// <summary>The player moved the party.</summary>
        public const string Move = "move";

        /// <summary>The player passed the turn.</summary>
        public const string Pass = "pass";

        /// <summary>The player retreated from battle.</summary>
        public const string Retreat = "retreat";

        /// <summary>A timer/tick caused it.</summary>
        public const string Tick = "tick";

        /// <summary>The game system caused it (no direct actor).</summary>
        public const string System = "system";
    }
}
