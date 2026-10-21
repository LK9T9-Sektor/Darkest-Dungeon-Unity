using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.ViewModels;
using Sektor.DarkestDungeon.Wpf.Views;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the reusable skill square icon sizing.</summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SkillSquareViewTests
    {
        /// <summary>The default icon size keeps the legacy 50 px tray button and 58 px root.</summary>
        [Test]
        public void DefaultIconSize_KeepsLegacyTraySize()
        {
            EnsureApplication();
            var view = new SkillSquareView();

            Assert.That(Button(view).Width, Is.EqualTo(50));
            Assert.That(Button(view).Height, Is.EqualTo(50));
            Assert.That(Root(view).Width, Is.EqualTo(58));
        }

        /// <summary>A 72 px icon size renders the bundled 72x72 art 1:1 inside the 8 px frame.</summary>
        [Test]
        public void IconSize72_AddsFrameAroundTheIcon()
        {
            EnsureApplication();
            var view = new SkillSquareView { IconSize = 72 };

            Assert.That(Button(view).Width, Is.EqualTo(80));
            Assert.That(Button(view).Height, Is.EqualTo(80));
            Assert.That(Root(view).Width, Is.EqualTo(88));
        }

        /// <summary>An undersized value is clamped instead of producing a negative button.</summary>
        [Test]
        public void IconSize_BelowMinimum_IsClamped()
        {
            EnsureApplication();
            var view = new SkillSquareView { IconSize = 1 };

            Assert.That(Button(view).Width, Is.EqualTo(24));
            Assert.That(Root(view).Width, Is.EqualTo(32));
        }

        /// <summary>Clickable tiles follow IsUsable changes raised by the lobby view model.</summary>
        [Test]
        public void IsUsable_Change_UpdatesButtonEnabledState()
        {
            EnsureApplication();
            var heroClass = DuelClasses.Get("crusader");
            Assert.That(heroClass, Is.Not.Null);
            var skill = new LobbySkillViewModel(heroClass!.CombatSkills[0], isActive: false, icon: null)
            {
                IsUsable = false,
            };
            var view = new SkillSquareView
            {
                DataContext = skill,
                SelectCommand = new StubCommand(),
            };
            view.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            Assert.That(Button(view).IsEnabled, Is.False);

            skill.IsUsable = true;

            Assert.That(Button(view).IsEnabled, Is.True);
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

        private static Button Button(SkillSquareView view) => (Button)view.FindName("SkillButton");

        private static StackPanel Root(SkillSquareView view) => (StackPanel)view.FindName("RootPanel");

        private static void EnsureApplication()
        {
            if (Application.Current != null)
                return;

            var app = new App();
            app.InitializeComponent();
        }
    }
}
