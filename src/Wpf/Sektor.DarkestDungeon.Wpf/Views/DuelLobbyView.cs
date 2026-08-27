using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Sektor.DarkestDungeon.Wpf.Networking;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Multiplayer lobby screen; pumps the transport while it is visible.</summary>
    public partial class DuelLobbyView : UserControl
    {
        private readonly DispatcherTimer pumpTimer;

        /// <summary>Initializes a new instance of the <see cref="DuelLobbyView"/> class.</summary>
        public DuelLobbyView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            pumpTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            pumpTimer.Tick += OnPumpTick;
        }

        private void OnPumpTick(object? sender, EventArgs e)
        {
            (DataContext as IPumpable)?.Pump();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
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
