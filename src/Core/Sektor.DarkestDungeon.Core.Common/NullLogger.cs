namespace Sektor.DarkestDungeon.Core.Common
{
    /// <summary>
    /// No-op logger for tests and contexts where logging is not wired. Keeps core call sites
    /// logging-safe without requiring a concrete sink.
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        /// <summary>Shared no-op instance.</summary>
        public static readonly NullLogger Instance = new NullLogger();

        /// <inheritdoc/>
        public void Log(string message)
        {
        }

        /// <inheritdoc/>
        public void Warn(string message)
        {
        }
    }
}