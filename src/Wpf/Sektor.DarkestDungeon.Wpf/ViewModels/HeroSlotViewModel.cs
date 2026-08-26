using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Wpf.Combat;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single hero slot in the duel lobby with class cycling.</summary>
    public partial class HeroSlotViewModel : ObservableObject
    {
        private static readonly string[] AvailableClasses =
            new[] { "crusader", "highwayman", "plague_doctor", "vestal" };

        /// <summary>Gets the deterministic seed of this slot.</summary>
        public int Seed { get; }

        /// <summary>Gets or sets the selected class id.</summary>
        [ObservableProperty]
        private string _classId;

        /// <summary>Gets or sets a value indicating whether the slot is empty.</summary>
        [ObservableProperty]
        private bool _isEmpty = true;

        /// <summary>Gets the command that cycles to the previous class.</summary>
        public IRelayCommand PrevClassCommand { get; }

        /// <summary>Gets the command that cycles to the next class.</summary>
        public IRelayCommand NextClassCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="HeroSlotViewModel"/> class.</summary>
        /// <param name="seed">The deterministic seed.</param>
        public HeroSlotViewModel(int seed)
        {
            Seed = seed;
            _classId = AvailableClasses[0];
            PrevClassCommand = new RelayCommand(PrevClass);
            NextClassCommand = new RelayCommand(NextClass);
        }

        /// <summary>Gets the display name of the selected class.</summary>
        public string ClassName { get { return ClassId; } }

        private void PrevClass()
        {
            int index = System.Array.IndexOf(AvailableClasses, ClassId);
            ClassId = AvailableClasses[(index - 1 + AvailableClasses.Length) % AvailableClasses.Length];
            IsEmpty = false;
        }

        private void NextClass()
        {
            int index = System.Array.IndexOf(AvailableClasses, ClassId);
            ClassId = AvailableClasses[(index + 1) % AvailableClasses.Length];
            IsEmpty = false;
        }

        partial void OnClassIdChanged(string value)
        {
            OnPropertyChanged(nameof(ClassName));
        }
    }
}