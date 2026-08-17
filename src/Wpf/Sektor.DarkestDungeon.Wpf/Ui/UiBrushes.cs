using System.Windows.Media;
using Sektor.DarkestDungeon.Core.Ui;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>
    /// Bridges the engine-free <see cref="UiStyle"/> tokens into WPF brushes so the shared core
    /// remains the single source of UI styling.
    /// </summary>
    public static class UiBrushes
    {
        /// <summary>Gets the shared label/text brush (golden).</summary>
        public static Brush Label { get; } = From(UiStyle.Label);

        /// <summary>Gets the default overlay panel background brush (almost opaque black).</summary>
        public static Brush PanelBackground { get; } = From(UiStyle.PanelBackground);

        /// <summary>Gets the neutral button background brush.</summary>
        public static Brush ButtonBackground { get; } = From(UiStyle.ButtonBackground);

        /// <summary>Converts a core color token into a frozen WPF solid brush.</summary>
        public static Brush From(ArgbColor color)
        {
            var brush = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            brush.Freeze();
            return brush;
        }
    }
}
