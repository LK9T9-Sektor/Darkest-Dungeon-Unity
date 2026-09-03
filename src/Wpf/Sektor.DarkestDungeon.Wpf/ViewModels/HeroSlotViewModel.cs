using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Trinket;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single hero slot in a lobby: class cycling, active skill selection, trait reroll and trinket slots.</summary>
    public partial class HeroSlotViewModel : ObservableObject
    {
        private const int TrinketSlotCount = 2;

        private readonly IReadOnlyList<string> availableClasses;
        private readonly Random rng = new Random();

        /// <summary>Gets the deterministic seed of this slot.</summary>
        public int Seed { get; }

        /// <summary>Gets or sets the selected class id.</summary>
        [ObservableProperty]
        private string _classId;

        /// <summary>Gets or sets a value indicating whether the slot is empty.</summary>
        [ObservableProperty]
        private bool _isEmpty = true;

        /// <summary>Gets or sets the maximum number of active combat skills.</summary>
        [ObservableProperty]
        private int _maxActiveSkills = 4;

        /// <summary>Gets or sets the tooltip details (stats, skills, quirks).</summary>
        [ObservableProperty]
        private string _details = string.Empty;

        /// <summary>Gets or sets the portrait image (null until one is provided).</summary>
        [ObservableProperty]
        private ImageSource? _portrait;

        /// <summary>Gets or sets the quirk summary text ("+tough, -fragile").</summary>
        [ObservableProperty]
        private string _quirkSummary = string.Empty;

        /// <summary>Gets the combat skill toggles.</summary>
        public ObservableCollection<LobbySkillViewModel> Skills { get; } = new ObservableCollection<LobbySkillViewModel>();

        /// <summary>Gets the rolled quirks.</summary>
        public ObservableCollection<Quirk> Quirks { get; } = new ObservableCollection<Quirk>();

        /// <summary>Gets the two trinket slots (left then right).</summary>
        public ObservableCollection<LobbyTrinketViewModel> TrinketSlots { get; } = new ObservableCollection<LobbyTrinketViewModel>();

        /// <summary>Gets the stat sheet preview of the selected class.</summary>
        public HeroStatsViewModel Stats { get; } = new HeroStatsViewModel();

        /// <summary>Gets the command that cycles to the previous class.</summary>
        public IRelayCommand PrevClassCommand { get; }

        /// <summary>Gets the command that cycles to the next class.</summary>
        public IRelayCommand NextClassCommand { get; }

        /// <summary>Gets the command that toggles an active combat skill.</summary>
        public IRelayCommand<LobbySkillViewModel> ToggleSkillCommand { get; }

        /// <summary>Gets the command that rerolls the hero's quirks.</summary>
        public IRelayCommand RerollQuirksCommand { get; }

        /// <summary>Gets the command that assigns two random trinkets valid for the hero class.</summary>
        public IRelayCommand RerollTrinketsCommand { get; }

        /// <summary>Gets the ids of the active combat skills.</summary>
        public IReadOnlyList<string> SelectedSkillIds
        {
            get { return Skills.Where(skill => skill.IsActive).Select(skill => skill.Id).ToList(); }
        }

        /// <summary>Gets the ids of the chosen quirks.</summary>
        public IReadOnlyList<string> SelectedQuirkIds
        {
            get { return Quirks.Select(quirk => quirk.Id).ToList(); }
        }

        /// <summary>Gets the ids of the equipped trinkets.</summary>
        public IReadOnlyList<string> SelectedTrinketIds
        {
            get { return TrinketSlots.Where(slot => slot.TrinketId.Length > 0).Select(slot => slot.TrinketId).ToList(); }
        }

        /// <summary>Initializes a new instance of the <see cref="HeroSlotViewModel"/> class.</summary>
        /// <param name="seed">The deterministic seed.</param>
        /// <param name="availableClasses">The selectable class ids.</param>
        public HeroSlotViewModel(int seed, IReadOnlyList<string> availableClasses)
        {
            this.availableClasses = availableClasses;
            Seed = seed;
            _classId = this.availableClasses[0];
            PrevClassCommand = new RelayCommand(PrevClass);
            NextClassCommand = new RelayCommand(NextClass);
            ToggleSkillCommand = new RelayCommand<LobbySkillViewModel>(ToggleSkill);
            RerollQuirksCommand = new RelayCommand(RerollQuirks);
            RerollTrinketsCommand = new RelayCommand(RerollTrinkets);
            LoadClass();
        }

        /// <summary>Gets the display name of the selected class.</summary>
        public string ClassName { get { return Ui.DisplayNames.Class(ClassId); } }

        /// <summary>Assigns a specific class and reloads its skills and quirks.</summary>
        /// <param name="classId">The class id to assign.</param>
        public void AssignClass(string classId)
        {
            ClassId = classId;
            IsEmpty = false;
            LoadClass();
        }

        private void PrevClass()
        {
            int index = IndexOf(ClassId);
            ClassId = availableClasses[(index - 1 + availableClasses.Count) % availableClasses.Count];
            IsEmpty = false;
            LoadClass();
        }

        private void NextClass()
        {
            int index = IndexOf(ClassId);
            ClassId = availableClasses[(index + 1) % availableClasses.Count];
            IsEmpty = false;
            LoadClass();
        }

        private void ToggleSkill(LobbySkillViewModel? skill)
        {
            if (skill == null)
                return;
            if (skill.IsActive)
            {
                skill.IsActive = false;
                return;
            }
            if (Skills.Count(active => active.IsActive) >= MaxActiveSkills)
                return;
            skill.IsActive = true;
        }

        private void RerollQuirks()
        {
            Quirks.Clear();
            AddRandomQuirk(Data.QuirkCatalog.Positive);
            AddRandomQuirk(Data.QuirkCatalog.Negative);
            QuirkSummary = Quirks.Count == 0
                ? "no quirks"
                : string.Join(", ", Quirks.Select(q => (q.IsPositive ? "+" : "-") + q.Id));
        }

        private void AddRandomQuirk(List<Quirk> pool)
        {
            if (pool.Count == 0)
                return;
            var candidates = pool.Where(quirk => !Quirks.Any(existing => existing.IncompatibleQuirks.Contains(quirk.Id))).ToList();
            if (candidates.Count == 0)
                return;
            Quirks.Add(candidates[rng.Next(candidates.Count)]);
        }

        private void RerollTrinkets()
        {
            var pool = TrinketPool();
            var ids = new HashSet<string>();
            foreach (var slot in TrinketSlots)
            {
                var candidates = pool.Where(trinket => !ids.Contains(trinket.Id)).ToList();
                if (candidates.Count == 0)
                {
                    slot.Select(string.Empty);
                    continue;
                }
                var chosen = candidates[rng.Next(candidates.Count)];
                ids.Add(chosen.Id);
                slot.Select(chosen.Id);
            }
        }

        private void LoadClass()
        {
            var heroClass = DuelClasses.Get(ClassId);
            MaxActiveSkills = heroClass != null && heroClass.NumberOfSelectedCombatSkills > 0
                ? heroClass.NumberOfSelectedCombatSkills
                : 4;

            Skills.Clear();
            var allSkills = heroClass?.CombatSkills ?? new List<CombatSkill>();
            for (int i = 0; i < allSkills.Count; i++)
            {
                Skills.Add(new LobbySkillViewModel(allSkills[i].Id, i < MaxActiveSkills)
                {
                    Details = Ui.SkillDetails.Build(allSkills[i]),
                });
            }

            ReloadTrinkets();
            Details = BuildDetails(heroClass);
            ApplyStats(heroClass);
            RerollQuirks();
        }

        private void ReloadTrinkets()
        {
            var pool = TrinketPool();
            if (TrinketSlots.Count == 0)
            {
                for (int i = 0; i < TrinketSlotCount; i++)
                    TrinketSlots.Add(new LobbyTrinketViewModel(pool));
                return;
            }

            foreach (var slot in TrinketSlots)
                slot.SetPool(pool);
        }

        private List<Trinket> TrinketPool()
        {
            var result = new List<Trinket>();
            foreach (var trinket in Data.TrinketCatalog.All)
            {
                if (trinket.HeroClassRequirements.Count == 0 || trinket.HeroClassRequirements.Contains(ClassId))
                    result.Add(trinket);
            }
            return result;
        }

        private void ApplyStats(HeroClass? heroClass)
        {
            Stats.HeroName = "Hero";
            Stats.HeroClass = heroClass == null ? string.Empty : Ui.DisplayNames.Class(heroClass.StringId);
            if (heroClass == null)
                return;

            Stats.HitPoints = Raw(heroClass, AttributeType.HitPoints).ToString();
            Stats.Stress = "0 / 100";
            Stats.Speed = Raw(heroClass, AttributeType.SpeedRating).ToString();
            Stats.Damage = Raw(heroClass, AttributeType.DamageLow) + " - " + Raw(heroClass, AttributeType.DamageHigh);
            Stats.Accuracy = "+" + Pct(heroClass, AttributeType.AttackRating);
            Stats.Crit = Pct(heroClass, AttributeType.CritChance) + "%";
            Stats.Dodge = Pct(heroClass, AttributeType.DefenseRating).ToString();
            Stats.Protection = Pct(heroClass, AttributeType.ProtectionRating) + "%";
            Stats.WeaponLevel = "Lv. 1";
            Stats.ArmorLevel = "Lv. 1";
        }

        private string BuildDetails(HeroClass? heroClass)
        {
            if (heroClass == null)
                return ClassId;

            var sb = new StringBuilder();
            sb.AppendLine(heroClass.StringId);
            sb.AppendFormat("HP {0}   SPD {1}   DMG {2}-{3}", Raw(heroClass, AttributeType.HitPoints), Raw(heroClass, AttributeType.SpeedRating), Raw(heroClass, AttributeType.DamageLow), Raw(heroClass, AttributeType.DamageHigh));
            sb.AppendLine();
            sb.AppendFormat("ACC +{0}   CRIT {1}%   DODGE {2}   PROT {3}%", Pct(heroClass, AttributeType.AttackRating), Pct(heroClass, AttributeType.CritChance), Pct(heroClass, AttributeType.DefenseRating), Pct(heroClass, AttributeType.ProtectionRating));
            sb.AppendLine();
            if (heroClass.CombatSkills.Count > 0)
                sb.AppendLine("Skills: " + string.Join(", ", heroClass.CombatSkills.Select(skill => skill.Id)));
            return sb.ToString();
        }

        private static int Raw(HeroClass heroClass, AttributeType type)
        {
            float value;
            return heroClass.Attributes.TryGetValue(type, out value) ? (int)value : 0;
        }

        private static int Pct(HeroClass heroClass, AttributeType type)
        {
            float value;
            return heroClass.Attributes.TryGetValue(type, out value) ? (int)(value * 100) : 0;
        }

        private int IndexOf(string classId)
        {
            for (int i = 0; i < availableClasses.Count; i++)
            {
                if (availableClasses[i] == classId)
                    return i;
            }
            return -1;
        }

        partial void OnClassIdChanged(string value)
        {
            OnPropertyChanged(nameof(ClassName));
        }
    }
}