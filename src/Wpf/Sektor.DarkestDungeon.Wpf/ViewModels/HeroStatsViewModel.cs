using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Stat sheet of the inspected hero/unit (opened on right-click).</summary>
    public partial class HeroStatsViewModel : ObservableObject
    {
        /// <summary>Gets or sets the hero name.</summary>
        [ObservableProperty]
        private string _heroName = "Reynauld";

        /// <summary>Gets or sets the hero class.</summary>
        [ObservableProperty]
        private string _heroClass = "Crusader";

        /// <summary>Gets or sets the current hit points text.</summary>
        [ObservableProperty]
        private string _hitPoints = "78 / 78";

        /// <summary>Gets or sets the current stress text.</summary>
        [ObservableProperty]
        private string _stress = "0 / 100";

        /// <summary>Gets or sets the speed value.</summary>
        [ObservableProperty]
        private string _speed = "4";

        /// <summary>Gets or sets the damage range.</summary>
        [ObservableProperty]
        private string _damage = "8 - 15";

        /// <summary>Gets or sets the accuracy modifier.</summary>
        [ObservableProperty]
        private string _accuracy = "+85";

        /// <summary>Gets or sets the critical chance.</summary>
        [ObservableProperty]
        private string _crit = "7%";

        /// <summary>Gets or sets the dodge value.</summary>
        [ObservableProperty]
        private string _dodge = "15";

        /// <summary>Gets or sets the protection value.</summary>
        [ObservableProperty]
        private string _protection = "10%";

        /// <summary>Gets or sets the weapon level label.</summary>
        [ObservableProperty]
        private string _weaponLevel = "Lv. 1";

        /// <summary>Gets or sets the armor level label.</summary>
        [ObservableProperty]
        private string _armorLevel = "Lv. 1";

        /// <summary>Fills the sheet from a stage unit, keeping placeholder values for the rest.</summary>
        /// <param name="unit">The inspected stage unit.</param>
        public void Apply(UnitViewModel unit)
        {
            HeroName = unit.Name;
            HeroClass = unit.ClassName;
            HitPoints = unit.HpCurrent + " / " + unit.HpMax;
            Stress = unit.Stress + " / 100";
        }

        /// <summary>Fills the sheet from a duel unit.</summary>
        /// <param name="unit">The inspected unit.</param>
        public void Apply(DuelUnitViewModel unit)
        {
            HeroName = unit.Name;
            HeroClass = unit.ClassName;
            HitPoints = unit.HpCurrent + " / " + unit.HpMax;
            Stress = unit.Stress + " / 100";
            Speed = unit.Speed.ToString();
            Damage = unit.Damage;
            Accuracy = "+" + unit.Accuracy;
            Crit = unit.Crit + "%";
            Dodge = unit.Dodge.ToString();
            Protection = unit.Protection + "%";
        }
    }
}