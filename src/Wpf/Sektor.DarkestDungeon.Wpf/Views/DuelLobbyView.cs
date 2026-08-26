using System.Windows;
using System.Windows.Threading;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Window hosting the duel lobby.</summary>
    public partial class DuelLobbyView : Window
    {
        private readonly DuelLobbyViewModel viewModel;
        private readonly DispatcherTimer pumpTimer;

        /// <summary>Initializes a new instance of the <see cref="DuelLobbyView"/> class.</summary>
        /// <param name="viewModel">The lobby view model.</param>
        public DuelLobbyView(DuelLobbyViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = viewModel;

            pumpTimer = new DispatcherTimer { Interval = System.TimeSpan.FromMilliseconds(50) };
            pumpTimer.Tick += (s, e) => Pump();
            pumpTimer.Start();
        }

        private void Pump()
        {
            viewModel.Pump();
        }

        /// <inheritdoc/>
        protected override void OnClosed(System.EventArgs e)
        {
            pumpTimer.Stop();
            viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}