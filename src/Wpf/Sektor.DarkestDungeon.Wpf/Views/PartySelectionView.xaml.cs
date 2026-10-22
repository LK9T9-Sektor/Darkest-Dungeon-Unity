using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable party selection panel: player label and four compact hero slots.</summary>
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

        /// <summary>Identifies the <see cref="IsReversedSlots"/> dependency property.</summary>
        public static readonly DependencyProperty IsReversedSlotsProperty = DependencyProperty.Register(
            nameof(IsReversedSlots),
            typeof(bool),
            typeof(PartySelectionView),
            new PropertyMetadata(true));

        /// <summary>Identifies the <see cref="IsMirrored"/> dependency property.</summary>
        public static readonly DependencyProperty IsMirroredProperty = DependencyProperty.Register(
            nameof(IsMirrored),
            typeof(bool),
            typeof(PartySelectionView),
            new PropertyMetadata(false));

        /// <summary>Raised when a hero slot requests the loadout editor.</summary>
        public event EventHandler<HeroSlotEditRequestedEventArgs>? SlotEditRequested;

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

        /// <summary>Gets or sets a value indicating whether slots are shown in descending position order.</summary>
        public bool IsReversedSlots
        {
            get { return (bool)GetValue(IsReversedSlotsProperty); }
            set { SetValue(IsReversedSlotsProperty, value); }
        }

        /// <summary>Gets or sets a value indicating whether the hero portrait art is mirrored horizontally.</summary>
        public bool IsMirrored
        {
            get { return (bool)GetValue(IsMirroredProperty); }
            set { SetValue(IsMirroredProperty, value); }
        }

        /// <summary>Initializes a new instance of the <see cref="PartySelectionView"/> class.</summary>
        public PartySelectionView()
        {
            InitializeComponent();
        }

        private void OnSlotEditRequested(object? sender, HeroSlotEditRequestedEventArgs e)
        {
            SlotEditRequested?.Invoke(this, e);
        }
    }
}
