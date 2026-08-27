using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Single player lobby: pick a party, fight a random hero party driven by AI.</summary>
    public partial class SinglePlayerLobbyViewModel : ObservableObject
    {
        private static readonly Random Rng = new Random();

        private readonly INavigationService navigation;
        private readonly IReadOnlyList<string> availableClasses;

        /// <summary>Gets the four hero slots.</summary>
        public ObservableCollection<HeroSlotViewModel> Slots { get; } = new ObservableCollection<HeroSlotViewModel>();

        /// <summary>Gets or sets the summary of the randomly generated rival party.</summary>
        [ObservableProperty]
        private string _rivalSummary = string.Empty;

        /// <summary>Gets the command that rerolls the AI party.</summary>
        public IRelayCommand RandomizeRivalCommand { get; }

        /// <summary>Gets the command that starts the local duel.</summary>
        public IRelayCommand StartCommand { get; }

        /// <summary>Gets the command returning to the main menu.</summary>
        public IRelayCommand BackCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="SinglePlayerLobbyViewModel"/> class.</summary>
        /// <param name="navigation">The navigation service.</param>
        /// <param name="availableClasses">The selectable hero class ids.</param>
        public SinglePlayerLobbyViewModel(INavigationService navigation, IReadOnlyList<string> availableClasses)
        {
            this.navigation = navigation;
            this.availableClasses = availableClasses;
            for (int i = 0; i < 4; i++)
                Slots.Add(new HeroSlotViewModel(i * 10 + 1, availableClasses));

            RandomizeRivalCommand = new RelayCommand(RandomizeRival);
            StartCommand = new RelayCommand(Start);
            BackCommand = new RelayCommand(Back);
            RandomizeRival();
        }

        private void RandomizeRival()
        {
            RivalSummary = "AI party: " + string.Join(", ", PickRandomParty());
        }

        private void Start()
        {
            int sessionSeed = Environment.TickCount;
            var duel = new DuelController();
            duel.StartDuel(ToPicks(Slots), RandomPicks(), sessionSeed, isHost: true);
            if (!duel.IsStarted)
                return;
            duel.StartBattle();

            var link = new AiRivalLink();
            var battle = new DuelBattleViewModel(duel, link, () =>
            {
                link.Dispose();
                navigation.GoHome();
            });
            navigation.NavigateTo(battle);
        }

        private void Back()
        {
            navigation.GoHome();
        }

        private List<string> PickRandomParty()
        {
            var pool = availableClasses.ToList();
            var picks = new List<string>();
            for (int i = 0; i < 4; i++)
                picks.Add(pool[Rng.Next(pool.Count)]);
            return picks;
        }

        private DuelHeroPick[] RandomPicks()
        {
            return PickRandomParty()
                .Select((classId, index) => new DuelHeroPick(classId, index * 7 + 13))
                .ToArray();
        }

        private static DuelHeroPick[] ToPicks(IEnumerable<HeroSlotViewModel> slots)
        {
            return slots.Select(slot => new DuelHeroPick(slot.ClassId, slot.Seed)).ToArray();
        }
    }
}
