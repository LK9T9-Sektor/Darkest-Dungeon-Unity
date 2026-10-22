namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Multiplayer lobby screen; pumps the transport while it is visible.</summary>
    public partial class MultiplayerLobbyView : PumpableScreenBase
    {
        /// <summary>Initializes a new instance of the <see cref="MultiplayerLobbyView"/> class.</summary>
        public MultiplayerLobbyView()
        {
            InitializeComponent();
        }

        private void OnSlotEditRequested(object? sender, HeroSlotEditRequestedEventArgs e)
        {
            HeroEditor.Open(e.Slot);
        }
    }
}