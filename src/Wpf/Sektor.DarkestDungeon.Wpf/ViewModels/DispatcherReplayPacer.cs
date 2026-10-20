using System;
using System.Windows.Threading;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Runs the replay pacing callback on the WPF dispatcher.</summary>
    internal sealed class DispatcherReplayPacer : IReplayPacer
    {
        private readonly DispatcherTimer timer;
        private Action? callback;

        /// <summary>Initializes a new instance of the <see cref="DispatcherReplayPacer"/> class.</summary>
        public DispatcherReplayPacer()
        {
            timer = new DispatcherTimer();
            timer.Tick += OnTick;
        }

        /// <inheritdoc/>
        public void Schedule(TimeSpan delay, Action callback)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            timer.Stop();
            timer.Interval = delay > TimeSpan.Zero ? delay : TimeSpan.FromMilliseconds(1);
            timer.Start();
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            timer.Stop();
            callback = null;
        }

        private void OnTick(object? sender, EventArgs e)
        {
            Action? pending = callback;
            Cancel();
            pending?.Invoke();
        }
    }
}
