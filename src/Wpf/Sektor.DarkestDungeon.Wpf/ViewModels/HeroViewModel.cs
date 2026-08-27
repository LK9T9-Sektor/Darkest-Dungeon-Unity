using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Selected hero: banner, stats and skill set sourced from core data.</summary>
    public partial class HeroViewModel : ObservableObject
    {
        /// <summary>Gets or sets the hero name.</summary>
        [ObservableProperty]
        private string _name = "Reynauld";

        /// <summary>Gets or sets the hero class.</summary>
        [ObservableProperty]
        private string _className = "Crusader";

        /// <summary>Gets the move slot.</summary>
        public SkillViewModel Move { get; } = new SkillViewModel("Move");

        /// <summary>Gets the four combat skill slots built from core <see cref="CombatSkill"/> definitions.</summary>
        public ObservableCollection<SkillViewModel> CombatSkills { get; }

        /// <summary>Gets the pass slot.</summary>
        public SkillViewModel Pass { get; } = new SkillViewModel("Pass");

        /// <summary>Gets the camping skill slots.</summary>
        public ObservableCollection<SkillViewModel> CampingSkills { get; } = new ObservableCollection<SkillViewModel>
        {
            new SkillViewModel("Pray"),
            new SkillViewModel("Zeal"),
            new SkillViewModel("Stand Tall"),
            new SkillViewModel("Battle Repetition"),
        };

        /// <summary>Gets the banner skill slots in display order (move, combat, pass).</summary>
        public ObservableCollection<SkillViewModel> BannerSkills { get; }

        /// <summary>Gets the hero stat sheet.</summary>
        public HeroStatsViewModel Stats { get; } = new HeroStatsViewModel();

        /// <summary>Initializes a new instance of the <see cref="HeroViewModel"/> class using core sample skills.</summary>
        public HeroViewModel()
            : this(CombatSampleData.BuildHeroCombatSkills())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HeroViewModel"/> class.</summary>
        /// <param name="combatSkills">The core combat skills of the hero.</param>
        public HeroViewModel(IReadOnlyList<CombatSkill> combatSkills)
        {
            CombatSkills = new ObservableCollection<SkillViewModel>(combatSkills.Select(ToSkillViewModel));
            BannerSkills = new ObservableCollection<SkillViewModel> { Move };
            foreach (var skill in CombatSkills)
                BannerSkills.Add(skill);
            BannerSkills.Add(Pass);
        }

        /// <summary>Fills the panel with the acting unit's live data.</summary>
        /// <param name="name">The unit name.</param>
        /// <param name="className">The class label.</param>
        /// <param name="combatSkills">The unit's combat skills.</param>
        /// <param name="hpCurrent">The current hit points.</param>
        /// <param name="hpMax">The maximum hit points.</param>
        /// <param name="stress">The stress value.</param>
        public void Apply(string name, string className, IEnumerable<CombatSkill> combatSkills, int hpCurrent, int hpMax, int stress)
        {
            Name = name;
            ClassName = className;
            Stats.HeroName = name;
            Stats.HeroClass = className;
            Stats.HitPoints = hpCurrent + " / " + hpMax;
            Stats.Stress = stress + " / 100";

            CombatSkills.Clear();
            foreach (var skill in combatSkills)
                CombatSkills.Add(ToSkillViewModel(skill));

            BannerSkills.Clear();
            BannerSkills.Add(Move);
            foreach (var skill in CombatSkills)
                BannerSkills.Add(skill);
            BannerSkills.Add(Pass);
        }

        private static SkillViewModel ToSkillViewModel(CombatSkill skill)
        {
            string label = skill.Id;
            if (skill.TargetRanks.IsSelfFormation && skill.TargetRanks.IsMultitarget)
                label += " (party)";
            else if (skill.TargetRanks.IsRandomTarget)
                label += " (random)";
            else if (skill.TargetRanks.IsSelfFormation)
                label += " (self)";
            return new SkillViewModel(label);
        }
    }
}