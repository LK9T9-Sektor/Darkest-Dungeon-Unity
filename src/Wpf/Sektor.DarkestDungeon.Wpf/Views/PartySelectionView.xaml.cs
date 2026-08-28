using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable party selection panel: player label on top, four editable hero slots below.</summary>
    public partial class PartySelectionView : UserControl
    {
        /// <summary>Identifies the <see cref="PlayerLabel"/> dependency property.</summary>
        public static readonly DependencyProperty PlayerLabelProperty = DependencyProperty.Register(
            nameof(PlayerLabel),
            typeof(string),
            typeof(PartySelectionView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="Slots"/> dependency property.</summary>
        public static readonly DependencyProperty SlotsProperty = DependencyProperty.Register(
            nameof(Slots),
            typeof(IEnumerable),
            typeof(PartySelectionView),
            new PropertyMetadata(null));

        /// <summary>Gets or sets the player label ("#1 Player", "#2 AI").</summary>
        public string PlayerLabel
        {
            get { return (string)GetValue(PlayerLabelProperty); }
            set { SetValue(PlayerLabelProperty, value); }
        }

        /// <summary>Gets or sets the hero slot collection.</summary>
        public IEnumerable Slots
        {
            get { return (IEnumerable)GetValue(SlotsProperty); }
            set { SetValue(SlotsProperty, value); }
        }

        /// <summary>Initializes a new instance of the <see cref="PartySelectionView"/> class.</summary>
        public PartySelectionView()
        {
            InitializeComponent();
        }
    }
}