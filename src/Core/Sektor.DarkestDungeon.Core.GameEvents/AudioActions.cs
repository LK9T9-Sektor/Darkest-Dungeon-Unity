namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Wire values of <see cref="AudioPlayed.Action"/>.</summary>
    public static class AudioActions
    {
        /// <summary>A one-shot sound was played.</summary>
        public const string OneShot = "oneShot";

        /// <summary>A looping event was started.</summary>
        public const string Start = "start";

        /// <summary>A looping event was stopped.</summary>
        public const string Stop = "stop";

        /// <summary>A loop was paused.</summary>
        public const string Pause = "pause";

        /// <summary>A loop was resumed.</summary>
        public const string Resume = "resume";
    }
}
