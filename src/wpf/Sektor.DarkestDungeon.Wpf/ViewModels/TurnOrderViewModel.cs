using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Placeholder for the turn order panel shown in the top area.</summary>
    public partial class TurnOrderViewModel : ObservableObject
    {
        /// <summary>Gets the turn order slots (placeholder values).</summary>
        public ObservableCollection<int> Slots { get; } = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
    }
}
