using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the in-app SCREENSHOT button helper.</summary>
    [TestFixture]
    public class WindowScreenshotTests
    {
        /// <summary>Capture must save a readable PNG whose content matches the target element size.</summary>
        [Test]
        public void Capture_SavesReadablePng_MatchingElementSize()
        {
            Exception? error = null;
            string? savedPath = null;
            int pixelWidth = 0;
            int pixelHeight = 0;
            string directory = Path.Combine(Path.GetTempPath(), "sektor-window-screenshot-tests");

            var thread = new Thread(() =>
            {
                try
                {
                    var surface = new Border { Width = 320, Height = 200, Background = Brushes.Teal };
                    surface.Measure(new Size(320, 200));
                    surface.Arrange(new Rect(0, 0, 320, 200));
                    surface.UpdateLayout();

                    savedPath = WindowScreenshot.Capture(surface, directory);

                    BitmapFrame frame = BitmapFrame.Create(new Uri(savedPath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    pixelWidth = frame.PixelWidth;
                    pixelHeight = frame.PixelHeight;
                }
                catch (Exception e)
                {
                    error = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error != null)
                throw new AssertionException("Screenshot capture failed: " + error);

            Assert.That(savedPath, Is.Not.Null);
            Assert.That(File.Exists(savedPath), Is.True);
            Assert.That(Path.GetFileName(savedPath!), Does.StartWith("screenshot-"));
            Assert.That(Path.GetExtension(savedPath!), Is.EqualTo(".png"));
            Assert.That(pixelWidth, Is.GreaterThanOrEqualTo(320));
            Assert.That(pixelHeight, Is.GreaterThanOrEqualTo(200));
            Assert.That(pixelWidth / (double)pixelHeight, Is.EqualTo(1.6).Within(0.05), "captured content must keep the element aspect ratio");
        }
    }
}
