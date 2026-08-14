namespace Sektor.DarkestDungeon.Core.Ui
{
    /// <summary>
    /// Engine-free 32-bit color value (ARGB channels as bytes). Used by <see cref="UiStyle"/> so
    /// the shared presentation tokens stay usable from pure C#; Unity-side code converts it into
    /// a UnityEngine color value.
    /// </summary>
    public readonly struct ArgbColor
    {
        /// <summary>Gets the alpha channel (0-255).</summary>
        public byte A { get; }

        /// <summary>Gets the red channel (0-255).</summary>
        public byte R { get; }

        /// <summary>Gets the green channel (0-255).</summary>
        public byte G { get; }

        /// <summary>Gets the blue channel (0-255).</summary>
        public byte B { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgbColor"/> struct.
        /// </summary>
        /// <param name="a">The alpha channel (0-255).</param>
        /// <param name="r">The red channel (0-255).</param>
        /// <param name="g">The green channel (0-255).</param>
        /// <param name="b">The blue channel (0-255).</param>
        public ArgbColor(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Creates an <see cref="ArgbColor"/> from its ARGB channel bytes.
        /// </summary>
        /// <param name="a">The alpha channel (0-255).</param>
        /// <param name="r">The red channel (0-255).</param>
        /// <param name="g">The green channel (0-255).</param>
        /// <param name="b">The blue channel (0-255).</param>
        /// <returns>The constructed color value.</returns>
        public static ArgbColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new ArgbColor(a, r, g, b);
        }
    }
}
