using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single hero slot in a lobby: class cycling, active skill selection and trait reroll.</summary>
    public partial class HeroSlotViewModel : ObservableObject
    {
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

        /// <summary>Gets or sets the quirk summary text ("+tough, -fragile").</summary>
        [ObservableProperty]
        private string _quirkSummary = string.Empty;

        /// <summary>Gets the combat skill toggles.</summary>
        public ObservableCollection<LobbySkillViewModel> Skills { get; } = new ObservableCollection<LobbySkillViewModel>();

        /// <summary>Gets the rolled quirks.</summary>
        public ObservableCollection<Quirk> Quirks { get; } = new ObservableCollection<Quirk>();

        /// <summary>Gets the command that cycles to the previous class.</summary>
        public IRelayCommand PrevClassCommand { get; }

        /// <summary>Gets the command that cycles to the next class.</summary>
        public IRelayCommand NextClassCommand { get; }

        /// <summary>Gets the command that toggles an active combat skill.</summary>
        public IRelayCommand<LobbySkillViewModel> ToggleSkillCommand { get; }

        /// <summary>Gets the command that rerolls the hero's quirks.</summary>
        public IRelayCommand RerollQuirksCommand { get; }

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
            AddRandomQuirk(QuirkCatalog.Positive);
            AddRandomQuirk(QuirkCatalog.Negative);
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

        private void LoadClass()
        {
            var heroClass = DuelClasses.Get(ClassId);
            MaxActiveSkills = heroClass != null && heroClass.NumberOfSelectedCombatSkills > 0
                ? heroClass.NumberOfSelectedCombatSkills
                : 4;

            Skills.Clear();
            var allSkills = heroClass?.CombatSkills ?? new List<CombatSkill>();
            for (int i = 0; i < allSkills.Count; i++)
                Skills.Add(new LobbySkillViewModel(allSkills[i].Id, i < MaxActiveSkills));

            Details = BuildDetails(heroClass);
            RerollQuirks();
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