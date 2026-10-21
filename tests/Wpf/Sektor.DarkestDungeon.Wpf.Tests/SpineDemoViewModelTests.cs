using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
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
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), new FakeHeroIconCatalog(), new FakeHeroSkillCatalog());

            viewModel.ScreenshotSavedCommand.Execute(@"C:\shots\screenshot-2026-10-19_12-00-00-000.png");

            Assert.That(viewModel.ScreenshotStatus, Is.EqualTo("Saved: screenshot-2026-10-19_12-00-00-000.png"));
        }

        /// <summary>A failed capture reports the failure instead of a path.</summary>
        [Test]
        public void ScreenshotSavedCommand_WithoutPath_ReportsFailure()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), new FakeHeroIconCatalog(), new FakeHeroSkillCatalog());

            viewModel.ScreenshotSavedCommand.Execute(null);

            Assert.That(viewModel.ScreenshotStatus, Is.EqualTo("Screenshot failed"));
        }

        /// <summary>The screen starts on the default hero with its idle set selected.</summary>
        [Test]
        public void Constructor_SelectsDefaultHeroAndIdleAnimation()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), new FakeHeroIconCatalog(), new FakeHeroSkillCatalog());

            Assert.That(viewModel.CurrentCharacter, Is.EqualTo("Crusader"));
            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Idle"));
            Assert.That(viewModel.ContentDirectory, Is.EqualTo(Path.Combine("root", "Crusader", "Idle")));
            Assert.That(viewModel.ScreenTitle, Is.EqualTo("SPINE TEST \u2014 CRUSADER"));
        }

        /// <summary>Hero cycling moves forward, backward and wraps around the ends.</summary>
        [Test]
        public void CharacterCommands_CycleWithWrap()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), new FakeHeroIconCatalog(), new FakeHeroSkillCatalog());

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
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), new FakeHeroIconCatalog(), new FakeHeroSkillCatalog());

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
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), new FakeHeroIconCatalog(), new FakeHeroSkillCatalog());

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
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), new FakeHeroIconCatalog(), new FakeHeroSkillCatalog());
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

        /// <summary>The portrait shown next to the selectors follows the selected hero.</summary>
        [Test]
        public void CurrentHeroPortrait_TracksSelectedHero()
        {
            var icons = new FakeHeroIconCatalog();
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), icons, new FakeHeroSkillCatalog());

            Assert.That(viewModel.CurrentHeroPortrait, Is.SameAs(icons.Portrait("crusader")));

            viewModel.NextCharacterCommand.Execute(null);

            Assert.That(viewModel.CurrentHeroPortrait, Is.SameAs(icons.Portrait("hellion")));
        }

        /// <summary>The hero card falls back to the placeholder text when no icon is bundled.</summary>
        [Test]
        public void CurrentHeroPortrait_WithoutBundledIcon_IsNull()
        {
            var viewModel = new SpineDemoViewModel(
                new FakeNavigationService(),
                new FakeSpineDemoCatalog(),
                new FakeHeroIconCatalog(),
                new FakeHeroSkillCatalog());

            viewModel.NextCharacterCommand.Execute(null);
            viewModel.NextCharacterCommand.Execute(null);

            Assert.That(viewModel.CurrentCharacter, Is.EqualTo("Vestal"));
            Assert.That(viewModel.CurrentHeroPortrait, Is.Null);
        }

        /// <summary>Switching the hero refreshes the icon and the readable name of the hero card.</summary>
        [Test]
        public void CharacterSwitch_NotifiesHeroCardBindings()
        {
            var viewModel = new SpineDemoViewModel(new FakeNavigationService(), new FakeSpineDemoCatalog(), new FakeHeroIconCatalog(), new FakeHeroSkillCatalog());
            var changed = new List<string?>();
            viewModel.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            viewModel.NextCharacterCommand.Execute(null);

            Assert.That(changed, Does.Contain(nameof(SpineDemoViewModel.CurrentHeroPortrait)));
            Assert.That(changed, Does.Contain(nameof(SpineDemoViewModel.CurrentHeroName)));
            Assert.That(viewModel.CurrentHeroName, Is.EqualTo("Hellion"));
        }

        /// <summary>The selected hero's combat skills are listed with their bundled icons.</summary>
        [Test]
        public void Skills_FollowSelectedHero()
        {
            var viewModel = CreateViewModel();

            Assert.That(viewModel.Skills, Has.Count.EqualTo(2));
            Assert.That(viewModel.Skills[0].Id, Is.EqualTo("smite"));
            Assert.That(viewModel.Skills[0].Icon, Is.Not.Null);
            Assert.That(viewModel.Skills[1].IsUsable, Is.True);

            viewModel.NextCharacterCommand.Execute(null);

            Assert.That(viewModel.Skills, Is.Empty);
        }

        /// <summary>Clicking a skill plays its effect set once and restores the previous animation on completion.</summary>
        [Test]
        public void SelectSkill_PlaysEffectOnceAndRestoresPreviousAnimation()
        {
            var viewModel = CreateViewModel();

            viewModel.SelectSkillCommand.Execute(viewModel.Skills[0]);

            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Smite"));
            Assert.That(viewModel.IsSkillPlaying, Is.True);
            Assert.That(viewModel.IsAnimationLooping, Is.False);
            Assert.That(viewModel.ContentDirectory, Is.EqualTo(Path.Combine("root", "Crusader", "Smite")));

            viewModel.SkillAnimationCompletedCommand.Execute("Smite");

            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("Idle"));
            Assert.That(viewModel.IsSkillPlaying, Is.False);
            Assert.That(viewModel.IsAnimationLooping, Is.True);
        }

        /// <summary>Without an effect set the skill shows its base pose and keeps looping.</summary>
        [Test]
        public void SelectSkill_WithoutEffectSet_PlaysBasePoseLooping()
        {
            var viewModel = CreateViewModel();

            viewModel.SelectSkillCommand.Execute(viewModel.Skills[1]);

            Assert.That(viewModel.CurrentAnimation, Is.EqualTo("BattleHeal"));
            Assert.That(viewModel.IsSkillPlaying, Is.False);
            Assert.That(viewModel.IsAnimationLooping, Is.True);
        }

        /// <summary>Only the clicked skill is marked selected.</summary>
        [Test]
        public void SelectSkill_MarksOnlyClickedSkill()
        {
            var viewModel = CreateViewModel();

            viewModel.SelectSkillCommand.Execute(viewModel.Skills[1]);

            Assert.That(viewModel.Skills[0].IsSelected, Is.False);
            Assert.That(viewModel.Skills[1].IsSelected, Is.True);
        }

        /// <summary>Changing the animation manually cancels the pending restore.</summary>
        [Test]
        public void ManualAnimationChange_DuringSkillPlayback_CancelsRestore()
        {
            var viewModel = CreateViewModel();
            viewModel.SelectSkillCommand.Execute(viewModel.Skills[0]);

            viewModel.NextAnimationCommand.Execute(null);

            Assert.That(viewModel.IsSkillPlaying, Is.False);
            Assert.That(viewModel.IsAnimationLooping, Is.True);
        }

        private static SpineDemoViewModel CreateViewModel()
            => new SpineDemoViewModel(
                new FakeNavigationService(),
                new FakeSpineDemoCatalog(),
                new FakeHeroIconCatalog(),
                new FakeHeroSkillCatalog());

        private sealed class FakeSpineDemoCatalog : ISpineDemoCatalog
        {
            private static readonly List<string> Heroes = new List<string> { "Crusader", "Hellion", "Vestal" };

            private static readonly Dictionary<string, List<string>> Animations = new Dictionary<string, List<string>>
            {
                { "Crusader", new List<string> { "Idle", "Walk" } },
                { "Hellion", new List<string> { "Idle", "Walk" } },
                { "Vestal", new List<string> { "Idle", "Pray" } },
            };

            private static readonly Dictionary<string, string> SkillSets = new Dictionary<string, string>
            {
                { "Crusader|smite", "Smite" },
                { "Crusader|attack_sword", "AttackSword" },
                { "Crusader|heal", "BattleHeal" },
            };

            public IReadOnlyList<string> GetCharacters() => Heroes;

            public IReadOnlyList<string> GetAnimations(string character)
                => Animations.TryGetValue(character, out var animations) ? animations : new List<string>();

            public string GetContentDirectory(string character, string animation)
                => Path.Combine("root", character, animation);

            public string? FindAnimationSet(string character, string animationId)
                => SkillSets.TryGetValue(character + "|" + animationId, out string? set) ? set : null;
        }

        private sealed class FakeHeroIconCatalog : IHeroIconCatalog
        {
            private readonly Dictionary<string, ImageSource> portraits = new Dictionary<string, ImageSource>
            {
                { "crusader", CreateImage() },
                { "hellion", CreateImage() },
            };

            public ImageSource Portrait(string classId) => portraits[classId];

            public ImageSource? GetPortrait(string classId, string outfit)
                => portraits.TryGetValue(classId, out ImageSource? image) ? image : null;

            public ImageSource? GetHeader(string classId)
                => portraits.TryGetValue(classId, out ImageSource? image) ? image : null;

            public ImageSource? GetAbilityIcon(string classId, string iconId)
                => portraits.TryGetValue(classId, out ImageSource? image) ? image : null;

            private static ImageSource CreateImage()
                => BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[4], 4);
        }

        private sealed class FakeHeroSkillCatalog : IHeroSkillCatalog
        {
            private static readonly ImageSource Icon =
                BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[4], 4);

            public IReadOnlyList<HeroSkillInfo> GetSkills(string classId)
            {
                if (classId != "crusader")
                    return new List<HeroSkillInfo>();

                return new List<HeroSkillInfo>
                {
                    new HeroSkillInfo(CreateSkill("smite"), CreateArt("smite", "attack_sword", "smite"), Icon),
                    new HeroSkillInfo(CreateSkill("battle_heal"), CreateArt("battle_heal", "heal", null), Icon),
                };
            }

            public ImageSource? GetSkillIcon(string classId, string skillId)
            {
                foreach (var info in GetSkills(classId))
                {
                    if (string.Equals(info.Skill.Id, skillId, System.StringComparison.Ordinal))
                        return info.Icon;
                }

                return null;
            }

            private static CombatSkill CreateSkill(string id) => new CombatSkill
            {
                Id = id,
                Level = 0,
                LaunchRanks = new FormationSet("12"),
                TargetRanks = new FormationSet("12"),
            };

            private static SkillArtInfo CreateArt(string id, string animation, string? effect)
            {
                var tokens = new Dictionary<string, string>
                {
                    { "id", id },
                    { "anim", animation },
                };
                if (effect != null)
                    tokens["fx"] = effect;
                return new SkillArtInfo(tokens);
            }
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
