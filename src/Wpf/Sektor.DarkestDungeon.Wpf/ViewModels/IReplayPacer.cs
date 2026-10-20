using System;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Schedules the next replay step on the UI thread.</summary>
    public interface IReplayPacer
    {
        /// <summary>Schedules a single callback after the delay, replacing any pending callback.</summary>
        /// <param name="delay">The delay before the callback runs.</param>
        /// <param name="callback">The callback to run.</param>
        void Schedule(TimeSpan delay, Action callback);

        /// <summary>Cancels the pending callback, if any.</summary>
        void Cancel();
    }
}
