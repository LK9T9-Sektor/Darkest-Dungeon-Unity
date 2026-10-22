using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Shared hero slot selection list used by both lobbies. Right-clicking a slot requests
    /// the hero loadout editor from the hosting party panel.</summary>
    public partial class HeroSlotsPanel : UserControl
    {
        /// <summary>Identifies the <see cref="Slots"/> dependency property.</summary>
        public static readonly DependencyProperty SlotsProperty = DependencyProperty.Register(
            nameof(Slots),
            typeof(IEnumerable),
            typeof(HeroSlotsPanel),
            new PropertyMetadata(null, OnSlotsChanged));

        /// <summary>Identifies the <see cref="IsReversedSlots"/> dependency property.</summary>
        public static readonly DependencyProperty IsReversedSlotsProperty = DependencyProperty.Register(
            nameof(IsReversedSlots),
            typeof(bool),
            typeof(HeroSlotsPanel),
            new PropertyMetadata(true, OnIsReversedSlotsChanged));

        /// <summary>Identifies the <see cref="IsMirrored"/> dependency property.</summary>
        public static readonly DependencyProperty IsMirroredProperty = DependencyProperty.Register(
            nameof(IsMirrored),
            typeof(bool),
            typeof(HeroSlotsPanel),
            new PropertyMetadata(false));

        /// <summary>Raised when the user right-clicks a hero slot.</summary>
        public event EventHandler<HeroSlotEditRequestedEventArgs>? SlotEditRequested;

        /// <summary>Initializes a new instance of the <see cref="HeroSlotsPanel"/> class.</summary>
        public HeroSlotsPanel()
        {
            InitializeComponent();
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

        private static void OnSlotsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HeroSlotsPanel)d).ApplySlots(e.NewValue as IEnumerable);
        }

        private static void OnIsReversedSlotsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (HeroSlotsPanel)d;
            panel.ApplySlots(panel.Slots);
        }

        private void ApplySlots(IEnumerable? slots)
        {
            if (SlotsItems == null)
                return;

            if (slots == null)
            {
                SlotsItems.ItemsSource = null;
                return;
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(slots);
            if (view.CanSort)
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(
                    nameof(HeroSlotViewModel.Position),
                    IsReversedSlots ? ListSortDirection.Descending : ListSortDirection.Ascending));
            }

            SlotsItems.ItemsSource = view;
        }

        private void OnSlotRightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is HeroSlotViewModel slot)
            {
                SlotEditRequested?.Invoke(this, new HeroSlotEditRequestedEventArgs(slot));
                e.Handled = true;
            }
        }
    }
}
