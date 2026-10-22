using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    /// <summary>A single hero slot in a lobby: class cycling, combat and camping skill selection,
    /// trait reroll, trinket slots and the Idle Spine content of the current class.</summary>
    public partial class HeroSlotViewModel : ObservableObject
    {
        private const int TrinketSlotCount = 2;
        private const int CampingSkillLimit = 4;
        private const int QuirkVisibleRows = 5;
        private const string PortraitOutfit = "A";

        private readonly IReadOnlyList<string> availableClasses;
        private readonly IHeroIconCatalog? iconCatalog;
        private readonly IHeroSkillCatalog? skillCatalog;
        private readonly ISpineDemoCatalog? spineCatalog;
        private readonly Random rng = new Random();

        /// <summary>Gets the deterministic seed of this slot.</summary>
        public int Seed { get; }

        /// <summary>Gets or sets the 1-based formation position of this slot.</summary>
        [ObservableProperty]
        private int _position;

        /// <summary>Gets the hero level shown in the editor level medallion.</summary>
        public int Level { get; } = 1;

        /// <summary>Gets or sets the current skin id shown by the skin selector.</summary>
        [ObservableProperty]
        private int _skinId = 1;

        /// <summary>Gets the number of implemented skins (one until outfit switching is wired).</summary>
        public int SkinCount { get; } = 1;

        /// <summary>Gets the maximum hero-name length accepted by the editor.</summary>
        public int MaxHeroNameLength { get; } = 24;

        /// <summary>Gets or sets the randomized hero name.</summary>
        [ObservableProperty]
        private string _heroName = "Hero";

        /// <summary>Gets or sets a value indicating whether the name is being edited.</summary>
        [ObservableProperty]
        private bool _isEditingName;

        /// <summary>Gets or sets the selected class id.</summary>
        [ObservableProperty]
        private string _classId;

        /// <summary>Gets or sets a value indicating whether the slot is empty.</summary>
        [ObservableProperty]
        private bool _isEmpty = true;

        /// <summary>Gets or sets the maximum number of active combat skills.</summary>
        [ObservableProperty]
        private int _maxActiveSkills = 4;

        /// <summary>Gets the maximum number of selected camping skills (four).</summary>
        public int MaxCampingSkills { get { return CampingSkillLimit; } }

        /// <summary>Gets or sets a value indicating whether the player may choose the active combat skills.</summary>
        [ObservableProperty]
        private bool _canSelectSkills;

        /// <summary>Gets a value indicating whether the class locks the skill selection (e.g. Abomination).</summary>
        public bool SkillsLocked { get { return !CanSelectSkills; } }

        /// <summary>Gets or sets the tooltip details (stats, skills, quirks).</summary>
        [ObservableProperty]
        private string _details = string.Empty;

        /// <summary>Gets or sets the roster portrait image of the selected class.</summary>
        [ObservableProperty]
        private ImageSource? _portrait;

        /// <summary>Gets or sets the guild header art of the selected class.</summary>
        [ObservableProperty]
        private ImageSource? _header;

        /// <summary>Gets or sets the Spine Idle content directory of the selected class; empty when
        /// the content is not available.</summary>
        [ObservableProperty]
        private string _idleContentDirectory = string.Empty;

        /// <summary>Gets or sets the quirk summary text ("+tough, -fragile").</summary>
        [ObservableProperty]
        private string _quirkSummary = string.Empty;

        /// <summary>Gets the combat skill toggles.</summary>
        public ObservableCollection<LobbySkillViewModel> Skills { get; } = new ObservableCollection<LobbySkillViewModel>();

        /// <summary>Gets the camping skill toggles (at most four selected).</summary>
        public ObservableCollection<LobbyCampingSkillViewModel> CampingSkills { get; } = new ObservableCollection<LobbyCampingSkillViewModel>();

        /// <summary>Gets the rolled quirks.</summary>
        public ObservableCollection<Quirk> Quirks { get; } = new ObservableCollection<Quirk>();

        /// <summary>Gets the quirk rows shown by the editor, including their lock state.</summary>
        public ObservableCollection<QuirkRowViewModel> QuirkRows { get; } = new ObservableCollection<QuirkRowViewModel>();

        /// <summary>Gets the fixed three disease slots; unused slots are empty placeholders.</summary>
        public ObservableCollection<DiseaseRowViewModel> Diseases { get; } = new ObservableCollection<DiseaseRowViewModel>
        {
            new DiseaseRowViewModel(string.Empty),
            new DiseaseRowViewModel(string.Empty),
            new DiseaseRowViewModel(string.Empty),
        };

        /// <summary>Gets the two trinket slots (left then right).</summary>
        public ObservableCollection<LobbyTrinketViewModel> TrinketSlots { get; } = new ObservableCollection<LobbyTrinketViewModel>();

        /// <summary>Gets the stat sheet preview of the selected class.</summary>
        public HeroStatsViewModel Stats { get; } = new HeroStatsViewModel();

        /// <summary>Gets the command that cycles to the previous class.</summary>
        public IRelayCommand PrevClassCommand { get; }

        /// <summary>Gets the command that cycles to the next class.</summary>
        public IRelayCommand NextClassCommand { get; }

        /// <summary>Gets the command that rerolls the hero name.</summary>
        public IRelayCommand RerollNameCommand { get; }

        /// <summary>Gets the command that starts inline hero-name editing.</summary>
        public IRelayCommand EditNameCommand { get; }

        /// <summary>Gets the command that advances to the next skin when more than one exists.</summary>
        public IRelayCommand NextSkinCommand { get; }

        /// <summary>Gets the command that toggles an active combat skill.</summary>
        public IRelayCommand<LobbySkillViewModel> ToggleSkillCommand { get; }

        /// <summary>Gets the command that toggles a selected camping skill.</summary>
        public IRelayCommand<LobbyCampingSkillViewModel> ToggleCampingSkillCommand { get; }

        /// <summary>Gets the command that rerolls the hero's quirks.</summary>
        public IRelayCommand RerollQuirksCommand { get; }

        /// <summary>Gets the command that assigns two random trinkets valid for the hero class.</summary>
        public IRelayCommand RerollTrinketsCommand { get; }

        /// <summary>Gets the ids of the active combat skills.</summary>
        public IReadOnlyList<string> SelectedSkillIds
        {
            get { return Skills.Where(skill => skill.IsActive).Select(skill => skill.Id).ToList(); }
        }

        /// <summary>Gets the ids of the selected camping skills.</summary>
        public IReadOnlyList<string> SelectedCampingSkillIds
        {
            get { return CampingSkills.Where(skill => skill.IsActive).Select(skill => skill.Id).ToList(); }
        }

        /// <summary>Gets the ids of the chosen quirks.</summary>
        public IReadOnlyList<string> SelectedQuirkIds
        {
            get { return Quirks.Select(quirk => quirk.Id).ToList(); }
        }

        /// <summary>Gets the positive quirk rows shown in the editor's left quirk column.</summary>
        public IReadOnlyList<QuirkRowViewModel> PositiveQuirks
        {
            get { return PaddedQuirks(isPositive: true); }
        }

        /// <summary>Gets the negative quirk rows shown in the editor's right quirk column.</summary>
        public IReadOnlyList<QuirkRowViewModel> NegativeQuirks
        {
            get { return PaddedQuirks(isPositive: false); }
        }

        private IReadOnlyList<QuirkRowViewModel> PaddedQuirks(bool isPositive)
        {
            var rows = QuirkRows.Where(quirk => quirk.IsPositive == isPositive).ToList();
            while (rows.Count < QuirkVisibleRows)
                rows.Add(QuirkRowViewModel.Empty(isPositive));
            return rows;
        }

        /// <summary>Gets the ids of the equipped trinkets.</summary>
        public IReadOnlyList<string> SelectedTrinketIds
        {
            get { return TrinketSlots.Where(slot => slot.TrinketId.Length > 0).Select(slot => slot.TrinketId).ToList(); }
        }

        /// <summary>Initializes a new instance of the <see cref="HeroSlotViewModel"/> class.</summary>
        /// <param name="seed">The deterministic seed.</param>
        /// <param name="availableClasses">The selectable class ids.</param>
        /// <param name="iconCatalog">The catalog of hero portraits, or null when icons are not available.</param>
        /// <param name="skillCatalog">The catalog of combat skill icons and art, or null when icons are not available.</param>
        /// <param name="position">The 1-based formation position of this slot.</param>
        /// <param name="spineCatalog">The Spine content catalog, or null when the Idle animation is not available.</param>
        public HeroSlotViewModel(
            int seed,
            IReadOnlyList<string> availableClasses,
            IHeroIconCatalog? iconCatalog = null,
            IHeroSkillCatalog? skillCatalog = null,
            int position = 0,
            ISpineDemoCatalog? spineCatalog = null)
        {
            this.availableClasses = availableClasses;
            this.iconCatalog = iconCatalog;
            this.skillCatalog = skillCatalog;
            this.spineCatalog = spineCatalog;
            Seed = seed;
            Position = position;
            _classId = this.availableClasses[0];
            PrevClassCommand = new RelayCommand(PrevClass);
            NextClassCommand = new RelayCommand(NextClass);
            RerollNameCommand = new RelayCommand(RerollName);
            EditNameCommand = new RelayCommand(BeginEditName);
            NextSkinCommand = new RelayCommand(NextSkin);
            ToggleSkillCommand = new RelayCommand<LobbySkillViewModel>(ToggleSkill);
            ToggleCampingSkillCommand = new RelayCommand<LobbyCampingSkillViewModel>(ToggleCampingSkill);
            RerollQuirksCommand = new RelayCommand(RerollQuirks);
            RerollTrinketsCommand = new RelayCommand(RerollTrinkets);
            RerollName();
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

        private void RerollName()
        {
            IsEditingName = false;
            HeroName = Data.HeroNameCatalog.RandomName(rng) ?? "Hero";
        }

        private void BeginEditName()
        {
            IsEditingName = true;
        }

        private void NormalizeHeroName()
        {
            string name = (HeroName ?? string.Empty).Trim();
            if (name.Length == 0)
                name = "Hero";
            if (name.Length > MaxHeroNameLength)
                name = name.Substring(0, MaxHeroNameLength);
            HeroName = name;
            Stats.HeroName = name;
        }

        private void NextSkin()
        {
            if (SkinCount <= 1)
                return;
            SkinId = SkinId >= SkinCount ? 1 : SkinId + 1;
        }

        partial void OnHeroNameChanged(string value)
        {
            Stats.HeroName = value;
        }

        partial void OnIsEditingNameChanged(bool value)
        {
            if (!value)
                NormalizeHeroName();
        }

        private void ToggleSkill(LobbySkillViewModel? skill)
        {
            if (skill == null || !CanSelectSkills)
                return;

            if (skill.IsActive)
            {
                skill.IsActive = false;
                RefreshSkillAvailability();
                return;
            }

            if (Skills.Count(active => active.IsActive) >= MaxActiveSkills)
                return;

            skill.IsActive = true;
            RefreshSkillAvailability();
        }

        private void ToggleCampingSkill(LobbyCampingSkillViewModel? skill)
        {
            if (skill == null)
                return;

            if (skill.IsActive)
            {
                skill.IsActive = false;
                RefreshCampingSkillAvailability();
                return;
            }

            if (CampingSkills.Count(active => active.IsActive) >= MaxCampingSkills)
                return;

            skill.IsActive = true;
            RefreshCampingSkillAvailability();
        }

        private void RerollQuirks()
        {
            Quirks.Clear();
            AddRandomQuirk(Data.QuirkCatalog.Positive);
            AddRandomQuirk(Data.QuirkCatalog.Negative);
            QuirkSummary = Quirks.Count == 0
                ? "no quirks"
                : string.Join(", ", Quirks.Select(q => (q.IsPositive ? "+" : "-") + q.Id));

            // The lobby has no save-backed lock flags yet: keep the rolled positive quirk as the
            // innate locked one so the editor shows both lock states. Negative rows support the
            // same flag when a save supplies it.
            QuirkRows.Clear();
            foreach (Quirk quirk in Quirks)
                QuirkRows.Add(new QuirkRowViewModel(quirk, isLocked: quirk.IsPositive));
            OnPropertyChanged(nameof(PositiveQuirks));
            OnPropertyChanged(nameof(NegativeQuirks));
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
            CanSelectSkills = heroClass != null && heroClass.CanSelectCombatSkills;
            MaxActiveSkills = heroClass != null && heroClass.NumberOfSelectedCombatSkills > 0
                ? heroClass.NumberOfSelectedCombatSkills
                : 4;
            Portrait = iconCatalog?.GetPortrait(ClassId, PortraitOutfit);
            Header = iconCatalog?.GetHeader(ClassId);

            Skills.Clear();
            var skillInfos = skillCatalog?.GetSkills(ClassId);
            if (skillInfos != null && skillInfos.Count > 0)
            {
                for (int i = 0; i < skillInfos.Count; i++)
                {
                    var info = skillInfos[i];
                    Skills.Add(new LobbySkillViewModel(info.Skill, i < MaxActiveSkills, info.Icon));
                }
            }
            else
            {
                var allSkills = heroClass?.CombatSkills ?? new List<CombatSkill>();
                for (int i = 0; i < allSkills.Count; i++)
                    Skills.Add(new LobbySkillViewModel(allSkills[i], i < MaxActiveSkills, null));
            }

            RefreshSkillAvailability();
            LoadCampingSkills();
            IdleContentDirectory = ResolveIdleContentDirectory();

            ReloadTrinkets();
            Details = BuildDetails(heroClass);
            ApplyStats(heroClass);
            RerollQuirks();
        }

        private void RefreshSkillAvailability()
        {
            int activeCount = Skills.Count(skill => skill.IsActive);
            bool canSelectMore = CanSelectSkills && activeCount < MaxActiveSkills;
            foreach (var skill in Skills)
                skill.IsUsable = skill.IsActive || canSelectMore;
        }

        private void LoadCampingSkills()
        {
            CampingSkills.Clear();
            var campingSkills = Data.CampingSkillCatalog.GetForClass(ClassId);
            for (int i = 0; i < campingSkills.Count; i++)
            {
                var skill = campingSkills[i];
                CampingSkills.Add(new LobbyCampingSkillViewModel(
                    skill,
                    i < MaxCampingSkills,
                    Data.CampingSkillCatalog.GetIcon(skill.Id)));
            }

            RefreshCampingSkillAvailability();
        }

        private void RefreshCampingSkillAvailability()
        {
            int activeCount = CampingSkills.Count(skill => skill.IsActive);
            bool canSelectMore = activeCount < MaxCampingSkills;
            foreach (var skill in CampingSkills)
                skill.IsUsable = skill.IsActive || canSelectMore;
        }

        private string ResolveIdleContentDirectory()
        {
            if (spineCatalog == null)
                return string.Empty;

            string heroFolder = Ui.HeroIds.ToFolderName(ClassId);
            if (string.IsNullOrEmpty(heroFolder))
                return string.Empty;

            string directory = spineCatalog.GetContentDirectory(heroFolder, "Idle");
            return Directory.Exists(directory) ? directory : string.Empty;
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
            Stats.HeroName = HeroName;
            Stats.HeroClass = heroClass == null ? string.Empty : Ui.DisplayNames.Class(heroClass.StringId);
            Stats.Rank = Position;
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
            Stats.ResistStun = ResistPct(heroClass, AttributeType.Stun);
            Stats.ResistBlight = ResistPct(heroClass, AttributeType.Poison);
            Stats.ResistBleed = ResistPct(heroClass, AttributeType.Bleed);
            Stats.ResistDebuff = ResistPct(heroClass, AttributeType.Debuff);
            Stats.ResistMove = ResistPct(heroClass, AttributeType.Move);
            Stats.ResistDisease = ResistPct(heroClass, AttributeType.Disease);
            Stats.ResistDeathBlow = ResistPct(heroClass, AttributeType.DeathBlow);
            Stats.ResistTrap = ResistPct(heroClass, AttributeType.Trap);
            Stats.WeaponLevel = "1";
            Stats.ArmorLevel = "1";
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

        private static int ResistPct(HeroClass heroClass, AttributeType type)
        {
            float value;
            return heroClass.Resistances.TryGetValue(type, out value) ? (int)(value * 100) : 0;
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

        partial void OnCanSelectSkillsChanged(bool value)
        {
            OnPropertyChanged(nameof(SkillsLocked));
        }

        partial void OnClassIdChanged(string value)
        {
            OnPropertyChanged(nameof(ClassName));
        }
    }
}