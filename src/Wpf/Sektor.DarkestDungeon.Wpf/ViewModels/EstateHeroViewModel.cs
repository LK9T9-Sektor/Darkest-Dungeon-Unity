using Sektor.DarkestDungeon.Core.GameEvents;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>One hero card in the estate replay.</summary>
    public sealed class EstateHeroViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="EstateHeroViewModel"/> class.</summary>
        /// <param name="hero">The hero state.</param>
        public EstateHeroViewModel(EstateHeroState hero)
        {
            Id = hero.Id;
            Class = hero.Class;
            ClassLabel = DisplayNames.Class(hero.Class);
            Name = hero.Name;
            LevelText = $"Ур. {hero.Level}";
            Hp = hero.Hp;
            MaxHp = hero.MaxHp > 0 ? hero.MaxHp : 1;
            HpText = $"{hero.Hp}/{hero.MaxHp}";
            StressText = $"{hero.Stress}";
            StatusText = hero.Status;
            IsDead = !hero.IsAlive;
            QuirksText = hero.Quirks == null ? string.Empty : string.Join(", ", hero.Quirks);
            DiseasesText = hero.Diseases == null ? string.Empty : string.Join(", ", hero.Diseases);
            ConditionText = Condition(hero);
        }

        /// <summary>Gets the hero id.</summary>
        public string Id { get; }

        /// <summary>Gets the hero class id.</summary>
        public string Class { get; }

        /// <summary>Gets the readable hero class label.</summary>
        public string ClassLabel { get; }

        /// <summary>Gets the hero name.</summary>
        public string Name { get; }

        /// <summary>Gets the resolve level label.</summary>
        public string LevelText { get; }

        /// <summary>Gets the current hit points.</summary>
        public int Hp { get; }

        /// <summary>Gets the maximum hit points (at least one to keep the progress bar valid).</summary>
        public int MaxHp { get; }

        /// <summary>Gets a value indicating whether the hero is dead.</summary>
        public bool IsDead { get; }

        /// <summary>Gets the hp label.</summary>
        public string HpText { get; }

        /// <summary>Gets the stress label.</summary>
        public string StressText { get; }

        /// <summary>Gets the estate status.</summary>
        public string StatusText { get; }

        /// <summary>Gets the quirk list.</summary>
        public string QuirksText { get; }

        /// <summary>Gets the disease list.</summary>
        public string DiseasesText { get; }

        /// <summary>Gets the affliction/virtue/death-door label.</summary>
        public string ConditionText { get; }

        private static string Condition(EstateHeroState hero)
        {
            if (!hero.IsAlive)
                return "МЁРТВ";
            if (hero.IsAfflicted)
                return "АФФЛИКТ";
            if (hero.IsVirtued)
                return "ДОБРОДЕТЕЛЬ";
            return string.Empty;
        }
    }
}
