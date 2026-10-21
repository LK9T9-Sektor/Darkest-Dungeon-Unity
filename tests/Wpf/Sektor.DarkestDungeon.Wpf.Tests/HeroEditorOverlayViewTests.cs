using System.Threading;
using System.Windows;
using System.Windows.Controls;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.ViewModels;
using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the reusable hero loadout overlay actions.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HeroEditorOverlayViewTests
    {
        /// <summary>The full-window host opens the editor and collapses again on close.</summary>
        [Test]
        public void Host_OpensEditorAndClosesOnButton()
        {
            EnsureApplication();
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);
            var host = new LobbyHeroEditorHost();

            host.Open(slot);

            var editor = (HeroEditorOverlayView)host.FindName("Editor");
            Assert.That(host.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(editor.DataContext, Is.SameAs(slot));

            var close = (Button)editor.FindName("CloseButton");
            close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.That(host.Visibility, Is.EqualTo(Visibility.Collapsed));
        }

        /// <summary>The close button raises the host-facing close event.</summary>
        [Test]
        public void CloseButton_RaisesCloseRequested()
        {
            EnsureApplication();
            var view = new HeroEditorOverlayView
            {
                DataContext = new HeroSlotViewModel(1, DuelClasses.AllClassIds),
            };
            bool closed = false;
            view.CloseRequested += (_, _) => closed = true;

            Layout(view);
            var close = (Button)view.FindName("CloseButton");
            close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.That(closed, Is.True);
        }

        private static void Layout(FrameworkElement view)
        {
            var size = new Size(800, 600);
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
