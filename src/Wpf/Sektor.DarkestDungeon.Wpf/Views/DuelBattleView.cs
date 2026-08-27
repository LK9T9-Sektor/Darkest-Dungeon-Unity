using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Duel battle screen; pumps the rival link while it is visible.</summary>
    public partial class DuelBattleView : UserControl
    {
        private readonly DispatcherTimer pumpTimer;

        /// <summary>Initializes a new instance of the <see cref="DuelBattleView"/> class.</summary>
        public DuelBattleView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            pumpTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            pumpTimer.Tick += OnPumpTick;
        }

        private void OnPumpTick(object? sender, EventArgs e)
        {
            (DataContext as IPumpable)?.Pump();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            pumpTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            pumpTimer.Stop();
        }
    }
}
