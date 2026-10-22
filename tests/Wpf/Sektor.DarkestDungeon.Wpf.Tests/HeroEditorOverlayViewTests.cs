using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Ui;
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

        /// <summary>The default 1280×800 window fits the table without a body scrollbar, and the
        /// close button is flush in the panel's top-right corner.</summary>
        [Test]
        public void DefaultWindow_LayoutFitsAndCloseIsFlush()
        {
            EnsureApplication();
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds, position: 1);
            var overlay = new HeroEditorOverlayView { DataContext = slot };
            var host = new Grid { Width = 1280, Height = 800 };
            host.Children.Add(overlay);
            host.Measure(new Size(1280, 800));
            host.Arrange(new Rect(0, 0, 1280, 800));
            host.UpdateLayout();

            var panel = (Border)overlay.FindName("EditorPanel");
            var close = (Button)overlay.FindName("CloseButton");
            var body = (ScrollViewer)overlay.FindName("BodyScroller");
            var level = (HeroLevelView)overlay.FindName("HeroLevel");
            var controls = (HeroCharacterControlsView)overlay.FindName("CharacterControls");
            var idleFallback = (Border)overlay.FindName("IdleFallback");
            var skills = (SkillScrollView)overlay.FindName("SkillsScroller");
            var camping = (SkillScrollView)overlay.FindName("CampingScroller");
            var diseases = (ItemsControl)overlay.FindName("DiseasesList");

            Assert.That(panel.ActualWidth, Is.EqualTo(1240).Within(1));
            Assert.That(panel.ActualHeight, Is.LessThanOrEqualTo(761));
            Point closeOrigin = close.TransformToAncestor(panel).Transform(new Point(0, 0));
            Assert.That(closeOrigin.Y, Is.EqualTo(12).Within(1),
                "the close button must sit below the rounded top border");
            Assert.That(closeOrigin.X + close.ActualWidth, Is.EqualTo(panel.ActualWidth - 2).Within(1),
                "the close button must touch the right edge");
            Assert.That(body.ScrollableHeight, Is.EqualTo(0).Within(1),
                "the default window must not show a body scrollbar");
            Assert.That(level.Level, Is.EqualTo(1));
            Assert.That(controls.SkinId, Is.EqualTo(1));
            Assert.That(controls.Position, Is.EqualTo(1));
            Point idleBottom = idleFallback.TransformToAncestor(body)
                .Transform(new Point(0, idleFallback.ActualHeight));
            Point controlsTop = controls.TransformToAncestor(body).Transform(new Point(0, 0));
            Assert.That(idleBottom.Y, Is.LessThanOrEqualTo(controlsTop.Y),
                "Idle must stay above the bottom portrait controls");
            Assert.That(diseases.Items.Count, Is.EqualTo(3),
                "the disease row always keeps three fixed slots");
            Assert.That(skills.ActualHeight, Is.LessThan(120),
                "all seven combat skills must fit in one row");
            Assert.That(camping.ActualHeight, Is.LessThan(100),
                "all seven camping skills must fit in one row");

            var equipment = (HeroEquipmentView)overlay.FindName("EquipmentSection");
            var trinkets = (HeroTrinketsEditorView)overlay.FindName("TrinketsSection");
            Assert.That(equipment.ActualWidth, Is.EqualTo(trinkets.ActualWidth).Within(1),
                "equipment and trinkets must use the same column width");
            Assert.That(equipment.ActualHeight, Is.EqualTo(trinkets.ActualHeight).Within(1),
                "equipment and trinkets must use the same panel height");

            Point equipmentBottom = equipment.TransformToAncestor(body)
                .Transform(new Point(0, equipment.ActualHeight));
            Point diseasesBottom = diseases.TransformToAncestor(body)
                .Transform(new Point(0, diseases.ActualHeight));
            Assert.That(equipmentBottom.Y, Is.EqualTo(body.ActualHeight).Within(8),
                "the equipment row must sit at the bottom of the sheet");
            Assert.That(diseasesBottom.Y, Is.EqualTo(body.ActualHeight).Within(8),
                "the diseases row must sit at the bottom of the sheet");

            string path = WindowScreenshot.Capture(host, Path.Combine(Path.GetTempPath(), "hero-editor-preview"));
            TestContext.WriteLine("Hero editor preview: " + path);
        }

        /// <summary>The minimum window still keeps all seven skills in one row.</summary>
        [Test]
        public void MinimumWindow_KeepsSevenSkillsInOneRow()
        {
            EnsureApplication();
            var overlay = new HeroEditorOverlayView
            {
                DataContext = new HeroSlotViewModel(1, DuelClasses.AllClassIds, position: 1),
            };
            var host = new Grid { Width = 1100, Height = 700 };
            host.Children.Add(overlay);
            host.Measure(new Size(1100, 700));
            host.Arrange(new Rect(0, 0, 1100, 700));
            host.UpdateLayout();

            var skills = (SkillScrollView)overlay.FindName("SkillsScroller");
            var camping = (SkillScrollView)overlay.FindName("CampingScroller");
            Assert.That(skills.ActualHeight, Is.LessThan(120),
                "seven combat skills must fit in one row at the minimum window size");
            Assert.That(camping.ActualHeight, Is.LessThan(100),
                "seven camping skills must fit in one row at the minimum window size");
        }

        /// <summary>A resolved Idle directory shows the Spine control and hides the portrait fallback.</summary>
        [Test]
        public void ResolvedIdle_ShowsSpineAndHidesFallback()
        {
            EnsureApplication();
            var spineCatalog = new SpineDemoCatalog(Path.Combine(AppContext.BaseDirectory, "Content", "Spine"));
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds, spineCatalog: spineCatalog);
            slot.AssignClass("abomination");
            var overlay = new HeroEditorOverlayView { DataContext = slot };

            Layout(overlay);

            var idle = (SpineHeroControl)overlay.FindName("IdleHero");
            var fallback = (Border)overlay.FindName("IdleFallback");
            Assert.That(idle.ContentDirectory, Does.EndWith(Path.Combine("Abomination", "Idle")));
            Assert.That(idle.AnimationName, Is.Empty,
                "the Idle content set supplies its default lowercase 'idle' animation");
            Assert.That(idle.Visibility, Is.EqualTo(Visibility.Visible));
            Assert.That(idle.ActualWidth, Is.GreaterThan(0));
            Assert.That(idle.ActualHeight, Is.EqualTo(360).Within(1));
            Assert.That(fallback.Visibility, Is.EqualTo(Visibility.Collapsed));
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
