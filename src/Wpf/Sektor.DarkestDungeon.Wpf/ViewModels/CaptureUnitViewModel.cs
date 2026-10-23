using System.Collections.Generic;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>One combatant of the replayed capture, placed in the formation view.</summary>
    public partial class CaptureUnitViewModel : ObservableObject, IBattleFieldUnit
    {
        private const int CardWidthPerRank = 185;

        private readonly IReadOnlyList<CombatantStatusState> statuses;
        private readonly IReadOnlyList<CombatantBuffState> buffs;
        private readonly bool capturedIsDead;
        private string idleContentDirectory = string.Empty;

        /// <summary>Initializes a new instance of the <see cref="CaptureUnitViewModel"/> class.</summary>
        /// <param name="state">The captured combatant state.</param>
        /// <param name="fallbackRank">The rank used for recordings made before schema 1.1.</param>
        public CaptureUnitViewModel(CombatantState state, int fallbackRank = 1)
        {
            Id = state.Id;
            Name = state.Name;
            Hp = state.Hp;
            MaxHp = state.MaxHp;
            Stress = state.Stress;
            MaxStress = state.MaxStress;
            Rank = state.Rank > 0 ? state.Rank : fallbackRank;
            Size = state.Size > 0 ? state.Size : 1;
            IsEnemy = state.IsEnemy;
            Skeleton = state.Skeleton;
            AtDeathsDoor = state.AtDeathsDoor;
            IsAfflicted = state.IsAfflicted;
            IsVirtued = state.IsVirtued;
            statuses = state.Statuses ?? new List<CombatantStatusState>();
            buffs = state.Buffs ?? new List<CombatantBuffState>();
            capturedIsDead = state.IsDead;
        }

        /// <summary>Gets the unit id used by the events.</summary>
        public string Id { get; }

        /// <summary>Gets or sets the display name.</summary>
        [ObservableProperty]
        private string _name;

        /// <summary>Gets or sets the current hit points.</summary>
        [ObservableProperty]
        private int _hp;

        /// <summary>Gets or sets the maximum hit points.</summary>
        [ObservableProperty]
        private int _maxHp;

        /// <summary>Gets or sets the current stress.</summary>
        [ObservableProperty]
        private int _stress;

        /// <summary>Gets or sets the maximum stress.</summary>
        [ObservableProperty]
        private int _maxStress;

        /// <summary>Gets or sets the formation rank (1 nearest the enemies).</summary>
        [ObservableProperty]
        private int _rank;

        /// <summary>Gets or sets the formation size.</summary>
        [ObservableProperty]
        private int _size;

        /// <summary>Gets or sets a value indicating whether the unit is at Death's Door.</summary>
        [ObservableProperty]
        private bool _atDeathsDoor;

        /// <summary>Gets or sets a value indicating whether the unit is afflicted.</summary>
        [ObservableProperty]
        private bool _isAfflicted;

        /// <summary>Gets or sets a value indicating whether the unit is virtued.</summary>
        [ObservableProperty]
        private bool _isVirtued;

        /// <summary>Gets a value indicating whether the unit is a monster.</summary>
        public bool IsEnemy { get; }

        /// <summary>Gets the logical spine skeleton id.</summary>
        public string Skeleton { get; }

        /// <summary>Gets or sets the spine content currently shown for the unit.</summary>
        [ObservableProperty]
        private string _contentDirectory = string.Empty;

        /// <summary>Gets or sets a value indicating whether the current animation loops.</summary>
        [ObservableProperty]
        private bool _loop = true;

        /// <summary>Gets or sets a value indicating whether the unit is the current attacker.</summary>
        [ObservableProperty]
        private bool _isAttacker;

        /// <summary>Gets or sets a value indicating whether the unit is the current target.</summary>
        [ObservableProperty]
        private bool _isTarget;

        /// <summary>Gets a value indicating whether the unit has spine content to render.</summary>
        public bool HasSpineContent => !string.IsNullOrEmpty(ContentDirectory);

        /// <summary>Gets a value indicating whether the unit renders the monster/unknown card.</summary>
        public bool IsCard => !HasSpineContent;

        /// <summary>Gets a value indicating whether the capture recorded the unit as dead.</summary>
        /// <remarks>
        /// A hero at Death's Door has zero hit points but is still alive; older captures may also
        /// have written <c>IsDead</c> for it, so <see cref="AtDeathsDoor"/> wins here.
        /// </remarks>
        public bool IsDead => capturedIsDead && !AtDeathsDoor;

        /// <summary>Gets a value indicating whether the capture recorded a stress pool.</summary>
        public bool HasStress => MaxStress > 0;

        /// <summary>Gets the HP text shown next to the unit.</summary>
        public string HpText => $"{Hp}/{MaxHp}";

        /// <summary>Gets the HP ratio in the 0..1 range.</summary>
        public double HpRatio => MaxHp <= 0 ? 0 : (double)Hp / MaxHp;

        /// <summary>Gets the stress text shown next to the unit.</summary>
        public string StressText => HasStress ? $"{Stress}/{MaxStress}" : $"{Stress}";

        /// <summary>Gets the stress ratio in the 0..1 range.</summary>
        public double StressRatio => MaxStress <= 0 ? 0 : (double)Stress / MaxStress;

        /// <summary>Gets the rank/size badge shown above the unit.</summary>
        public string FormationLabel => Size > 1 ? $"R{Rank} · x{Size}" : $"R{Rank}";

        /// <summary>Gets the card width in pixels scaled by the occupied ranks.</summary>
        public double CardWidth => CardWidthPerRank * Size;

        /// <summary>Gets the affliction/death's-door condition shown below the unit.</summary>
        public string ConditionText
        {
            get
            {
                if (AtDeathsDoor)
                    return "СМЕРТНЫЙ ПОРОГ";
                if (IsAfflicted)
                    return "АФФЛИКТ";
                if (IsVirtued)
                    return "ДОБРОДЕТЕЛЬ";

                return string.Empty;
            }
        }

        /// <summary>Gets the active status and buff labels shown below the unit.</summary>
        public string StatusText
        {
            get
            {
                var labels = new List<string>();
                foreach (CombatantStatusState status in statuses.Where(status => status.IsApplied))
                {
                    labels.Add(status.Duration > 0 ? $"{status.Id} {status.Duration}" : status.Id);
                }

                foreach (CombatantBuffState buff in buffs.Where(buff => buff.IsApplied))
                {
                    labels.Add($"{buff.Attribute} {buff.ModifierValue:+0.##;-0.##}");
                }

                return string.Join(" · ", labels);
            }
        }

        /// <summary>Sets the looping animation shown while the unit waits for its attack.</summary>
        /// <param name="directory">The idle content directory, or <c>null</c> when the unit has no spine content.</param>
        public void SetIdleContent(string? directory)
        {
            idleContentDirectory = directory ?? string.Empty;
            ShowIdle();
        }

        /// <summary>Shows the captured attack animation, falling back to idle when the clip is unavailable.</summary>
        /// <param name="directory">The attack content directory, or <c>null</c> when it is not linked.</param>
        public void BeginAttack(string? directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                ShowIdle();
                return;
            }

            ContentDirectory = directory;
            Loop = false;
        }

        /// <summary>Returns the unit to its looping idle animation.</summary>
        public void EndAttack() => ShowIdle();

        partial void OnHpChanged(int value)
        {
            OnPropertyChanged(nameof(HpText));
            OnPropertyChanged(nameof(HpRatio));
            OnPropertyChanged(nameof(IsDead));
        }

        partial void OnMaxHpChanged(int value)
        {
            OnPropertyChanged(nameof(HpText));
            OnPropertyChanged(nameof(HpRatio));
        }

        partial void OnStressChanged(int value)
        {
            OnPropertyChanged(nameof(StressText));
            OnPropertyChanged(nameof(StressRatio));
        }

        partial void OnMaxStressChanged(int value)
        {
            OnPropertyChanged(nameof(HasStress));
            OnPropertyChanged(nameof(StressText));
            OnPropertyChanged(nameof(StressRatio));
        }

        partial void OnRankChanged(int value) => OnPropertyChanged(nameof(FormationLabel));

        partial void OnSizeChanged(int value) => OnPropertyChanged(nameof(FormationLabel));

        partial void OnAtDeathsDoorChanged(bool value)
        {
            OnPropertyChanged(nameof(ConditionText));
            OnPropertyChanged(nameof(IsDead));
        }

        partial void OnIsAfflictedChanged(bool value) => OnPropertyChanged(nameof(ConditionText));

        partial void OnIsVirtuedChanged(bool value) => OnPropertyChanged(nameof(ConditionText));

        partial void OnContentDirectoryChanged(string value)
        {
            OnPropertyChanged(nameof(HasSpineContent));
            OnPropertyChanged(nameof(IsCard));
        }

        private void ShowIdle()
        {
            ContentDirectory = idleContentDirectory;
            Loop = true;
        }
    }
}
