using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Wpf.Combat;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single hero slot in a lobby with class cycling.</summary>
    public partial class HeroSlotViewModel : ObservableObject
    {
        private readonly IReadOnlyList<string> availableClasses;

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
        /// <param name="availableClasses">The selectable class ids.</param>
        public HeroSlotViewModel(int seed, IReadOnlyList<string> availableClasses)
        {
            this.availableClasses = availableClasses;
            Seed = seed;
            _classId = this.availableClasses[0];
            PrevClassCommand = new RelayCommand(PrevClass);
            NextClassCommand = new RelayCommand(NextClass);
        }

        /// <summary>Gets the display name of the selected class.</summary>
        public string ClassName { get { return ClassId; } }

        private void PrevClass()
        {
            int index = IndexOf(ClassId);
            ClassId = availableClasses[(index - 1 + availableClasses.Count) % availableClasses.Count];
            IsEmpty = false;
        }

        private void NextClass()
        {
            int index = IndexOf(ClassId);
            ClassId = availableClasses[(index + 1) % availableClasses.Count];
            IsEmpty = false;
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
