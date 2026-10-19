using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the visual 1x1 arena orchestration (attack one-shot and damage feedback).</summary>
    [TestFixture]
    public class SpineBattleViewModelTests
    {
        /// <summary>The screen starts with both slots on their idle sets.</summary>
        [Test]
        public void Constructor_LoadsIdleSetsForBothSlots()
        {
            var viewModel = CreateViewModel(out _);

            Assert.That(viewModel.AttackerContentDirectory, Is.EqualTo(Path.Combine("root", "Crusader", "Idle")));
            Assert.That(viewModel.TargetContentDirectory, Is.EqualTo(Path.Combine("root", "Highwayman", "Idle")));
            Assert.That(viewModel.AttackerLoop, Is.True);
            Assert.That(viewModel.IsAttacking, Is.False);
            Assert.That(viewModel.ScreenTitle, Is.EqualTo("АРЕНА 1×1"));
        }

        /// <summary>Attacking swaps the attacker to the pose set and starts the skill-effect overlay.</summary>
        [Test]
        public void AttackCommand_ShowsPoseAndStartsSkillOverlay()
        {
            var viewModel = CreateViewModel(out _);

            viewModel.AttackCommand.Execute(null);

            Assert.That(viewModel.AttackerContentDirectory, Is.EqualTo(Path.Combine("root", "Crusader", "AttackSword")));
            Assert.That(viewModel.AttackerOverlayContentDirectory, Is.EqualTo(Path.Combine("root", "Crusader", "Smite")));
            Assert.That(viewModel.IsAttacking, Is.True);
            Assert.That(viewModel.AttackCommand.CanExecute(null), Is.False);
        }

        /// <summary>The completed skill overlay returns the attacker to idle, clears the overlay and applies damage feedback.</summary>
        [Test]
        public void AttackerAnimationCompleted_DuringAttack_ShowsDamageAndResetsAttacker()
        {
            var viewModel = CreateViewModel(out var logger);

            viewModel.AttackCommand.Execute(null);
            viewModel.AttackerAnimationCompletedCommand.Execute("smite");

            Assert.That(viewModel.AttackerContentDirectory, Is.EqualTo(Path.Combine("root", "Crusader", "Idle")));
            Assert.That(viewModel.AttackerOverlayContentDirectory, Is.Empty);
            Assert.That(viewModel.TargetPopupVisible, Is.True);
            Assert.That(viewModel.TargetPopupText, Is.EqualTo(SpineBattleViewModel.DamagePopup));
            Assert.That(viewModel.TargetCardFlash, Is.EqualTo(SpineBattleViewModel.DamageFlash));
            Assert.That(viewModel.IsAttacking, Is.False);
            Assert.That(viewModel.AttackCommand.CanExecute(null), Is.True);

            Assert.That(logger.Messages, Has.Some.Contains("attack started"));
            Assert.That(logger.Messages, Has.Some.Contains("attack completed"));
        }

        /// <summary>A completion outside of an attack is ignored (no flash, no popup).</summary>
        [Test]
        public void AttackerAnimationCompleted_WithoutAttack_IsIgnored()
        {
            var viewModel = CreateViewModel(out _);

            viewModel.AttackerAnimationCompletedCommand.Execute("smite");

            Assert.That(viewModel.TargetPopupVisible, Is.False);
            Assert.That(viewModel.TargetCardFlash, Is.Empty);
            Assert.That(viewModel.IsAttacking, Is.False);
            Assert.That(viewModel.AttackerOverlayContentDirectory, Is.Empty);
        }

        private static SpineBattleViewModel CreateViewModel(out FakeLogger logger)
        {
            logger = new FakeLogger();
            return new SpineBattleViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), logger);
        }

        private sealed class FakeSpineDemoCatalog : ISpineDemoCatalog
        {
            public IReadOnlyList<string> GetCharacters() => new List<string> { "Crusader", "Highwayman" };

            public IReadOnlyList<string> GetAnimations(string character)
                => character == "Crusader"
                    ? new List<string> { "Idle", "AttackSword", "Smite" }
                    : new List<string> { "Idle" };

            public string GetContentDirectory(string character, string animation)
                => Path.Combine("root", character, animation);
        }

        private sealed class FakeLogger : ILogger
        {
            public List<string> Messages { get; } = new List<string>();

            public void Log(string message) => Messages.Add(message);

            public void Warn(string message) => Messages.Add("WARN " + message);
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
