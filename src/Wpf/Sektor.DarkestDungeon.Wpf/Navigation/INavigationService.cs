namespace Sektor.DarkestDungeon.Wpf.Navigation
{
    /// <summary>Navigates the shell between screens.</summary>
    public interface INavigationService
    {
        /// <summary>Replaces the currently displayed screen.</summary>
        /// <param name="screen">The screen view model to display.</param>
        void NavigateTo(object screen);

        /// <summary>Returns to the home screen.</summary>
        void GoHome();
    }
}
