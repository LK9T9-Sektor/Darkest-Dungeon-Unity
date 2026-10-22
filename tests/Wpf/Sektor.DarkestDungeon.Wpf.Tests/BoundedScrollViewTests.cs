using System.Threading;
using System.Windows;
using System.Windows.Controls;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the reusable visible-row cap used by bounded list sections.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class BoundedScrollViewTests
    {
        /// <summary>The maximum height is the row count multiplied by the row height.</summary>
        [Test]
        public void Bounds_AreDerivedFromRowsAndHeight()
        {
            var view = new BoundedScrollView { MaxRows = 4, RowHeight = 20 };

            Assert.That(view.MaxHeight, Is.EqualTo(80).Within(0.01));
        }

        /// <summary>Default rows reserve height even when the content is shorter.</summary>
        [Test]
        public void DefaultRows_ReserveHeight()
        {
            var view = new BoundedScrollView { MaxRows = 5, DefaultRows = 5, RowHeight = 20 };

            Assert.That(view.MinHeight, Is.EqualTo(100).Within(0.01));
            Assert.That(view.MaxHeight, Is.EqualTo(100).Within(0.01));
        }

        /// <summary>Tall content scrolls instead of stretching the section.</summary>
        [Test]
        public void TallContent_BecomesScrollable()
        {
            var view = new BoundedScrollView
            {
                MaxRows = 2,
                RowHeight = 20,
                Content = new Border { Height = 100 },
            };

            Layout(view);

            Assert.That(view.ScrollableHeight, Is.GreaterThan(0));
        }

        private static void Layout(FrameworkElement view)
        {
            var size = new Size(300, 500);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();
        }
    }
}
