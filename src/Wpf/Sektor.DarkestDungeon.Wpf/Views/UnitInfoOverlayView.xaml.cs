using System;
using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Shared unit-info sheet: guild header art, stats, skills, quirks, equipment and
    /// trinkets. The data context supplies polymorphic capabilities, so the same view serves the lobby
    /// loadout editor and the read-only battle inspection sheet.</summary>
    public partial class UnitInfoOverlayView : UserControl
    {
        /// <summary>Raised when the user requests closing the editor.</summary>
        public event EventHandler? CloseRequested;

        /// <summary>Initializes a new instance of the <see cref="UnitInfoOverlayView"/> class.</summary>
        public UnitInfoOverlayView()
        {
            InitializeComponent();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
