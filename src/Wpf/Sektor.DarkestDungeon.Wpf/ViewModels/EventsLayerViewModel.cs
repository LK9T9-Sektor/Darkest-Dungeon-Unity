using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Events overlay state: round number, announcement and popup message.</summary>
    public partial class EventsLayerViewModel : ObservableObject
    {
        /// <summary>Gets or sets the current round number.</summary>
        [ObservableProperty]
        private int _round = 1;

        /// <summary>Gets or sets the announcement title (empty hides the banner).</summary>
        [ObservableProperty]
        private string _announcementTitle = string.Empty;

        /// <summary>Gets or sets the transient popup message (empty hides it).</summary>
        [ObservableProperty]
        private string _popupMessage = string.Empty;
    }
}
