using System.Collections.Generic;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A hero combat skill in a lobby: icon, structured tooltip and active/available state.</summary>
    public partial class LobbySkillViewModel : ObservableObject, ISkillTileViewModel
    {
        /// <summary>Gets the skill id.</summary>
        public string Id { get; }

        /// <summary>Gets the readable display name.</summary>
        public string Name { get; }

        /// <summary>Gets the display name used by the skill tooltip.</summary>
        public string DisplayName { get { return Name; } }

        /// <summary>Gets the uppercase name shown under the skill icon.</summary>
        public string DisplayNameUpper { get { return Name.ToUpperInvariant(); } }

        /// <summary>Gets the bundled ability icon, or null when no icon is delivered.</summary>
        public ImageSource? Icon { get; }

        /// <summary>Gets the skill level shown in the tooltip badge.</summary>
        public int Level { get; }

        /// <summary>Gets the base info (damage/heal, accuracy, crit, ranks) for the tooltip.</summary>
        public string BaseInfo { get; }

        /// <summary>Gets the buff/debuff rows the skill applies, shown in the tooltip table.</summary>
        public List<SkillEffectRowViewModel> EffectRows { get; }

        /// <summary>Gets the multi-line skill description.</summary>
        public string Details { get; }

        /// <summary>Gets or sets a value indicating whether the skill is active in battle.</summary>
        [ObservableProperty]
        private bool _isActive;

        /// <summary>Gets or sets a value indicating whether the tile can currently be clicked to toggle
        /// the skill. Active skills stay usable; inactive skills become unavailable at the selection limit.</summary>
        [ObservableProperty]
        private bool _isUsable = true;

        /// <summary>Gets a value indicating whether the skill is selected/active.</summary>
        public bool IsSelected { get { return IsActive; } }

        /// <summary>Initializes a new instance of the <see cref="LobbySkillViewModel"/> class.</summary>
        /// <param name="skill">The combat skill from the parsed content.</param>
        /// <param name="isActive">Whether the skill starts active.</param>
        /// <param name="icon">The bundled ability icon, or null when no icon is delivered.</param>
        public LobbySkillViewModel(CombatSkill skill, bool isActive, ImageSource? icon)
        {
            Id = skill.Id;
            Name = DisplayNames.Title(skill.Id);
            Level = skill.Level;
            BaseInfo = SkillDetails.BuildBaseInfo(skill);
            EffectRows = SkillDetails.BuildEffectRows(skill);
            Details = SkillDetails.Build(skill);
            Icon = icon;
            IsActive = isActive;
        }

        partial void OnIsActiveChanged(bool value)
        {
            OnPropertyChanged(nameof(IsSelected));
        }
    }
}
