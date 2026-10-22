using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Ui;

using TrinketContent = Sektor.DarkestDungeon.Core.Content.Trinket.Trinket;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Read-only unit-info adapter for a live battle unit. The sheet is filled from the
    /// current combat snapshot, while the polymorphic capabilities decide which controls the overlay
    /// may show or edit.</summary>
    public abstract partial class BattleUnitInfoViewModel : UnitInfoViewModelBase
    {
        private const string PortraitOutfit = "A";
        private const int QuirkVisibleRows = 5;
        private const int DiseaseSlotCount = 3;

        private readonly Random rng = new Random();
        private readonly DuelUnitViewModel unit;

        /// <summary>Gets or sets the hero name shown in the identity block.</summary>
        [ObservableProperty]
        private string _heroName = string.Empty;

        /// <summary>Gets or sets a value indicating whether the name is being edited.</summary>
        [ObservableProperty]
        private bool _isEditingName;

        /// <summary>Gets the stat sheet of the inspected unit.</summary>
        public HeroStatsViewModel Stats { get; }

        /// <summary>Gets the current formation position.</summary>
        public int Position { get; }

        /// <summary>Gets the level shown in the medallion.</summary>
        public int Level { get; } = 1;

        /// <summary>Gets or sets the current skin id.</summary>
        [ObservableProperty]
        private int _skinId = 1;

        /// <summary>Gets the number of available skins.</summary>
        public int SkinCount { get; } = 1;

        /// <summary>Gets the maximum hero-name length accepted by the overlay.</summary>
        public int MaxHeroNameLength { get; } = 24;

        /// <summary>Gets the portrait used by the Idle fallback.</summary>
        public ImageSource? Portrait { get; }

        /// <summary>Gets the guild header art of the unit's class.</summary>
        public ImageSource? Header { get; }

        /// <summary>Gets the Idle Spine content directory, or an empty string when unavailable.</summary>
        public string IdleContentDirectory { get; }

        /// <summary>Gets the live combat skills shown in the sheet.</summary>
        public ObservableCollection<DuelSkillViewModel> Skills { get; } = new ObservableCollection<DuelSkillViewModel>();

        /// <summary>Gets the class camping skills shown in the sheet.</summary>
        public ObservableCollection<LobbyCampingSkillViewModel> CampingSkills { get; } = new ObservableCollection<LobbyCampingSkillViewModel>();

        /// <summary>Gets the quirk rows, including reserved empty rows.</summary>
        public ObservableCollection<QuirkRowViewModel> QuirkRows { get; } = new ObservableCollection<QuirkRowViewModel>();

        /// <summary>Gets the fixed disease slots.</summary>
        public ObservableCollection<DiseaseRowViewModel> Diseases { get; } = new ObservableCollection<DiseaseRowViewModel>();

        /// <summary>Gets the two trinket slots.</summary>
        public ObservableCollection<LobbyTrinketViewModel> TrinketSlots { get; } = new ObservableCollection<LobbyTrinketViewModel>();

        /// <summary>Gets the command that enters name editing.</summary>
        public IRelayCommand EditNameCommand { get; }

        /// <summary>Gets the command that rerolls the name.</summary>
        public IRelayCommand RerollNameCommand { get; }

        /// <summary>Gets the no-op skin command (the battle snapshot carries one skin).</summary>
        public IRelayCommand NextSkinCommand { get; }

        /// <summary>Gets no previous-class command; battle stats do not switch the loadout.</summary>
        public IRelayCommand? PrevClassCommand { get { return null; } }

        /// <summary>Gets no next-class command; battle stats do not switch the loadout.</summary>
        public IRelayCommand? NextClassCommand { get { return null; } }

        /// <summary>Gets no combat-skill toggle; battle skill tiles are read-only.</summary>
        public IRelayCommand? ToggleSkillCommand { get { return null; } }

        /// <summary>Gets no camping-skill toggle; battle skill tiles are read-only.</summary>
        public IRelayCommand? ToggleCampingSkillCommand { get { return null; } }

        /// <summary>Gets the positive quirk rows shown in the left column.</summary>
        public IReadOnlyList<QuirkRowViewModel> PositiveQuirks { get { return PaddedQuirks(isPositive: true); } }

        /// <summary>Gets the negative quirk rows shown in the right column.</summary>
        public IReadOnlyList<QuirkRowViewModel> NegativeQuirks { get { return PaddedQuirks(isPositive: false); } }

        /// <summary>Initializes a new instance of the <see cref="BattleUnitInfoViewModel"/> class.</summary>
        /// <param name="unit">The live battle unit.</param>
        /// <param name="stats">The already filled stat snapshot.</param>
        /// <param name="iconCatalog">Optional hero icon catalog.</param>
        /// <param name="skillCatalog">Optional skill-icon catalog.</param>
        /// <param name="spineCatalog">Optional Spine content catalog.</param>
        protected BattleUnitInfoViewModel(
            DuelUnitViewModel unit,
            HeroStatsViewModel stats,
            IHeroIconCatalog? iconCatalog,
            IHeroSkillCatalog? skillCatalog,
            ISpineDemoCatalog? spineCatalog)
        {
            this.unit = unit;
            Stats = stats;
            Stats.HeroName = unit.Name;
            Stats.HeroClass = unit.ClassName;
            Stats.Rank = unit.Rank;

            HeroName = unit.Name;
            Position = unit.Rank;
            Portrait = iconCatalog?.GetPortrait(unit.ClassId, PortraitOutfit) ?? unit.Portrait;
            Header = iconCatalog?.GetHeader(unit.ClassId);
            IdleContentDirectory = ResolveIdleContentDirectory(spineCatalog, unit.ClassId);
            SkinId = 1;

            BuildSkills(unit, skillCatalog);
            BuildCampingSkills(unit.ClassId);
            BuildQuirks(unit);
            BuildTrinkets(unit);

            for (int i = 0; i < DiseaseSlotCount; i++)
                Diseases.Add(new DiseaseRowViewModel(string.Empty));

            EditNameCommand = new RelayCommand(BeginEditName);
            RerollNameCommand = new RelayCommand(RerollName);
            NextSkinCommand = new RelayCommand(NextSkin);
        }

        /// <summary>Creates the correct sheet subtype for the inspected unit.</summary>
        /// <param name="unit">The live battle unit.</param>
        /// <param name="stats">The already filled stat snapshot.</param>
        /// <param name="iconCatalog">Optional hero icon catalog.</param>
        /// <param name="skillCatalog">Optional skill-icon catalog.</param>
        /// <param name="spineCatalog">Optional Spine content catalog.</param>
        /// <returns>A hero or monster sheet.</returns>
        public static BattleUnitInfoViewModel Create(
            DuelUnitViewModel unit,
            HeroStatsViewModel stats,
            IHeroIconCatalog? iconCatalog,
            IHeroSkillCatalog? skillCatalog,
            ISpineDemoCatalog? spineCatalog)
        {
            if (unit.IsHero)
                return new BattleHeroInfoViewModel(unit, stats, iconCatalog, skillCatalog, spineCatalog);
            return new BattleMonsterInfoViewModel(unit, stats, iconCatalog, skillCatalog, spineCatalog);
        }

        private void BeginEditName()
        {
            if (CanEditName)
                IsEditingName = true;
        }

        private void RerollName()
        {
            if (!CanRerollName)
                return;

            IsEditingName = false;
            HeroName = HeroNameCatalog.RandomName(rng) ?? HeroName;
        }

        private void NormalizeHeroName()
        {
            string name = (HeroName ?? string.Empty).Trim();
            if (name.Length == 0)
                name = "Unit";
            if (name.Length > MaxHeroNameLength)
                name = name.Substring(0, MaxHeroNameLength);
            HeroName = name;
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
            if (CanEditName)
                unit.Name = value;
        }

        partial void OnIsEditingNameChanged(bool value)
        {
            if (!value)
                NormalizeHeroName();
        }

        private void BuildSkills(DuelUnitViewModel unit, IHeroSkillCatalog? skillCatalog)
        {
            foreach (var skill in unit.CombatSkills)
            {
                Skills.Add(new DuelSkillViewModel(
                    skill.Id,
                    DisplayNames.Title(skill.Id),
                    SkillToneClassifier.Classify(skill))
                {
                    IsUsable = true,
                    IsSelected = true,
                    Level = skill.Level > 0 ? skill.Level : 1,
                    BaseInfo = SkillDetails.BuildBaseInfo(skill),
                    EffectRows = SkillDetails.BuildEffectRows(skill),
                    Details = SkillDetails.Build(skill),
                    Icon = skillCatalog?.GetSkillIcon(unit.ClassId, skill.Id),
                });
            }

            if (Skills.Count > 0 || skillCatalog == null)
                return;

            foreach (var info in skillCatalog.GetSkills(unit.ClassId))
            {
                var skill = info.Skill;
                Skills.Add(new DuelSkillViewModel(
                    skill.Id,
                    DisplayNames.Title(skill.Id),
                    SkillToneClassifier.Classify(skill))
                {
                    IsUsable = true,
                    IsSelected = true,
                    Level = skill.Level > 0 ? skill.Level : 1,
                    BaseInfo = SkillDetails.BuildBaseInfo(skill),
                    EffectRows = SkillDetails.BuildEffectRows(skill),
                    Details = SkillDetails.Build(skill),
                    Icon = info.Icon,
                });
            }
        }

        private void BuildCampingSkills(string classId)
        {
            var camping = CampingSkillCatalog.GetForClass(classId);
            foreach (var skill in camping)
            {
                CampingSkills.Add(new LobbyCampingSkillViewModel(
                    skill,
                    isActive: true,
                    CampingSkillCatalog.GetIcon(skill.Id)));
            }
        }

        private void BuildQuirks(DuelUnitViewModel unit)
        {
            foreach (string quirkId in unit.QuirkIds)
            {
                var quirk = QuirkCatalog.Get(quirkId);
                if (quirk != null)
                    QuirkRows.Add(new QuirkRowViewModel(quirk, isLocked: false));
            }
        }

        private void BuildTrinkets(DuelUnitViewModel unit)
        {
            var pool = new List<TrinketContent>();
            if (unit.IsHero)
            {
                foreach (var trinket in TrinketCatalog.All)
                {
                    if (trinket.HeroClassRequirements.Count == 0
                        || trinket.HeroClassRequirements.Contains(unit.ClassId))
                    {
                        pool.Add(trinket);
                    }
                }
            }

            for (int i = 0; i < 2; i++)
            {
                var slot = new LobbyTrinketViewModel(pool);
                if (i < unit.EquippedTrinketIds.Count)
                    slot.Select(unit.EquippedTrinketIds[i]);
                TrinketSlots.Add(slot);
            }
        }

        private IReadOnlyList<QuirkRowViewModel> PaddedQuirks(bool isPositive)
        {
            var rows = QuirkRows.Where(quirk => quirk.IsPositive == isPositive).ToList();
            while (rows.Count < QuirkVisibleRows)
                rows.Add(QuirkRowViewModel.Empty(isPositive));
            return rows;
        }

        private static string ResolveIdleContentDirectory(ISpineDemoCatalog? spineCatalog, string classId)
        {
            if (spineCatalog == null)
                return string.Empty;

            string heroFolder = HeroIds.ToFolderName(classId);
            if (string.IsNullOrEmpty(heroFolder))
                return string.Empty;

            string directory = spineCatalog.GetContentDirectory(heroFolder, "Idle");
            return Directory.Exists(directory) ? directory : string.Empty;
        }
    }

    /// <summary>Battle sheet for a hero: name and skin stay editable, loadout controls do not.</summary>
    public sealed class BattleHeroInfoViewModel : BattleUnitInfoViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="BattleHeroInfoViewModel"/> class.</summary>
        public BattleHeroInfoViewModel(
            DuelUnitViewModel unit,
            HeroStatsViewModel stats,
            IHeroIconCatalog? iconCatalog,
            IHeroSkillCatalog? skillCatalog,
            ISpineDemoCatalog? spineCatalog)
            : base(unit, stats, iconCatalog, skillCatalog, spineCatalog)
        {
        }

        /// <inheritdoc/>
        public override bool CanEditName { get { return true; } }

        /// <inheritdoc/>
        public override bool CanRerollName { get { return true; } }

        /// <inheritdoc/>
        public override bool CanChangeSkin { get { return true; } }
    }

    /// <summary>Battle sheet for a monster: everything is read-only.</summary>
    public sealed class BattleMonsterInfoViewModel : BattleUnitInfoViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="BattleMonsterInfoViewModel"/> class.</summary>
        public BattleMonsterInfoViewModel(
            DuelUnitViewModel unit,
            HeroStatsViewModel stats,
            IHeroIconCatalog? iconCatalog,
            IHeroSkillCatalog? skillCatalog,
            ISpineDemoCatalog? spineCatalog)
            : base(unit, stats, iconCatalog, skillCatalog, spineCatalog)
        {
        }
    }
}
