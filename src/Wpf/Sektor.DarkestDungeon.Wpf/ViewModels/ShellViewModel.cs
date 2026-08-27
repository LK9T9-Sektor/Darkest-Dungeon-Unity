using CommunityToolkit.Mvvm.ComponentModel;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Shell owning the currently displayed screen.</summary>
    public partial class ShellViewModel : ObservableObject, INavigationService
    {
        private object? homeScreen;

        /// <summary>Gets or sets the currently displayed screen view model.</summary>
        [ObservableProperty]
        private object? _currentScreen;

        /// <inheritdoc/>
        public void NavigateTo(object screen)
        {
            CurrentScreen = screen;
        }

        /// <summary>Marks the given screen as the home screen.</summary>
        /// <param name="screen">The home screen view model.</param>
        public void SetHome(object screen)
        {
            homeScreen = screen;
        }

        /// <inheritdoc/>
        public void GoHome()
        {
            if (homeScreen != null)
                CurrentScreen = homeScreen;
        }
    }
}
