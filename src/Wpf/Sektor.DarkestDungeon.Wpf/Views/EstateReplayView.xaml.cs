using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Estate capture replay screen: resources, buildings, roster, quests and timeline.</summary>
    public partial class EstateReplayView : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="EstateReplayView"/> class.</summary>
        public EstateReplayView()
        {
            InitializeComponent();
        }

        private void OnTimelineSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollTimelineToCurrent(sender as ListBox);
        }

        private void OnTimelineLoaded(object sender, RoutedEventArgs e)
        {
            ScrollTimelineToCurrent(sender as ListBox);
        }

        private static void ScrollTimelineToCurrent(ListBox? listBox)
        {
            if (listBox?.SelectedItem == null)
                return;

            listBox.Dispatcher.BeginInvoke(
                new Action(() => listBox.ScrollIntoView(listBox.SelectedItem)),
                DispatcherPriority.Background);
        }
    }
}
