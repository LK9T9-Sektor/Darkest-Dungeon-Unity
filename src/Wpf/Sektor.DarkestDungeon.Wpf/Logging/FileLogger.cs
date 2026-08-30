using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Sektor.DarkestDungeon.Wpf.Logging
{
    /// <summary>
    /// Writes structured log records to a single text file (append-only). Rows follow the shape
    /// <c>[yyyy-MM-dd HH:mm:ss.fff] LEVEL category message</c>.
    /// </summary>
    public sealed class FileLogger : ILogger
    {
        private readonly object syncRoot = new object();
        private readonly string categoryName;
        private readonly string filePath;
        private readonly LogLevel minLevel;

        /// <summary>Initializes a new instance of the <see cref="FileLogger"/> class.</summary>
        /// <param name="categoryName">The category shown on each written row.</param>
        /// <param name="filePath">The destination text file path.</param>
        /// <param name="minLevel">The minimum level to record.</param>
        public FileLogger(string categoryName, string filePath, LogLevel minLevel = LogLevel.Information)
        {
            this.categoryName = categoryName;
            this.filePath = filePath;
            this.minLevel = minLevel;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= minLevel;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string message = formatter(state, exception);
            if (exception != null)
                message += Environment.NewLine + exception;

            string row = string.Format(
                CultureInfo.InvariantCulture,
                "[{0:yyyy-MM-dd HH:mm:ss.fff}] {1} {2} {3}",
                DateTime.Now,
                logLevel.ToString().ToUpperInvariant(),
                categoryName,
                message);

            lock (syncRoot)
            {
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);
                File.AppendAllText(filePath, row + Environment.NewLine);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            public void Dispose()
            {
            }
        }
    }
}