using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A unit card in the live duel battle view.</summary>
    public partial class DuelUnitViewModel : ObservableObject
    {
        /// <summary>Gets the combat id.</summary>
        public int CombatId { get; }

        /// <summary>Gets the formation rank (1-based).</summary>
        public int Rank { get; }

        /// <summary>Gets the display name.</summary>
        public string Name { get; }

        /// <summary>Gets the class label.</summary>
        public string ClassName { get; }

        /// <summary>Gets or sets the current hit points.</summary>
        [ObservableProperty]
        private int _hpCurrent;

        /// <summary>Gets or sets the maximum hit points.</summary>
        [ObservableProperty]
        private int _hpMax;

        /// <summary>Gets or sets the stress value.</summary>
        [ObservableProperty]
        private int _stress;

        /// <summary>Gets or sets the speed value.</summary>
        [ObservableProperty]
        private int _speed;

        /// <summary>Gets or sets the damage range text ("min - max").</summary>
        [ObservableProperty]
        private string _damage = string.Empty;

        /// <summary>Gets or sets the accuracy value.</summary>
        [ObservableProperty]
        private int _accuracy;

        /// <summary>Gets or sets the critical chance percentage.</summary>
        [ObservableProperty]
        private int _crit;

        /// <summary>Gets or sets the dodge value.</summary>
        [ObservableProperty]
        private int _dodge;

        /// <summary>Gets or sets the protection percentage.</summary>
        [ObservableProperty]
        private int _protection;

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

        /// <summary>Gets or sets the comma-separated ids of all class combat skills.</summary>
        [ObservableProperty]
        private string _allSkills = string.Empty;

        /// <summary>Gets or sets a value indicating whether this unit is the current acting unit.</summary>
        [ObservableProperty]
        private bool _isCurrent;

        /// <summary>Gets or sets a value indicating whether this unit is highlighted as a target.</summary>
        [ObservableProperty]
        private bool _isTarget;

        /// <summary>Gets or sets a value indicating whether this unit is hovered.</summary>
        [ObservableProperty]
        private bool _isSelected;

        /// <summary>Gets or sets a value indicating whether the unit belongs to the enemy side.</summary>
        [ObservableProperty]
        private bool _isEnemy;

        /// <summary>Gets or sets the floating damage popup text shown over the card.</summary>
        [ObservableProperty]
        private string _damagePopupText = string.Empty;

        /// <summary>Gets or sets a value indicating whether the damage popup is shown and animating.</summary>
        [ObservableProperty]
        private bool _damagePopupVisible;

        /// <summary>Gets the hit point ratio (0-1) for the health bar.</summary>
        public double HpRatio { get { return HpMax <= 0 ? 0 : (double)HpCurrent / HpMax; } }

        /// <summary>Gets the hit points text.</summary>
        public string HpText { get { return HpCurrent + " / " + HpMax; } }

        /// <summary>Gets the hp bar segments (1 = filled, 0 = empty).</summary>
        public List<int> HpSegments { get; } = new List<int>();

        /// <summary>Gets the stress pips (0 = empty, 1 = normal, 2 = stressed).</summary>
        public List<int> StressPips { get; } = new List<int>();

        /// <summary>Initializes a new instance of the <see cref="DuelUnitViewModel"/> class.</summary>
        /// <param name="combatId">The combat id.</param>
        /// <param name="rank">The formation rank.</param>
        /// <param name="name">The name.</param>
        /// <param name="className">The class label.</param>
        public DuelUnitViewModel(int combatId, int rank, string name, string className)
        {
            CombatId = combatId;
            Rank = rank;
            Name = name;
            ClassName = className;
            UpdateBars();
        }

        /// <summary>Rebuilds the segmented hp bar and the 10 stress pips from the current values.</summary>
        public void UpdateBars()
        {
            HpSegments.Clear();
            int hpFilled = HpMax <= 0 ? 0 : (int)Math.Round(HpRatio * 12);
            for (int i = 0; i < 12; i++)
                HpSegments.Add(i < hpFilled ? 1 : 0);

            StressPips.Clear();
            int stressFilled = Math.Min(10, (int)Math.Round(Stress / 10.0));
            for (int i = 0; i < 10; i++)
                StressPips.Add(i < stressFilled ? (i >= 5 ? 2 : 1) : 0);
        }

        partial void OnHpCurrentChanged(int value)
        {
            OnPropertyChanged(nameof(HpRatio));
            UpdateBars();
        }

        partial void OnHpMaxChanged(int value)
        {
            OnPropertyChanged(nameof(HpRatio));
            UpdateBars();
        }

        partial void OnStressChanged(int value)
        {
            UpdateBars();
        }
    }
}