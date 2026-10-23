using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Replays a recorded Unity capture: formation view, event log and replay controls.</summary>
    public partial class CaptureReplayView : UserControl
    {
        private CaptureReplayViewModel? viewModel;

        /// <summary>Initializes a new instance of the <see cref="CaptureReplayView"/> class.</summary>
        public CaptureReplayView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        private BattleArrowView? AttackArrow
        {
            get { return BattleFieldControl.Overlay as BattleArrowView; }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Attach(e.NewValue as CaptureReplayViewModel);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Attach(null);
        }

        private void Attach(CaptureReplayViewModel? next)
        {
            if (ReferenceEquals(viewModel, next))
                return;

            if (viewModel != null)
            {
                viewModel.AttackStarted -= OnAttackStarted;
                viewModel.AttackFinished -= OnAttackFinished;
            }

            viewModel = next;

            if (viewModel != null)
            {
                viewModel.AttackStarted += OnAttackStarted;
                viewModel.AttackFinished += OnAttackFinished;
            }
            else
            {
                AttackArrow?.Hide();
            }
        }

        private void OnAttackStarted(object? sender, CaptureAttackEventArgs e)
        {
            BattleFieldControl.ActivityLabel = e.SkillName ?? string.Empty;
            AttackArrow?.Show(e.Attacker, e.Target);
        }

        private void OnAttackFinished(object? sender, EventArgs e)
        {
            BattleFieldControl.ActivityLabel = string.Empty;
            AttackArrow?.Hide();
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
