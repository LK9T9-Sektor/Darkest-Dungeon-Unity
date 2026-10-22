using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Single-player duel lobby: mixed party selection and start.</summary>
    public partial class DuelLobbyView : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="DuelLobbyView"/> class.</summary>
        public DuelLobbyView()
        {
            InitializeComponent();
        }

        private void OnSlotEditRequested(object? sender, HeroSlotEditRequestedEventArgs e)
        {
            HeroEditor.Open(e.Slot);
        }
    }
}
