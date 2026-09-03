using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sektor.DarkestDungeon.Core.Content.Trinket;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A single trinket slot in the lobby: cycles through the trinkets valid for the hero class.</summary>
    public partial class LobbyTrinketViewModel : ObservableObject
    {
        private IReadOnlyList<Trinket> pool;
        private int index = -1;

        /// <summary>Gets or sets the trinket id (empty when no trinket is equipped).</summary>
        [ObservableProperty]
        private string _trinketId = string.Empty;

        /// <summary>Gets or sets the trinket label shown in the slot.</summary>
        [ObservableProperty]
        private string _label = "-";

        /// <summary>Gets or sets the trinket tooltip text.</summary>
        [ObservableProperty]
        private string _details = string.Empty;

        /// <summary>Gets the command that cycles to the previous trinket.</summary>
        public IRelayCommand PrevCommand { get; }

        /// <summary>Gets the command that cycles to the next trinket.</summary>
        public IRelayCommand NextCommand { get; }

        /// <summary>Gets the command that clears the slot.</summary>
        public IRelayCommand ClearCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="LobbyTrinketViewModel"/> class.</summary>
        /// <param name="pool">The trinkets valid for the current hero class.</param>
        public LobbyTrinketViewModel(IReadOnlyList<Trinket> pool)
        {
            this.pool = pool;
            PrevCommand = new RelayCommand(Prev);
            NextCommand = new RelayCommand(Next);
            ClearCommand = new RelayCommand(Clear);
            Apply();
        }

        /// <summary>Refreshes the valid pool (called after the hero class changes).</summary>
        /// <param name="newPool">The trinkets valid for the new hero class.</param>
        public void SetPool(IReadOnlyList<Trinket> newPool)
        {
            pool = newPool;
            index = -1;
            Apply();
        }

        /// <summary>Assigns a specific trinket id, or clears when unknown.</summary>
        /// <param name="id">The trinket id.</param>
        public void Select(string id)
        {
            index = -1;
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].Id == id)
                {
                    index = i;
                    break;
                }
            }
            Apply();
        }

        private void Prev()
        {
            index = index <= -1 ? pool.Count - 1 : index - 1;
            Apply();
        }

        private void Next()
        {
            index = index >= pool.Count - 1 ? -1 : index + 1;
            Apply();
        }

        private void Clear()
        {
            index = -1;
            Apply();
        }

        private void Apply()
        {
            if (index < 0 || index >= pool.Count)
            {
                TrinketId = string.Empty;
                Label = "-";
                Details = "No trinket";
                return;
            }

            var trinket = pool[index];
            TrinketId = trinket.Id;
            Label = trinket.Id;
            Details = BuildDetails(trinket);
        }

        private static string BuildDetails(Trinket trinket)
        {
            var sb = new StringBuilder();
            sb.Append(trinket.Id);
            if (trinket.HeroClassRequirements.Count > 0)
                sb.AppendLine().Append("For: " + string.Join(", ", trinket.HeroClassRequirements));
            if (trinket.BuffIds.Count == 0)
                return sb.ToString();

            sb.AppendLine();
            foreach (var buffId in trinket.BuffIds)
            {
                var buff = Data.BuffCatalog.Get(buffId);
                if (buff == null)
                    continue;
                string tone = buff.IsPositive() ? "+" : "-";
                sb.AppendLine(tone + BuffDetails.FormatDescription(buff));
            }
            return sb.ToString();
        }
    }
}