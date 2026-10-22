using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Loads the bundled quirk status icons from <c>Content\Quirks</c>.</summary>
    public static class QuirkIconCatalog
    {
        private static readonly ImageSource? Lock = Load("lockquirk.png");

        /// <summary>Gets the gold lock icon shown for a locked quirk, or null when it is not delivered.</summary>
        public static ImageSource? LockIcon { get { return Lock; } }

        private static ImageSource? Load(string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Quirks", fileName);
            if (!File.Exists(path))
                return null;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(path, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch (Exception exception) when (exception is IOException
                or NotSupportedException
                or FileFormatException
                or ArgumentException)
            {
                return null;
            }
        }
    }
}
