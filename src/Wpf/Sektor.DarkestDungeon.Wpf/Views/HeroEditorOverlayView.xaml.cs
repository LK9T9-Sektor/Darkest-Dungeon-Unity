using System;
using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Modal hero loadout editor: guild header art, stats, active-skill selection, quirks and
    /// trinkets. The host supplies a <see cref="ViewModels.HeroSlotViewModel"/> as the data context.</summary>
    public partial class HeroEditorOverlayView : UserControl
    {
        /// <summary>Raised when the user requests closing the editor.</summary>
        public event EventHandler? CloseRequested;

        /// <summary>Initializes a new instance of the <see cref="HeroEditorOverlayView"/> class.</summary>
        public HeroEditorOverlayView()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
