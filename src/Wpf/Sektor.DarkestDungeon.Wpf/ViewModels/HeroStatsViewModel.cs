using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        /// <summary>Gets or sets the formation rank.</summary>
        [ObservableProperty]
        private int _rank;

        /// <summary>Gets or sets a value indicating whether the unit belongs to the enemy side.</summary>
        [ObservableProperty]
        private bool _isEnemy;

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

        /// <summary>Gets or sets the label of the equipped trinket in the left slot.</summary>
        [ObservableProperty]
        private string _trinket1Text = "-";

        /// <summary>Gets or sets the label of the equipped trinket in the right slot.</summary>
        [ObservableProperty]
        private string _trinket2Text = "-";

        /// <summary>Gets or sets the all combat skills text.</summary>
        [ObservableProperty]
        private string _skillsText = string.Empty;

        /// <summary>Gets the structured combat skills shown as skill squares in the sheet.</summary>
        public ObservableCollection<DuelSkillViewModel> Skills { get; } = new ObservableCollection<DuelSkillViewModel>();

        /// <summary>Gets or sets the stun resistance percentage.</summary>
        [ObservableProperty]
        private int _resistStun;

        /// <summary>Gets or sets the blight resistance percentage.</summary>
        [ObservableProperty]
        private int _resistBlight;

        /// <summary>Gets or sets the bleed resistance percentage.</summary>
        [ObservableProperty]
        private int _resistBleed;

        /// <summary>Gets or sets the debuff resistance percentage.</summary>
        [ObservableProperty]
        private int _resistDebuff;

        /// <summary>Gets or sets the move resistance percentage.</summary>
        [ObservableProperty]
        private int _resistMove;

        /// <summary>Gets or sets the disease resistance percentage.</summary>
        [ObservableProperty]
        private int _resistDisease;

        /// <summary>Gets or sets the death blow resistance percentage.</summary>
        [ObservableProperty]
        private int _resistDeathBlow;

        /// <summary>Gets or sets the trap resistance percentage.</summary>
        [ObservableProperty]
        private int _resistTrap;

        /// <summary>Gets or sets the quirks text.</summary>
        [ObservableProperty]
        private string _quirksText = string.Empty;

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
        /// <param name="skills">The structured combat skills to show as skill squares.</param>
        public void Apply(DuelUnitViewModel unit, IReadOnlyList<DuelSkillViewModel>? skills)
        {
            HeroName = unit.Name;
            HeroClass = unit.ClassName;
            Rank = unit.Rank;
            IsEnemy = unit.IsEnemy;
            HitPoints = unit.HpCurrent + " / " + unit.HpMax;
            Stress = unit.Stress + " / 100";
            Speed = unit.Speed.ToString();
            Damage = unit.Damage;
            Accuracy = "+" + unit.Accuracy;
            Crit = unit.Crit + "%";
            Dodge = unit.Dodge.ToString();
            Protection = unit.Protection + "%";
            QuirksText = unit.QuirksText;
            ResistStun = unit.ResistStun;
            ResistBlight = unit.ResistBlight;
            ResistBleed = unit.ResistBleed;
            ResistDebuff = unit.ResistDebuff;
            ResistMove = unit.ResistMove;
            ResistDisease = unit.ResistDisease;
            ResistDeathBlow = unit.ResistDeathBlow;
            ResistTrap = unit.ResistTrap;

            Skills.Clear();
            if (skills != null)
            {
                foreach (var skill in skills)
                    Skills.Add(skill);
            }
        }
    }
}