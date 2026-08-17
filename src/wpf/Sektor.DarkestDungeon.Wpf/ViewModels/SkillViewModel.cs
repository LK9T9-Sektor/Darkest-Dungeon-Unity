using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A skill slot (combat, camping, move or pass).</summary>
    public partial class SkillViewModel : ObservableObject
    {
        /// <summary>Gets the display name of the skill.</summary>
        public string Name { get; }

        /// <summary>Gets or sets a value indicating whether the skill is usable in the current state.</summary>
        [ObservableProperty]
        private bool _isAvailable = true;

        /// <summary>Gets or sets a value indicating whether the skill is selected.</summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>Initializes a new instance of the <see cref="SkillViewModel"/> class.</summary>
        public SkillViewModel(string name)
        {
            Name = name;
        }
    }
}
