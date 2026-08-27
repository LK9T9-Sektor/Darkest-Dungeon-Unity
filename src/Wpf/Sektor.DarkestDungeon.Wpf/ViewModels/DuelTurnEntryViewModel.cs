using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single entry of the current round's turn order strip.</summary>
    public partial class DuelTurnEntryViewModel : ObservableObject
    {
        /// <summary>Gets the unit display name.</summary>
        public string Name { get; }

        /// <summary>Gets a value indicating whether the entry belongs to the enemy side.</summary>
        public bool IsEnemy { get; }

        /// <summary>Gets a value indicating whether this entry is the currently acting unit.</summary>
        [ObservableProperty]
        private bool _isCurrent;

        /// <summary>Initializes a new instance of the <see cref="DuelTurnEntryViewModel"/> class.</summary>
        /// <param name="name">The unit display name.</param>
        /// <param name="isEnemy">Whether the unit belongs to the enemy side.</param>
        public DuelTurnEntryViewModel(string name, bool isEnemy)
        {
            Name = name;
            IsEnemy = isEnemy;
        }
    }
}
