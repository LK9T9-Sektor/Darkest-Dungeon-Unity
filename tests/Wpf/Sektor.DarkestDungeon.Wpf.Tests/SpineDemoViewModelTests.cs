using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the Spine demo screen state.</summary>
    [TestFixture]
    public class SpineDemoViewModelTests
    {
        /// <summary>The screenshot result reports the saved file name.</summary>
        [Test]
        public void ScreenshotSavedCommand_WithPath_ShowsFileName()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog());

            viewModel.ScreenshotSavedCommand.Execute(@"C:\shots\screenshot-2026-10-19_12-00-00-000.png");

            Assert.That(viewModel.ScreenshotStatus, Is.EqualTo("Saved: screenshot-2026-10-19_12-00-00-000.png"));
        }

        /// <summary>A failed capture reports the failure instead of a path.</summary>
        [Test]
        public void ScreenshotSavedCommand_WithoutPath_ReportsFailure()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog());

            viewModel.ScreenshotSavedCommand.Execute(null);

            Assert.That(viewModel.ScreenshotStatus, Is.EqualTo("Screenshot failed"));
        }

        /// <summary>The screen starts on the default hero with its idle set selected.</summary>
        [Test]
        public void Constructor_SelectsDefaultHeroAndIdleAnimation()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog());

            Assert.That(viewModel.CurrentCharacter, Is.EqualTo("Crusader"));
            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Idle"));
            Assert.That(viewModel.ContentDirectory, Is.EqualTo(Path.Combine("root", "Crusader", "Idle")));
            Assert.That(viewModel.ScreenTitle, Is.EqualTo("SPINE TEST \u2014 CRUSADER"));
        }

        /// <summary>Hero cycling moves forward, backward and wraps around the ends.</summary>
        [Test]
        public void CharacterCommands_CycleWithWrap()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog());

            viewModel.NextCharacterCommand.Execute(null);
            Assert.That(viewModel.CurrentCharacter, Is.EqualTo("Hellion"));

            viewModel.NextCharacterCommand.Execute(null);
            viewModel.NextCharacterCommand.Execute(null);
            Assert.That(viewModel.CurrentCharacter, Is.EqualTo("Crusader"));

            viewModel.PreviousCharacterCommand.Execute(null);
            Assert.That(viewModel.CurrentCharacter, Is.EqualTo("Vestal"));
        }

        /// <summary>Animation cycling wraps and the idle set is the first one.</summary>
        [Test]
        public void AnimationCommands_CycleWithWrap()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog());

            viewModel.NextAnimationCommand.Execute(null);
            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Walk"));

            viewModel.NextAnimationCommand.Execute(null);
            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Idle"));

            viewModel.PreviousAnimationCommand.Execute(null);
            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Walk"));
        }

        /// <summary>A known animation survives a hero switch; an unknown one resets to idle.</summary>
        [Test]
        public void CharacterSwitch_KeepsKnownAnimation_ResetsUnknownOne()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog());

            viewModel.NextAnimationCommand.Execute(null);
            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Walk"));

            viewModel.NextCharacterCommand.Execute(null);
            Assert.That(viewModel.CurrentCharacter, Is.EqualTo("Hellion"));
            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Walk"));

            viewModel.NextCharacterCommand.Execute(null);
            Assert.That(viewModel.CurrentCharacter, Is.EqualTo("Vestal"));
            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Idle"));
        }

        /// <summary>Speed grows by the step up to ten times and wraps back to normal speed.</summary>
        [Test]
        public void SpeedCommands_StepAndWrap()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog());
            Assert.That(viewModel.SpeedText, Is.EqualTo("100%"));

            for (int i = 0; i < 18; i++)
                viewModel.IncreaseSpeedCommand.Execute(null);

            Assert.That(viewModel.SpeedMultiplier, Is.EqualTo(10.0).Within(1e-9));
            Assert.That(viewModel.SpeedText, Is.EqualTo("1000%"));

            viewModel.IncreaseSpeedCommand.Execute(null);
            Assert.That(viewModel.SpeedMultiplier, Is.EqualTo(1.0).Within(1e-9));

            viewModel.DecreaseSpeedCommand.Execute(null);
            Assert.That(viewModel.SpeedMultiplier, Is.EqualTo(10.0).Within(1e-9));
        }

        private sealed class FakeSpineDemoCatalog : ISpineDemoCatalog
        {
            private static readonly List<string> Heroes = new List<string> { "Crusader", "Hellion", "Vestal" };

            private static readonly Dictionary<string, List<string>> Animations = new Dictionary<string, List<string>>
            {
                { "Crusader", new List<string> { "Idle", "Walk" } },
                { "Hellion", new List<string> { "Idle", "Walk" } },
                { "Vestal", new List<string> { "Idle", "Pray" } },
            };

            public IReadOnlyList<string> GetCharacters() => Heroes;

            public IReadOnlyList<string> GetAnimations(string character)
                => Animations.TryGetValue(character, out var animations) ? animations : new List<string>();

            public string GetContentDirectory(string character, string animation)
                => Path.Combine("root", character, animation);
        }

        private sealed class FakeNavigationService : INavigationService
        {
            public void NavigateTo(object screen)
            {
            }

            public void GoHome()
            {
            }
        }
    }
}
