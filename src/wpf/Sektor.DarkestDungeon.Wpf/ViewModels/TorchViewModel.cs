using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Torch meter state for the top-center torch panel.</summary>
    public partial class TorchViewModel : ObservableObject
    {
        /// <summary>Gets or sets the torch value (0-100).</summary>
        [ObservableProperty]
        private double _torchValue = 75;
    }
}
