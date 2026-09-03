using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable duel unit card: portrait, name/class labels and rank banner/tab.</summary>
    public partial class DuelUnitCardView : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="DuelUnitCardView"/> class.</summary>
        public DuelUnitCardView()
        {
            InitializeComponent();
        }

        /// <summary>Swallows the info button click so it does not trigger the wrapping slot button.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnInfoButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}