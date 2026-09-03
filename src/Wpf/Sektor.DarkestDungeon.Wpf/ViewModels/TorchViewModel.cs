using System;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Torch meter state for the top-center torch panel plus the current round number.</summary>
    public partial class TorchViewModel : ObservableObject
    {
        /// <summary>Gets or sets the torch value (0-100).</summary>
        [ObservableProperty]
        private double _torchValue = 75;

        /// <summary>Gets or sets the current round number shown inside the torch circle.</summary>
        [ObservableProperty]
        private int _round = 1;

        /// <summary>Gets or sets the flame brush derived from the torch value (darker at 0, brighter and yellower toward 100).</summary>
        [ObservableProperty]
        private Brush? _torchLevelBrush = CreateLevelBrush(75);

        /// <summary>Gets the command that raises the torch by 25 (clamped at 100).</summary>
        public IRelayCommand IncreaseCommand { get; }

        /// <summary>Gets the command that lowers the torch by 25 (clamped at 0).</summary>
        public IRelayCommand DecreaseCommand { get; }

        /// <summary>Initializes a new instance of the <see cref="TorchViewModel"/> class.</summary>
        public TorchViewModel()
        {
            IncreaseCommand = new RelayCommand(() => TorchValue = Math.Min(100, TorchValue + 25));
            DecreaseCommand = new RelayCommand(() => TorchValue = Math.Max(0, TorchValue - 25));
        }

        partial void OnTorchValueChanged(double value)
        {
            TorchLevelBrush = CreateLevelBrush(value);
        }

        /// <summary>Builds a frozen brush for a torch level: 0 is dark gray, every 25 points moves
        /// toward a bright yellow (0-25-50-75-100 bands).</summary>
        private static Brush CreateLevelBrush(double value)
        {
            byte g = (byte)Math.Min(255, 60 + value * 1.9);
            byte r = (byte)Math.Min(255, 60 + value * 2.0);
            byte b = (byte)Math.Min(255, 50 + value * 0.35);
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }
    }
}