namespace Sektor.DarkestDungeon.Core.Ui
{
    /// <summary>
    /// Shared presentation tokens for the runtime-created UI overlays. Engine-free: colors are
    /// expressed as <see cref="ArgbColor"/> so both Unity projects (and any future client) can
    /// consume the same values; the Unity side converts them into a UnityEngine color value.
    /// </summary>
    public static class UiStyle
    {
        /// <summary>Gets the resources path of the font used by the runtime UI overlays.</summary>
        public const string FontResource = "Fonts/DwarvenAxe";

        /// <summary>Gets the small body text size used for hints and secondary labels.</summary>
        public const int Small = 20;

        /// <summary>Gets the primary body text size used for buttons and labels.</summary>
        public const int Body = 22;

        /// <summary>Gets the log line text size of the session log window.</summary>
        public const int LogBody = 20;

        /// <summary>Gets the panel/window title text size.</summary>
        public const int Title = 28;

        /// <summary>Gets the large title text size used by the provider menu.</summary>
        public const int LargeTitle = 34;

        /// <summary>Gets the provider row label text size.</summary>
        public const int RowLabel = 35;

        /// <summary>Gets the value/slider label text size of the settings window.</summary>
        public const int Value = 22;

        /// <summary>Gets the return button label text size of the settings window.</summary>
        public const int ReturnButton = 30;

        /// <summary>Gets the shared label/text color (golden).</summary>
        public static readonly ArgbColor Label = new ArgbColor(255, 255, 219, 119);

        /// <summary>Gets the default overlay panel background color (almost opaque black).</summary>
        public static readonly ArgbColor PanelBackground = new ArgbColor(242, 0, 0, 0);

        /// <summary>Gets the neutral button background color.</summary>
        public static readonly ArgbColor ButtonBackground = new ArgbColor(242, 51, 51, 51);

        /// <summary>Gets the background color of the selected provider row.</summary>
        public static readonly ArgbColor SelectedRow = new ArgbColor(242, 115, 97, 51);

        /// <summary>Gets the background color of the idle provider row.</summary>
        public static readonly ArgbColor IdleRow = new ArgbColor(153, 51, 51, 51);
    }
}
