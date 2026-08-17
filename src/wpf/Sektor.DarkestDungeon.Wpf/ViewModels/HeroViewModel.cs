using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Selected hero: banner, stats and skill set.</summary>
    public partial class HeroViewModel : ObservableObject
    {
        /// <summary>Gets the hero name placeholder.</summary>
        public string Name { get; } = "Reynauld";

        /// <summary>Gets the hero class placeholder.</summary>
        public string ClassName { get; } = "Crusader";

        /// <summary>Gets the move slot.</summary>
        public SkillViewModel Move { get; } = new SkillViewModel("Move");

        /// <summary>Gets the four combat skill slots.</summary>
        public System.Collections.Generic.List<SkillViewModel> CombatSkills { get; } = new System.Collections.Generic.List<SkillViewModel>
        {
            new SkillViewModel("Smite"),
            new SkillViewModel("Zealous Accusation"),
            new SkillViewModel("Stunning Blow"),
            new SkillViewModel("Battle Heal"),
        };

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

        /// <summary>Gets the hero stat sheet.</summary>
        public HeroStatsViewModel Stats { get; } = new HeroStatsViewModel();
    }
}
