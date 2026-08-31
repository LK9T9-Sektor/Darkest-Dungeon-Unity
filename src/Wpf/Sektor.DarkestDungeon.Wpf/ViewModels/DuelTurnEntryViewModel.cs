using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single entry of the current round's turn order strip.</summary>
    public partial class DuelTurnEntryViewModel : ObservableObject
    {
        /// <summary>Gets or sets the portrait image (null until one is provided).</summary>
        [ObservableProperty]
        private ImageSource? _portrait;
        /// <summary>Gets the unit display name.</summary>
        public string Name { get; }

        /// <summary>Gets a value indicating whether the entry belongs to the enemy side.</summary>
        public bool IsEnemy { get; }

        /// <summary>Gets the unit's real speed.</summary>
        public int Speed { get; }

        /// <summary>Gets the rolled initiative (speed + roll) that ordered this round's turns.</summary>
        public double InitiativeRoll { get; }

        /// <summary>Gets a value indicating whether this entry is the currently acting unit.</summary>
        [ObservableProperty]
        private bool _isCurrent;

        /// <summary>Gets a value indicating whether the unit is dead (the tile is dimmed in place,
        /// keeping the turn order strip free of layout jumps).</summary>
        [ObservableProperty]
        private bool _isDead;

        /// <summary>Initializes a new instance of the <see cref="DuelTurnEntryViewModel"/> class.</summary>
        /// <param name="name">The unit display name.</param>
        /// <param name="isEnemy">Whether the unit belongs to the enemy side.</param>
        /// <param name="speed">The unit's real speed.</param>
        /// <param name="initiativeRoll">The rolled initiative for this round.</param>
        public DuelTurnEntryViewModel(string name, bool isEnemy, int speed, double initiativeRoll)
        {
            Name = name;
            IsEnemy = isEnemy;
            Speed = speed;
            InitiativeRoll = initiativeRoll;
        }
    }
}