using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Captures a visual tree element into a timestamped PNG file.</summary>
    public static class WindowScreenshot
    {
        private const string DefaultFolderName = "screenshots";
        private const string FileNamePrefix = "screenshot-";
        private const string TimeStampFormat = "yyyy-MM-dd_HH-mm-ss-fff";

        /// <summary>Saves the current element content as a PNG and returns the full path of the created file.</summary>
        /// <param name="element">The target element, usually the application window.</param>
        /// <param name="directory">Target folder; when omitted, the "screenshots" folder inside the build output directory is used.</param>
        /// <returns>The full path of the saved PNG file.</returns>
        public static string Capture(FrameworkElement element, string? directory = null)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            string targetDirectory = directory ?? Path.Combine(AppContext.BaseDirectory, DefaultFolderName);
            Directory.CreateDirectory(targetDirectory);
            string path = Path.Combine(targetDirectory, FileNamePrefix + DateTime.Now.ToString(TimeStampFormat) + ".png");

            DpiScale dpi = VisualTreeHelper.GetDpi(element);
            var bitmap = new RenderTargetBitmap(
                (int)Math.Ceiling(element.ActualWidth * dpi.DpiScaleX),
                (int)Math.Ceiling(element.ActualHeight * dpi.DpiScaleY),
                96 * dpi.DpiScaleX,
                96 * dpi.DpiScaleY,
                PixelFormats.Pbgra32);
            bitmap.Render(element);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(path);
            encoder.Save(stream);
            return path;
        }
    }
}
