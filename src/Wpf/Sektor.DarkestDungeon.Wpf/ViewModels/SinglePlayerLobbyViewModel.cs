using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Core.Duel.Fight;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Single player lobby: build both parties (player and AI) and fight a duel against the AI.</summary>
    public partial class SinglePlayerLobbyViewModel : ObservableObject
    {
        private const string PortraitOutfit = "A";

        private static readonly Random Rng = new Random();

        private readonly INavigationService navigation;
        private readonly IReadOnlyList<string> availableClasses;
        private readonly ILogger logger;
        private readonly IHeroSkillCatalog skillCatalog;
        private readonly IHeroIconCatalog iconCatalog;
        private readonly ISpineDemoCatalog? spineCatalog;

        /// <summary>Gets the four hero slots of the player's party.</summary>
        public ObservableCollection<HeroSlotViewModel> Slots { get; } = new ObservableCollection<HeroSlotViewModel>();

        /// <summary>Gets the four hero slots of the AI party (editable like the player's).</summary>
        public ObservableCollection<HeroSlotViewModel> AiSlots { get; } = new ObservableCollection<HeroSlotViewModel>();

        /// <summary>Gets the hero rows shown on the Heroes tab of the selection table.</summary>
        public IReadOnlyList<UnitSelectionRowViewModel> HeroRows { get; }

        /// <summary>Gets the monster rows shown on the Monsters tab of the selection table.</summary>
        public IReadOnlyList<UnitSelectionRowViewModel> MonsterRows { get; }

        /// <summary>Gets the command that assigns a double-clicked table row to the selected slot.</summary>
        public IRelayCommand<UnitSelectionRowViewModel> ActivateRowCommand { get; }

        /// <summary>Gets or sets the currently selected slot, on either side.</summary>
        public HeroSlotViewModel? SelectedSlot
        {
            get { return _selectedSlot; }
            private set
            {
                if (ReferenceEquals(_selectedSlot, value))
                    return;

                if (_selectedSlot != null)
                    _selectedSlot.IsSelected = false;

                _selectedSlot = value;

                if (_selectedSlot != null)
                    _selectedSlot.IsSelected = true;

                OnPropertyChanged();
                SelectionStatus = value == null
                    ? "Select a slot, then double-click a table row."
                    : "Slot " + value.Position + " selected. Double-click a table row to assign.";
            }
        }

        /// <summary>Gets or sets the table hint/status line.</summary>
        [ObservableProperty]
        private string _selectionStatus = "Select a slot, then double-click a table row.";

        private HeroSlotViewModel? _selectedSlot;

        /// <summary>Gets the command that rerolls the AI party with random distinct classes.</summary>
        public IRelayCommand RandomizeRivalCommand { get; }

        /// <summary>Gets the command that starts the local duel.</summary>
        public IRelayCommand StartCommand { get; }

        /// <summary>Gets the command returning to the main menu.</summary>
        public IRelayCommand BackCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="SinglePlayerLobbyViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="availableClasses">The selectable hero class ids.</param>
        /// <param name="logger">The structural logger.</param>
        /// <param name="skillCatalog">The catalog of hero skill icons for the battle tray.</param>
        /// <param name="iconCatalog">The catalog of hero portraits for the lobby slots.</param>
        /// <param name="spineCatalog">The Spine content catalog for the hero editor Idle animation.</param>
        public SinglePlayerLobbyViewModel(
            INavigationService navigation,
            IReadOnlyList<string> availableClasses,
            ILogger logger,
            IHeroSkillCatalog skillCatalog,
            IHeroIconCatalog iconCatalog,
            ISpineDemoCatalog? spineCatalog = null)
        {
            this.navigation = navigation;
            this.availableClasses = availableClasses;
            this.logger = logger;
            this.skillCatalog = skillCatalog;
            this.iconCatalog = iconCatalog;
            this.spineCatalog = spineCatalog;
            BuildSlots(Slots, 1);
            BuildSlots(AiSlots, 101);
            HeroRows = BuildHeroRows();
            MonsterRows = BuildMonsterRows();

            RandomizeRivalCommand = new RelayCommand(RandomizeRival);
            StartCommand = new RelayCommand(Start);
            BackCommand = new RelayCommand(Back);
            ActivateRowCommand = new RelayCommand<UnitSelectionRowViewModel>(ActivateRow);
            AssignDistinct(Slots);
            RandomizeRival();
            SelectedSlot = Slots[0];
        }

        private void RandomizeRival()
        {
            AssignDistinct(AiSlots);
        }

        private void BuildSlots(ObservableCollection<HeroSlotViewModel> slots, int seedBase)
        {
            for (int i = 0; i < 4; i++)
            {
                slots.Add(new HeroSlotViewModel(
                    seedBase + i * 10,
                    availableClasses,
                    iconCatalog,
                    skillCatalog,
                    i + 1,
                    spineCatalog,
                    SelectSlot));
            }
        }

        private void SelectSlot(HeroSlotViewModel slot)
        {
            SelectedSlot = slot;
        }

        private IReadOnlyList<UnitSelectionRowViewModel> BuildHeroRows()
        {
            var rows = new List<UnitSelectionRowViewModel>();
            foreach (string classId in availableClasses)
            {
                string id = classId;
                rows.Add(new UnitSelectionRowViewModel(
                    iconCatalog.GetPortrait(id, PortraitOutfit),
                    Ui.DisplayNames.Class(id),
                    1,
                    slot => slot.AssignClass(id)));
            }

            return rows;
        }

        private IReadOnlyList<UnitSelectionRowViewModel> BuildMonsterRows()
        {
            var rows = new List<UnitSelectionRowViewModel>();
            foreach (string monsterId in DuelContent.MonsterCatalog.Ids)
            {
                if (!DuelContent.MonsterCatalog.TryGet(monsterId, out var monster))
                    continue;

                string id = monsterId;
                rows.Add(new UnitSelectionRowViewModel(
                    null,
                    Ui.DisplayNames.Title(id),
                    monster.Size,
                    slot => slot.AssignMonster(id)));
            }

            return rows;
        }

        private void ActivateRow(UnitSelectionRowViewModel? row)
        {
            if (row == null)
                return;

            if (!row.IsSelectable)
            {
                SelectionStatus = "Only one-slot units are available for now.";
                return;
            }

            if (SelectedSlot == null)
            {
                SelectionStatus = "Select a slot first.";
                return;
            }

            row.AssignTo(SelectedSlot);
            SelectionStatus = "Assigned " + row.DisplayName + ".";
        }

        private void Start()
        {
            int sessionSeed = Environment.TickCount;
            var duel = new DuelController(new DuelContent(), logger);
            duel.StartFight(ToSpecs(Slots), ToSpecs(AiSlots), sessionSeed);
            if (!duel.IsStarted)
                return;
            duel.SetLocalTeam(Team.Heroes);
            duel.StartBattle();

            var link = new MonsterBrainRivalLink();
            var battle = new DuelBattleViewModel(duel, link, () =>
            {
                link.Dispose();
                navigation.GoHome();
            }, skillCatalog, iconCatalog, spineCatalog);
            navigation.NavigateTo(battle);
        }

        private void Back()
        {
            navigation.GoHome();
        }

        private void AssignDistinct(IEnumerable<HeroSlotViewModel> slots)
        {
            var shuffled = availableClasses.OrderBy(_ => Rng.Next()).ToList();
            int index = 0;
            foreach (var slot in slots)
                slot.AssignClass(shuffled[index++ % shuffled.Count]);
        }

        private static List<FightUnitSpec> ToSpecs(IEnumerable<HeroSlotViewModel> slots)
        {
            var specs = new List<FightUnitSpec>();
            foreach (var slot in slots)
            {
                if (slot.IsMonster)
                {
                    specs.Add(new MonsterFightUnitSpec(slot.MonsterId));
                    continue;
                }

                specs.Add(new HeroFightUnitSpec(
                    slot.ClassId,
                    slot.Seed,
                    slot.SelectedSkillIds,
                    slot.SelectedQuirkIds,
                    slot.SelectedTrinketIds));
            }

            return specs;
        }
    }
}