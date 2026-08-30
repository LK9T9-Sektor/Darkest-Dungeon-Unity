using Microsoft.Extensions.Logging;

namespace Sektor.DarkestDungeon.Wpf.Logging
{
    /// <summary>Creates <see cref="FileLogger"/> instances sharing one destination file.</summary>
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly string filePath;

        /// <summary>Initializes a new instance of the <see cref="FileLoggerProvider"/> class.</summary>
        /// <param name="filePath">The destination text file path.</param>
        public FileLoggerProvider(string filePath)
        {
            this.filePath = filePath;
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, filePath);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}