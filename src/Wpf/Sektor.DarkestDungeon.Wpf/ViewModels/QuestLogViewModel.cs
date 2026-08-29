using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Quest log state shown in the top-left panel.</summary>
    public partial class QuestLogViewModel : ObservableObject
    {
        /// <summary>Gets or sets the quest title.</summary>
        [ObservableProperty]
        private string _title = "The Old Road";

        /// <summary>Gets or sets the quest goal text.</summary>
        [ObservableProperty]
        private string _goal = "Clear the first room and reach the exit.";
    }
}