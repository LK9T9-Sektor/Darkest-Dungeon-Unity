using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable single trinket slot. The data context is a LobbyTrinketViewModel.</summary>
    public partial class TrinketSlotView : UserControl
    {
        /// <summary>Identifies the <see cref="ShowReroll"/> dependency property.</summary>
        public static readonly DependencyProperty ShowRerollProperty = DependencyProperty.Register(
            nameof(ShowReroll),
            typeof(bool),
            typeof(TrinketSlotView),
            new PropertyMetadata(true, OnShowRerollChanged));

        /// <summary>Initializes a new instance of the <see cref="TrinketSlotView"/> class.</summary>
        public TrinketSlotView()
        {
            InitializeComponent();
            UpdateRerollColumn();
        }

        /// <summary>Gets or sets a value indicating whether the reroll button is shown.</summary>
        public bool ShowReroll
        {
            get => (bool)GetValue(ShowRerollProperty);
            set => SetValue(ShowRerollProperty, value);
        }

        private static void OnShowRerollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TrinketSlotView)d).UpdateRerollColumn();
        }

        private void UpdateRerollColumn()
        {
            if (RerollColumn != null)
                RerollColumn.Width = new GridLength(ShowReroll ? 24 : 0);
        }
    }
}
