using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Selected hero: banner, stats and skill set sourced from core data.</summary>
    public partial class HeroViewModel : ObservableObject
    {
        /// <summary>Gets the hero name placeholder.</summary>
        public string Name { get; } = "Reynauld";

        /// <summary>Gets the hero class placeholder.</summary>
        public string ClassName { get; } = "Crusader";

        /// <summary>Gets the move slot.</summary>
        public SkillViewModel Move { get; } = new SkillViewModel("Move");

        /// <summary>Gets the four combat skill slots built from core <see cref="CombatSkill"/> definitions.</summary>
        public System.Collections.Generic.List<SkillViewModel> CombatSkills { get; }

        /// <summary>Gets the pass slot.</summary>
        public SkillViewModel Pass { get; } = new SkillViewModel("Pass");

        /// <summary>Gets the camping skill slots.</summary>
        public System.Collections.Generic.List<SkillViewModel> CampingSkills { get; } = new System.Collections.Generic.List<SkillViewModel>
        {
            new SkillViewModel("Pray"),
            new SkillViewModel("Zeal"),
            new SkillViewModel("Stand Tall"),
            new SkillViewModel("Battle Repetition"),
        };

        /// <summary>Gets the banner skill slots in display order (move, combat, pass).</summary>
        public System.Collections.Generic.IReadOnlyList<SkillViewModel> BannerSkills { get; }

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
            CombatSkills = combatSkills.Select(ToSkillViewModel).ToList();
            var bannerSkills = new System.Collections.Generic.List<SkillViewModel> { Move };
            bannerSkills.AddRange(CombatSkills);
            bannerSkills.Add(Pass);
            BannerSkills = bannerSkills;
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