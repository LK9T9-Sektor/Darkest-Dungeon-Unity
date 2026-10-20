namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Wire values of <see cref="AudioPlayed.Kind"/>.</summary>
    public static class AudioEventKinds
    {
        /// <summary>Music tracks.</summary>
        public const string Music = "music";

        /// <summary>Ambient loops.</summary>
        public const string Ambience = "ambience";

        /// <summary>Narration and voice-over.</summary>
        public const string Voice = "voice";

        /// <summary>Sound effects.</summary>
        public const string Sfx = "sfx";

        /// <summary>UI sounds.</summary>
        public const string Ui = "ui";

        /// <summary>Prop and curio sounds.</summary>
        public const string Props = "props";

        /// <summary>Camp sounds.</summary>
        public const string Camp = "camp";

        /// <summary>Town sounds.</summary>
        public const string Town = "town";
    }
}
