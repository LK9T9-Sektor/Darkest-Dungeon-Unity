using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        /// <summary>The class-cycling arrows are hidden by default and shown on demand.</summary>
        [Test]
        public void Arrows_AreHiddenByDefaultAndShownOnDemand()
        {
            var view = new HeroPortraitView();
            Layout(view);

            Button previous = (Button)view.FindName("PreviousButton");
            Button next = (Button)view.FindName("NextButton");
            Assert.That(previous.Visibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(next.Visibility, Is.EqualTo(Visibility.Collapsed));

            view.ShowArrows = true;
            Layout(view);

            Assert.That(previous.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(next.Visibility, Is.EqualTo(Visibility.Visible));
        }

        /// <summary>The portrait click is enabled only when the host supplies a command.</summary>
        [Test]
        public void PortraitClick_IsEnabledOnlyWithSelectCommand()
        {
            var view = new HeroPortraitView();
            Layout(view);

            Button portrait = (Button)view.FindName("PortraitButton");
            Assert.That(portrait.IsHitTestVisible, Is.False);
            Assert.That(portrait.Cursor, Is.EqualTo(Cursors.Arrow));

            view.SelectCommand = new StubCommand();
            Assert.That(portrait.IsHitTestVisible, Is.True);
            Assert.That(portrait.Cursor, Is.EqualTo(Cursors.Hand));
        }

        private sealed class StubCommand : ICommand
        {
#pragma warning disable CS0067
            public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter)
            {
            }
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
