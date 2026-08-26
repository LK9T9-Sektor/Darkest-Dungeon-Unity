using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A skill button in the live duel battle view.</summary>
    public partial class DuelSkillViewModel : ObservableObject
    {
        /// <summary>Gets the skill id.</summary>
        public string Id { get; }

        /// <summary>Gets the display name.</summary>
        public string DisplayName { get; }

        /// <summary>Gets or sets a value indicating whether the skill is usable.</summary>
        [ObservableProperty]
        private bool _isUsable;

        /// <summary>Gets or sets a value indicating whether the skill is selected.</summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>Initializes a new instance of the <see cref="DuelSkillViewModel"/> class.</summary>
        /// <param name="id">The skill id.</param>
        /// <param name="displayName">The display name.</param>
        public DuelSkillViewModel(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }
}