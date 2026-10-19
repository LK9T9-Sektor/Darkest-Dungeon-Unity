using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Prototype screen playing the Crusader idle animation (Spine 2.1 + SkiaSharp).</summary>
    public partial class SpineDemoViewModel : ObservableObject
    {
        private readonly INavigationService navigation;

        /// <summary>Gets or sets a value indicating whether the animation clock is paused.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PauseButtonText))]
        private bool _isPaused;

        /// <summary>Gets or sets the status line shown next to the screenshot button.</summary>
        [ObservableProperty]
        private string _screenshotStatus = string.Empty;

        /// <summary>Gets the command returning to the main menu.</summary>
        public IRelayCommand BackCommand { get; }

        /// <summary>Gets the command toggling the pause state.</summary>
        public IRelayCommand TogglePauseCommand { get; }

        /// <summary>Gets the command reporting the screenshot result; the parameter is the saved path, or null on failure.</summary>
        public IRelayCommand<string?> ScreenshotSavedCommand { get; }

        /// <summary>Gets the text of the pause toggle button.</summary>
        public string PauseButtonText => IsPaused ? "Resume" : "Pause";

        /// <summary>Initializes a new instance of the <see cref="SpineDemoViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        public SpineDemoViewModel(INavigationService navigation)
        {
            this.navigation = navigation;
            BackCommand = new RelayCommand(navigation.GoHome);
            TogglePauseCommand = new RelayCommand(TogglePause);
            ScreenshotSavedCommand = new RelayCommand<string?>(ReportScreenshot);
        }

        private void TogglePause() => IsPaused = !IsPaused;

        private void ReportScreenshot(string? path) =>
            ScreenshotStatus = path == null ? "Screenshot failed" : "Saved: " + Path.GetFileName(path);
    }
}
