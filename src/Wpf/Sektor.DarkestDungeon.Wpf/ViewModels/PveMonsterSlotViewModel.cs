using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Combat.Character;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single monster slot in the PvE lobby: cycles the monster class, shows name and size.</summary>
    public partial class PveMonsterSlotViewModel : ObservableObject
    {
        private readonly MonsterCatalog monsters;
        private int index;

        /// <summary>Gets the command that cycles to the previous monster.</summary>
        public IRelayCommand PrevCommand { get; }

        /// <summary>Gets the command that cycles to the next monster.</summary>
        public IRelayCommand NextCommand { get; }

        /// <summary>Gets or sets the selected monster id (empty = empty slot).</summary>
        [ObservableProperty]
        private string _monsterId = string.Empty;

        /// <summary>Gets the display name of the selected monster.</summary>
        public string DisplayName { get { return MonsterId.Length == 0 ? "- empty -" : MonsterId; } }

        /// <summary>Gets the occupied ranks of the selected monster (0 for an empty slot).</summary>
        public int Size
        {
            get
            {
                if (MonsterId.Length == 0)
                    return 0;
                MonsterClass monster;
                return monsters.TryGet(MonsterId, out monster) ? monster.Size : 0;
            }
        }

        /// <summary>Gets the size label for the slot ("[size 2]" or empty).</summary>
        public string SizeLabel { get { return Size > 1 ? "[size " + Size + "]" : string.Empty; } }

        /// <summary>Initializes a new instance of the <see cref="PveMonsterSlotViewModel"/> class.</summary>
        /// <param name="monsters">The monster catalog to cycle.</param>
        /// <param name="initialId">The initially selected monster id (or empty).</param>
        public PveMonsterSlotViewModel(MonsterCatalog monsters, string initialId)
        {
            this.monsters = monsters;
            PrevCommand = new RelayCommand(Prev);
            NextCommand = new RelayCommand(Next);
            MonsterId = initialId ?? string.Empty;
            index = -1;
            var ids = monsters.Ids;
            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == MonsterId)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
                index = ids.Count > 0 ? 0 : -1;
        }

        private void Prev()
        {
            Cycle(-1);
        }

        private void Next()
        {
            Cycle(1);
        }

        private void Cycle(int delta)
        {
            var ids = monsters.Ids;
            if (ids.Count == 0)
                return;

            int count = ids.Count;
            index = (index + delta + count) % count;
            MonsterId = ids[index];
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Size));
            OnPropertyChanged(nameof(SizeLabel));
        }
    }
}