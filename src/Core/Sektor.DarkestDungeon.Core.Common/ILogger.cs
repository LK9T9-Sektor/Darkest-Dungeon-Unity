namespace Sektor.DarkestDungeon.Core.Common
{
    /// <summary>
    /// Structural logger for the core. Injected through constructors (no singletons). The
    /// presentation layer provides a concrete implementation that routes to its own log.
    /// </summary>
    public interface ILogger
    {
        /// <summary>Logs an informational message.</summary>
        /// <param name="message">The message.</param>
        void Log(string message);

        /// <summary>Logs a warning message.</summary>
        /// <param name="message">The message.</param>
        void Warn(string message);
    }
}