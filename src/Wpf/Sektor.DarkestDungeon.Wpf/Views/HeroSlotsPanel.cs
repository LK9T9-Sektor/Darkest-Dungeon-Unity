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

        private static void OnSlotsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HeroSlotsPanel)d).ApplySlots(e.NewValue as IEnumerable);
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
                    ListSortDirection.Descending));
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
