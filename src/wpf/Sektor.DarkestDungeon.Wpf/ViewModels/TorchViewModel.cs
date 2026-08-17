using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Torch meter state for the top-center torch panel.</summary>
    public partial class TorchViewModel : ObservableObject
    {
        /// <summary>Gets or sets the torch value (0-100).</summary>
        [ObservableProperty]
        private double _torchValue = 75;

        /// <summary>Gets the command that raises the torch by 25 (clamped at 100).</summary>
        public IRelayCommand IncreaseCommand { get; }

        /// <summary>Gets the command that lowers the torch by 25 (clamped at 0).</summary>
        public IRelayCommand DecreaseCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="TorchViewModel"/> class.</summary>
        public TorchViewModel()
        {
            IncreaseCommand = new RelayCommand(() => TorchValue = System.Math.Min(100, TorchValue + 25));
            DecreaseCommand = new RelayCommand(() => TorchValue = System.Math.Max(0, TorchValue - 25));
        }
    }
}
