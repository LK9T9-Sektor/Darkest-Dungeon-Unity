using System;
using System.Collections.Generic;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using Sektor.DarkestDungeon.Core.Content.Camping;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A camping skill in the lobby loadout: icon, tooltip, active/available state. Camping
    /// skills are selected like combat skills (up to four), but carry no level label.</summary>
    public partial class LobbyCampingSkillViewModel : ObservableObject, ISkillTileViewModel
    {
        /// <summary>Gets the skill id.</summary>
        public string Id { get; }

        /// <summary>Gets the readable display name.</summary>
        public string Name { get; }

        /// <summary>Gets the display name used by the skill tooltip.</summary>
        public string DisplayName { get { return Name; } }

        /// <summary>Gets the uppercase name shown under the skill icon.</summary>
        public string DisplayNameUpper { get { return Name.ToUpperInvariant(); } }

        /// <summary>Gets the bundled camping icon, or null when no icon is delivered.</summary>
        public ImageSource? Icon { get; }

        /// <summary>Gets the skill level (camping content currently uses 0).</summary>
        public int Level { get; }

        /// <summary>Gets the camping cost and use limit for the tooltip.</summary>
        public string BaseInfo { get; }

        /// <summary>Gets the buff rows for the tooltip; camping content currently has no structured rows.</summary>
        public List<SkillEffectRowViewModel> EffectRows { get; }

        /// <summary>Gets the multi-line skill description.</summary>
        public string Details { get; }

        /// <summary>Gets or sets a value indicating whether the skill is selected.</summary>
        [ObservableProperty]
        private bool _isActive;

        /// <summary>Gets or sets a value indicating whether the tile can currently be clicked to toggle
        /// the skill. Selected skills stay usable; unselected skills become unavailable at the limit.</summary>
        [ObservableProperty]
        private bool _isUsable = true;

        /// <summary>Gets a value indicating whether the skill is selected.</summary>
        public bool IsSelected { get { return IsActive; } }

        /// <summary>Gets a value indicating whether this is a pre-battle loadout tile (always true).</summary>
        public bool IsLoadout { get { return true; } }

        /// <summary>Gets a value indicating whether the icon should be dimmed (unselected).</summary>
        public bool IsMasked { get { return !IsSelected; } }

        /// <summary>Initializes a new instance of the <see cref="LobbyCampingSkillViewModel"/> class.</summary>
        /// <param name="skill">The camping skill from the parsed content.</param>
        /// <param name="isActive">Whether the skill starts selected.</param>
        /// <param name="icon">The bundled camping icon, or null when no icon is delivered.</param>
        public LobbyCampingSkillViewModel(CampingSkill skill, bool isActive, ImageSource? icon)
        {
            Id = skill.Id;
            Name = DisplayNames.Title(skill.Id);
            Level = skill.Level;
            BaseInfo = "Camping cost: " + skill.Cost + Environment.NewLine +
                       "Uses per camp: " + (skill.UseLimit > 0 ? skill.UseLimit.ToString() : "unlimited");
            EffectRows = new List<SkillEffectRowViewModel>();
            Details = Name + Environment.NewLine + BaseInfo;
            Icon = icon;
            IsActive = isActive;
        }

        partial void OnIsActiveChanged(bool value)
        {
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(IsMasked));
        }
    }
}
