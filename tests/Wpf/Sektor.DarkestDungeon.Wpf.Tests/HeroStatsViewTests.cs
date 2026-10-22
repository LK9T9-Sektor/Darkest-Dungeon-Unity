using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies that the shared detail headings follow the full-details flag.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HeroStatsViewTests
    {
        /// <summary>The SKILLS/RESISTANCES/QUIRKS headings are shown only in full-details mode.</summary>
        [Test]
        public void DetailHeadings_FollowShowFullDetails()
        {
            EnsureApplication();
            var view = new HeroStatsView { ShowFullDetails = true };

            Layout(view);

            List<SectionHeaderView> headings = FindChildren<SectionHeaderView>(view)
                .Where(heading => heading.Text == "SKILLS"
                    || heading.Text == "RESISTANCES"
                    || heading.Text == "QUIRKS")
                .ToList();

            Assert.That(headings.Count, Is.EqualTo(3));
            Assert.That(headings.All(heading => heading.Visibility == Visibility.Visible), Is.True);

            view.ShowFullDetails = false;
            Layout(view);

            Assert.That(headings.All(heading => heading.Visibility == Visibility.Collapsed), Is.True);
        }

        private static void Layout(FrameworkElement view)
        {
            var size = new Size(800, 1200);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();
        }

        private static IEnumerable<T> FindChildren<T>(DependencyObject root)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                    yield return match;

                foreach (T descendant in FindChildren<T>(child))
                    yield return descendant;
            }
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
