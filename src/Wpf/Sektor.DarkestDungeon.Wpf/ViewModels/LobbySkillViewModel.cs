using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A hero combat skill that can be toggled active in the lobby.</summary>
    public partial class LobbySkillViewModel : ObservableObject
    {
        /// <summary>Gets the skill id.</summary>
        public string Id { get; }

        /// <summary>Gets the display name.</summary>
        public string Name { get; }

        /// <summary>Gets or sets the tooltip details (damage, ranks).</summary>
        [ObservableProperty]
        private string _details = string.Empty;

        /// <summary>Gets or sets a value indicating whether the skill is active in battle.</summary>
        [ObservableProperty]
        private bool _isActive;

        /// <summary>Initializes a new instance of the <see cref="LobbySkillViewModel"/> class.</summary>
        /// <param name="id">The skill id.</param>
        /// <param name="isActive">Whether the skill starts active.</param>
        public LobbySkillViewModel(string id, bool isActive)
        {
            Id = id;
            Name = id;
            IsActive = isActive;
        }
    }
}