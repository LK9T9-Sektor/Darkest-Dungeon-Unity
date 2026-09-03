using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Core.Duel.Fight;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>
    /// PvE lobby: build a hero party and a monster side (total occupied ranks up to 4) and start a
    /// heroes-vs-monsters fight on the pure core (<see cref="DuelController.StartFight"/>).
    /// </summary>
    public partial class PveLobbyViewModel : ObservableObject
    {
        private const int MaxFormationRanks = 4;
        private const int SeedMin = 0;
        private const int SeedMax = 9999;

        private readonly INavigationService navigation;
        private readonly IReadOnlyList<string> availableClasses;
        private readonly ILogger logger;

        /// <summary>Gets the four hero slots of the player's party.</summary>
        public ObservableCollection<HeroSlotViewModel> Slots { get; } = new ObservableCollection<HeroSlotViewModel>();

        /// <summary>Gets the monster slots of the enemy side.</summary>
        public ObservableCollection<PveMonsterSlotViewModel> MonsterSlots { get; } = new ObservableCollection<PveMonsterSlotViewModel>();

        /// <summary>Gets or sets the deterministic battle seed.</summary>
        [ObservableProperty]
        private int _seed = 7;

        /// <summary>Gets the total occupied ranks of the selected monsters (must not exceed 4).</summary>
        public int MonsterTotalSize
        {
            get { return MonsterSlots.Where(slot => slot.MonsterId.Length > 0).Sum(slot => slot.Size); }
        }

        /// <summary>Gets a value indicating whether the monster side fits the formation (up to 4 ranks).</summary>
        public bool MonsterSideFits { get { return MonsterTotalSize <= MaxFormationRanks; } }

        /// <summary>Gets a value indicating whether the enemy side has at least one monster.</summary>
        public bool HasMonsters { get { return MonsterSlots.Any(slot => slot.MonsterId.Length > 0); } }

        /// <summary>Gets a value indicating whether the fight can start.</summary>
        public bool CanStart { get { return HasMonsters && MonsterSideFits; } }

        /// <summary>Gets the command that starts the fight.</summary>
        public IRelayCommand StartCommand { get; }

        /// <summary>Gets the command returning to the main menu.</summary>
        public IRelayCommand BackCommand { get; }

        /// <summary>Gets the command that lowers the seed.</summary>
        public IRelayCommand DecreaseSeedCommand { get; }

        /// <summary>Gets the command that raises the seed.</summary>
        public IRelayCommand IncreaseSeedCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="PveLobbyViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="availableClasses">The selectable hero class ids.</param>
        /// <param name="logger">The structural logger.</param>
        public PveLobbyViewModel(INavigationService navigation, IReadOnlyList<string> availableClasses, ILogger logger)
        {
            this.navigation = navigation;
            this.availableClasses = availableClasses;
            this.logger = logger;

            for (int i = 0; i < 4; i++)
                Slots.Add(new HeroSlotViewModel(i * 10 + 1, availableClasses));

            var monsters = DuelContent.MonsterCatalog;
            var ids = monsters.Ids;
            string[] candidates = DefaultMonsterSide(ids);
            for (int i = 0; i < 4; i++)
                MonsterSlots.Add(new PveMonsterSlotViewModel(monsters, i < candidates.Length ? candidates[i] : (ids.Count > 0 ? ids[0] : string.Empty)));

            foreach (var slot in MonsterSlots)
            {
                slot.PropertyChanged += (s, e) => RefreshMonsterState();
            }

            StartCommand = new RelayCommand(Start);
            BackCommand = new RelayCommand(Back);
            DecreaseSeedCommand = new RelayCommand(() => Seed = Math.Max(SeedMin, Seed - 1));
            IncreaseSeedCommand = new RelayCommand(() => Seed = Math.Min(SeedMax, Seed + 1));
            RefreshMonsterState();
        }

        private static string[] DefaultMonsterSide(IReadOnlyList<string> ids)
        {
            if (ids.Count == 0)
                return new string[0];

            var result = new List<string>();
            int used = 0;
            foreach (var id in ids)
            {
                if (used >= MaxFormationRanks)
                    break;
                if (DuelContent.MonsterCatalog.TryGet(id, out var monster))
                {
                    result.Add(id);
                    used += monster.Size;
                }
            }

            return result.ToArray();
        }

        private void RefreshMonsterState()
        {
            OnPropertyChanged(nameof(MonsterTotalSize));
            OnPropertyChanged(nameof(MonsterSideFits));
            OnPropertyChanged(nameof(HasMonsters));
            OnPropertyChanged(nameof(CanStart));
        }

        private void Start()
        {
            if (!CanStart)
                return;

            var heroSpecs = Slots
                .Select(slot => new HeroFightUnitSpec(
                    slot.ClassId,
                    slot.Seed,
                    slot.SelectedSkillIds,
                    slot.SelectedQuirkIds,
                    slot.SelectedTrinketIds))
                .Cast<FightUnitSpec>()
                .ToList();

            var monsterSpecs = MonsterSlots
                .Where(slot => slot.MonsterId.Length > 0)
                .Select(slot => (FightUnitSpec)new MonsterFightUnitSpec(slot.MonsterId))
                .ToList();

            var duel = new DuelController(new DuelContent(), logger);
            duel.StartFight(heroSpecs, monsterSpecs, Seed);
            if (!duel.IsStarted)
                return;
            duel.StartBattle();

            var battle = new PveBattleViewModel(duel, () => navigation.GoHome());
            navigation.NavigateTo(battle);
        }

        private void Back()
        {
            navigation.GoHome();
        }
    }
}