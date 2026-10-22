using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the reusable section heading chrome and its text property.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SectionHeaderViewTests
    {
        /// <summary>The heading renders its text centered over a bottom border.</summary>
        [Test]
        public void Text_IsRenderedCenteredWithBottomBorder()
        {
            var view = new SectionHeaderView { Text = "QUIRKS" };

            Layout(view);

            TextBlock text = FindChildren<TextBlock>(view).Single();
            Border border = FindChildren<Border>(view).Single(item => item.BorderThickness.Bottom == 1);
            Assert.That(text.Text, Is.EqualTo("QUIRKS"));
            Assert.That(text.TextAlignment, Is.EqualTo(TextAlignment.Center));
            Assert.That(border.BorderThickness.Bottom, Is.EqualTo(1));
        }

        /// <summary>Changing Text updates the rendered caption.</summary>
        [Test]
        public void Text_Change_UpdatesCaption()
        {
            var view = new SectionHeaderView { Text = "SKILLS" };
            Layout(view);

            view.Text = "CAMPING SKILLS";

            Assert.That(FindChildren<TextBlock>(view).Single().Text, Is.EqualTo("CAMPING SKILLS"));
        }

        private static void Layout(FrameworkElement view)
        {
            var size = new Size(400, 100);
            view.Measure(size);
            view.Arrange(new Rect(size));
            view.UpdateLayout();
        }

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
