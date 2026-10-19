using System;
using System.Collections.Generic;
using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Spine demo screen: switches heroes, animation sets and playback speed of the rendered skeleton.</summary>
    public partial class SpineDemoViewModel : ObservableObject
    {
        private const string DefaultCharacter = "Crusader";
        private const double MinSpeedMultiplier = 1.0;
        private const double MaxSpeedMultiplier = 10.0;
        private const double SpeedStep = 0.5;

        private readonly INavigationService navigation;
        private readonly ISpineDemoCatalog catalog;

        /// <summary>Gets or sets a value indicating whether the animation clock is paused.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PauseButtonText))]
        private bool _isPaused;

        /// <summary>Gets or sets the status line shown next to the screenshot button.</summary>
        [ObservableProperty]
        private string _screenshotStatus = string.Empty;

        /// <summary>Gets or sets the selected hero.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ContentDirectory))]
        [NotifyPropertyChangedFor(nameof(ScreenTitle))]
        private string _currentCharacter = string.Empty;

        /// <summary>Gets the animation sets of the selected hero; the idle set comes first.</summary>
        [ObservableProperty]
        private IReadOnlyList<string> _animations = Array.Empty<string>();

        /// <summary>Gets or sets the selected animation set.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ContentDirectory))]
        private string _currentAnimation = string.Empty;

        /// <summary>Gets or sets the animation speed multiplier (1 = 100%, 10 = 1000%).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SpeedText))]
        private double _speedMultiplier = MinSpeedMultiplier;

        /// <summary>Gets the available hero names.</summary>
        public IReadOnlyList<string> Characters { get; }

        /// <summary>Gets the content directory of the selected animation set, as bound by the hero control.</summary>
        public string ContentDirectory => catalog.GetContentDirectory(CurrentCharacter, CurrentAnimation);

        /// <summary>Gets the screen title including the selected hero.</summary>
        public string ScreenTitle => $"SPINE TEST \u2014 {CurrentCharacter.ToUpperInvariant()}";

        /// <summary>Gets the text of the pause toggle button.</summary>
        public string PauseButtonText => IsPaused ? "Resume" : "Pause";

        /// <summary>Gets the playback speed shown as a percentage.</summary>
        public string SpeedText => $"{SpeedMultiplier * 100:0}%";

        /// <summary>Gets the command returning to the main menu.</summary>
        public IRelayCommand BackCommand { get; }

        /// <summary>Gets the command toggling the pause state.</summary>
        public IRelayCommand TogglePauseCommand { get; }

        /// <summary>Gets the command reporting the screenshot result; the parameter is the saved path, or null on failure.</summary>
        public IRelayCommand<string?> ScreenshotSavedCommand { get; }

        /// <summary>Gets the command selecting the previous hero (wraps around).</summary>
        public IRelayCommand PreviousCharacterCommand { get; }

        /// <summary>Gets the command selecting the next hero (wraps around).</summary>
        public IRelayCommand NextCharacterCommand { get; }

        /// <summary>Gets the command selecting the previous animation set (wraps around).</summary>
        public IRelayCommand PreviousAnimationCommand { get; }

        /// <summary>Gets the command selecting the next animation set (wraps around).</summary>
        public IRelayCommand NextAnimationCommand { get; }

        /// <summary>Gets the command decreasing the playback speed (wraps to the maximum).</summary>
        public IRelayCommand DecreaseSpeedCommand { get; }

        /// <summary>Gets the command increasing the playback speed (wraps to the minimum).</summary>
        public IRelayCommand IncreaseSpeedCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="SpineDemoViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="catalog">The catalog of heroes and animation sets to browse.</param>
        public SpineDemoViewModel(INavigationService navigation, ISpineDemoCatalog catalog)
        {
            this.navigation = navigation;
            this.catalog = catalog;
            Characters = catalog.GetCharacters();

            BackCommand = new RelayCommand(navigation.GoHome);
            TogglePauseCommand = new RelayCommand(TogglePause);
            ScreenshotSavedCommand = new RelayCommand<string?>(ReportScreenshot);
            PreviousCharacterCommand = new RelayCommand(() => SelectCharacter(-1));
            NextCharacterCommand = new RelayCommand(() => SelectCharacter(1));
            PreviousAnimationCommand = new RelayCommand(() => SelectAnimation(-1));
            NextAnimationCommand = new RelayCommand(() => SelectAnimation(1));
            DecreaseSpeedCommand = new RelayCommand(() => ShiftSpeed(-1));
            IncreaseSpeedCommand = new RelayCommand(() => ShiftSpeed(1));

            int initial = IndexOf(Characters, DefaultCharacter);
            CurrentCharacter = Characters.Count > 0 ? Characters[initial < 0 ? 0 : initial] : string.Empty;
        }

        partial void OnCurrentCharacterChanged(string value)
        {
            Animations = catalog.GetAnimations(value);
            if (IndexOf(Animations, CurrentAnimation) < 0)
                CurrentAnimation = Animations.Count > 0 ? Animations[0] : string.Empty;
        }

        private static int IndexOf(IReadOnlyList<string> items, string value)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (string.Equals(items[i], value, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        private void SelectCharacter(int delta)
        {
            if (Characters.Count == 0)
                return;

            int index = IndexOf(Characters, CurrentCharacter);
            index = index < 0 ? 0 : (index + delta + Characters.Count) % Characters.Count;
            CurrentCharacter = Characters[index];
        }

        private void SelectAnimation(int delta)
        {
            if (Animations.Count == 0)
                return;

            int index = IndexOf(Animations, CurrentAnimation);
            index = index < 0 ? 0 : (index + delta + Animations.Count) % Animations.Count;
            CurrentAnimation = Animations[index];
        }

        private void ShiftSpeed(int direction)
        {
            double shifted = SpeedMultiplier + (direction * SpeedStep);
            if (shifted > MaxSpeedMultiplier)
                shifted = MinSpeedMultiplier;
            else if (shifted < MinSpeedMultiplier)
                shifted = MaxSpeedMultiplier;

            SpeedMultiplier = shifted;
        }

        private void TogglePause() => IsPaused = !IsPaused;

        private void ReportScreenshot(string? path) =>
            ScreenshotStatus = path == null ? "Screenshot failed" : "Saved: " + Path.GetFileName(path);
    }
}
