using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Quest log state and retreat action.</summary>
    public partial class QuestLogViewModel : ObservableObject
    {
        /// <summary>Gets the quest title placeholder.</summary>
        public string Title { get; } = "The Old Road";

        /// <summary>Gets the quest goal text placeholder.</summary>
        public string Goal { get; } = "Clear the first room and reach the exit.";

        /// <summary>Gets or sets a value indicating whether retreat is allowed.</summary>
        [ObservableProperty]
        private bool _canRetreat = true;

        /// <summary>Gets the retreat command (mock).</summary>
        public IRelayCommand RetreatCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="QuestLogViewModel"/> class.</summary>
        public QuestLogViewModel()
        {
            RetreatCommand = new RelayCommand(Retreat);
        }

        private void Retreat()
        {
        }
    }
}
