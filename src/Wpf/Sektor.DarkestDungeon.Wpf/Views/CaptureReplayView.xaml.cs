using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Replays a recorded Unity capture: formation view, event log and replay controls.</summary>
    public partial class CaptureReplayView : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="CaptureReplayView"/> class.</summary>
        public CaptureReplayView()
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
