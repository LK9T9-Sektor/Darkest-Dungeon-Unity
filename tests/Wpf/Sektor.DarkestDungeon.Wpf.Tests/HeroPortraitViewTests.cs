using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the reusable hero card (portrait, placeholder and caption states).</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HeroPortraitViewTests
    {
        /// <summary>The card sizes its frame from PortraitSize and shows the caption under it.</summary>
        [Test]
        public void PortraitAndCaption_AreRendered()
        {
            var view = new HeroPortraitView
            {
                Portrait = CreateImage(),
                Caption = "Crusader",
                PortraitSize = 64,
            };

            Layout(view);

            Border frame = (Border)view.FindName("PortraitFrame");
            Assert.That(frame.ActualWidth, Is.EqualTo(64).Within(1));
            Assert.That(FindChildren<Image>(view).Single().Source, Is.Not.Null);
            Assert.That(Caption(view, "Crusader").Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(Caption(view, "?").Visibility, Is.EqualTo(Visibility.Collapsed));
        }

        /// <summary>A missing portrait falls back to the "?" placeholder.</summary>
        [Test]
        public void MissingPortrait_ShowsPlaceholder()
        {
            var view = new HeroPortraitView { Caption = "Man At Arms" };

            Layout(view);

            Assert.That(Caption(view, "?").Visibility, Is.EqualTo(Visibility.Visible));
        }

        /// <summary>An empty caption is hidden so the card can be used as a bare icon.</summary>
        [Test]
        public void EmptyCaption_IsHidden()
        {
            var view = new HeroPortraitView { Portrait = CreateImage() };

            Layout(view);

            Assert.That(FindChildren<TextBlock>(view).All(text => text.Visibility == Visibility.Collapsed), Is.True);
        }

        private static TextBlock Caption(HeroPortraitView view, string text)
            => FindChildren<TextBlock>(view).Single(block => block.Text == text);

        private static void Layout(FrameworkElement view)
        {
            var size = new Size(400, 400);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();
        }

        private static ImageSource CreateImage()
            => BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[4], 4);

        private static List<T> FindChildren<T>(DependencyObject parent)
            where T : DependencyObject
        {
            var result = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed)
                    result.Add(typed);
                result.AddRange(FindChildren<T>(child));
            }

            return result;
        }
    }
}
