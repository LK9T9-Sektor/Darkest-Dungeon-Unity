using System.Collections.Generic;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A skill button in the live duel battle view.</summary>
    public partial class DuelSkillViewModel : ObservableObject
    {
        /// <summary>Gets the skill id.</summary>
        public string Id { get; }

        /// <summary>Gets the display name.</summary>
        public string DisplayName { get; }

        /// <summary>Gets the visual tone (attack/heal/buff) used by the target arrow color.</summary>
        public SkillTone Tone { get; }

        /// <summary>Gets or sets the skill upgrade level (0 in duels).</summary>
        public int Level { get; set; }

        /// <summary>Gets or sets the base info text (damage/heal, accuracy, crit, ranks) for the tooltip.</summary>
        public string BaseInfo { get; set; } = string.Empty;

        /// <summary>Gets or sets the buff/debuff rows the skill applies, shown in the tooltip table.</summary>
        public List<SkillEffectRowViewModel> EffectRows { get; set; } = new List<SkillEffectRowViewModel>();

        /// <summary>Gets the uppercase display name shown below the skill button.</summary>
        public string DisplayNameUpper { get { return DisplayName.ToUpperInvariant(); } }

        /// <summary>Gets or sets the icon image (null until one is provided).</summary>
        [ObservableProperty]
        private ImageSource? _icon;

        /// <summary>Gets or sets the tooltip details (targets, heal, damage).</summary>
        [ObservableProperty]
        private string _details = string.Empty;

        /// <summary>Gets or sets a value indicating whether the skill is usable.</summary>
        [ObservableProperty]
        private bool _isUsable;

        /// <summary>Gets or sets a value indicating whether the skill is selected.</summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>Initializes a new instance of the <see cref="DuelSkillViewModel"/> class.</summary>
        /// <param name="id">The skill id.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="tone">The visual tone.</param>
        public DuelSkillViewModel(string id, string displayName, SkillTone tone = SkillTone.Attack)
        {
            Id = id;
            DisplayName = displayName;
            Tone = tone;
        }
    }
}