using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>
    /// File system catalog over the linked hero icons (<c>Content\HeroIcons\&lt;ClassId&gt;\portrait.&lt;Outfit&gt;.png</c>).
    /// Images are read once, frozen and cached; a missing or unreadable file yields <c>null</c> so a
    /// screen falls back to its placeholder instead of failing.
    /// </summary>
    public sealed class HeroIconCatalog : IHeroIconCatalog
    {
        private const string PortraitFileNamePrefix = "portrait.";
        private const string AbilityFileNamePrefix = "ability.";
        private const string HeaderFileName = "header.png";
        private const string ImageExtension = ".png";

        private readonly string rootDirectory;
        private readonly Dictionary<string, ImageSource?> cache = new Dictionary<string, ImageSource?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Initializes a new instance of the <see cref="HeroIconCatalog"/> class.</summary>
        /// <param name="rootDirectory">The hero icon root directory (contains one folder per class id).</param>
        public HeroIconCatalog(string rootDirectory)
        {
            this.rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        }

        /// <inheritdoc/>
        public ImageSource? GetPortrait(string classId, string outfit)
        {
            if (string.IsNullOrWhiteSpace(classId) || string.IsNullOrWhiteSpace(outfit))
                return null;

            string fileName = PortraitFileNamePrefix + outfit.Trim().ToUpperInvariant() + ImageExtension;
            return Load(Path.Combine(rootDirectory, classId.Trim(), fileName));
        }

        /// <inheritdoc/>
        public ImageSource? GetHeader(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
                return null;

            return Load(Path.Combine(rootDirectory, classId.Trim(), HeaderFileName));
        }

        /// <inheritdoc/>
        public ImageSource? GetAbilityIcon(string classId, string iconId)
        {
            if (string.IsNullOrWhiteSpace(classId) || string.IsNullOrWhiteSpace(iconId))
                return null;

            string fileName = AbilityFileNamePrefix + iconId.Trim().ToLowerInvariant() + ImageExtension;
            return Load(Path.Combine(rootDirectory, classId.Trim(), fileName));
        }

        private ImageSource? Load(string path)
        {
            ImageSource? cached;
            if (cache.TryGetValue(path, out cached))
                return cached;

            ImageSource? image = TryRead(path);
            cache[path] = image;
            return image;
        }

        private static ImageSource? TryRead(string path)
        {
            if (!File.Exists(path))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
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
