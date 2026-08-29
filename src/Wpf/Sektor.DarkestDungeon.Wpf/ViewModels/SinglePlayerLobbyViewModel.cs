using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.Navigation;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Single player lobby: build both parties (player and AI) and fight a duel against the AI.</summary>
    public partial class SinglePlayerLobbyViewModel : ObservableObject
    {
        private static readonly Random Rng = new Random();

        private readonly INavigationService navigation;
        private readonly IReadOnlyList<string> availableClasses;

        /// <summary>Gets the four hero slots of the player's party.</summary>
        public ObservableCollection<HeroSlotViewModel> Slots { get; } = new ObservableCollection<HeroSlotViewModel>();

        /// <summary>Gets the four hero slots of the AI party (editable like the player's).</summary>
        public ObservableCollection<HeroSlotViewModel> AiSlots { get; } = new ObservableCollection<HeroSlotViewModel>();

        /// <summary>Gets the command that rerolls the AI party with random distinct classes.</summary>
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
            for (int i = 0; i < 4; i++)
                AiSlots.Add(new HeroSlotViewModel(i * 10 + 101, availableClasses));

            RandomizeRivalCommand = new RelayCommand(RandomizeRival);
            StartCommand = new RelayCommand(Start);
            BackCommand = new RelayCommand(Back);
            AssignDistinct(Slots);
            RandomizeRival();
        }

        private void RandomizeRival()
        {
            AssignDistinct(AiSlots);
        }

        private void Start()
        {
            int sessionSeed = Environment.TickCount;
            var duel = new DuelController(new DuelContent());
            duel.StartDuel(ToPicks(Slots), ToPicks(AiSlots), sessionSeed, isHost: true);
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

        private void AssignDistinct(IEnumerable<HeroSlotViewModel> slots)
        {
            var shuffled = availableClasses.OrderBy(_ => Rng.Next()).ToList();
            int index = 0;
            foreach (var slot in slots)
                slot.AssignClass(shuffled[index++ % shuffled.Count]);
        }

        private static DuelHeroPick[] ToPicks(IEnumerable<HeroSlotViewModel> slots)
        {
            return slots.Select(slot => new DuelHeroPick(slot.ClassId, slot.Seed, slot.SelectedSkillIds, slot.SelectedQuirkIds)).ToArray();
        }
    }
}