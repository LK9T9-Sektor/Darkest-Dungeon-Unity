namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single buff/debuff row a skill applies, shown in the skill tooltip table.</summary>
    public class SkillEffectRowViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="SkillEffectRowViewModel"/> class.</summary>
        /// <param name="name">The effect name.</param>
        /// <param name="description">The effect description.</param>
        /// <param name="tone">The tone ("Buff", "Heal" or "Debuff").</param>
        public SkillEffectRowViewModel(string name, string description, string tone)
        {
            Name = name;
            Description = description;
            Tone = tone;
        }

        /// <summary>Gets the effect name.</summary>
        public string Name { get; }

        /// <summary>Gets the effect description.</summary>
        public string Description { get; }

        /// <summary>Gets the tone ("Buff", "Heal" or "Debuff").</summary>
        public string Tone { get; }
    }
}