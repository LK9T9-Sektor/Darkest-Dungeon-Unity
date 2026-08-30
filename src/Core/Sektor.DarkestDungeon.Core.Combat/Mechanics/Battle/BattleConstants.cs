namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>
    /// Shared numeric constants of battle mechanics: chance clamps, surprise odds and default
    /// durations. Using named constants instead of magic numbers keeps balance tuning in one place.
    /// </summary>
    public static class BattleConstants
    {
        /// <summary>Maximum allowed hit/chance roll (0.95) — a hit can never be guaranteed above 95%.</summary>
        public const float MaxChance = 0.95f;

        /// <summary>Minimum accuracy floor for skills (0.1).</summary>
        public const float MinAccuracy = 0.1f;

        /// <summary>Critical damage multiplier.</summary>
        public const float CritMultiplier = 1.5f;

        /// <summary>Base surprise chance of a side (0.1).</summary>
        public const float BaseSurpriseChance = 0.1f;

        /// <summary>Upper bound of the surprise chance (0.65).</summary>
        public const float MaxSurpriseChance = 0.65f;

        /// <summary>Initiative penalty of a surprised side in the first round (-100).</summary>
        public const int SurprisedInitiativePenalty = -100;

        /// <summary>Default duration (in ticks) of a damage-over-time effect.</summary>
        public const int DefaultDotDuration = 3;

        /// <summary>Default mark duration (in turns).</summary>
        public const int DefaultMarkDuration = 3;

        /// <summary>Default riposte/guard duration (in rounds).</summary>
        public const int DefaultStatusDuration = 1;
    }
}