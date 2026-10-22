using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.ViewModels;
using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the bounded skill grid: two visible rows and a stable empty-row height.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SkillScrollViewTests
    {
        /// <summary>An empty list keeps one row of height instead of collapsing.</summary>
        [Test]
        public void EmptyItems_KeepOneRowHeight()
        {
            EnsureApplication();
            var view = new SkillScrollView
            {
                ItemsSource = new List<DuelSkillViewModel>(),
                IconSize = 56,
                ShowNames = false,
                ShowLevels = true,
                MaxRows = 2,
            };

            Layout(view);

            ScrollViewer scroller = (ScrollViewer)view.FindName("Scroller");
            Assert.That(scroller.MinHeight, Is.EqualTo(82).Within(0.01));
            Assert.That(scroller.ActualHeight, Is.GreaterThanOrEqualTo(82 - 0.01));
        }

        /// <summary>The row height follows the icon/name/level configuration.</summary>
        [Test]
        public void RowBounds_FollowConfiguration()
        {
            EnsureApplication();
            var view = new SkillScrollView
            {
                IconSize = 42,
                ShowNames = true,
                ShowLevels = true,
                MaxRows = 2,
            };

            Layout(view);

            ScrollViewer scroller = (ScrollViewer)view.FindName("Scroller");
            Assert.That(scroller.MinHeight, Is.EqualTo(90).Within(0.01));
            Assert.That(scroller.MaxHeight, Is.EqualTo(180).Within(0.01));
        }

        /// <summary>MaxRows below one is clamped to a single row.</summary>
        [Test]
        public void MaxRows_BelowOne_IsClamped()
        {
            EnsureApplication();
            var view = new SkillScrollView
            {
                IconSize = 56,
                ShowNames = false,
                ShowLevels = true,
                MaxRows = 0,
            };

            Layout(view);

            ScrollViewer scroller = (ScrollViewer)view.FindName("Scroller");
            Assert.That(scroller.MaxHeight, Is.EqualTo(scroller.MinHeight).Within(0.01));
        }

        private static void Layout(FrameworkElement view)
        {
            var size = new Size(800, 400);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();
        }

        private static void EnsureApplication()
        {
            if (Application.Current != null)
                return;

            var app = new App();
            app.InitializeComponent();
        }
    }
}
