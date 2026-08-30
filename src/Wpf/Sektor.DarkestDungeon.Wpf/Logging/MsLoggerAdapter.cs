using Microsoft.Extensions.Logging;

using Sektor.DarkestDungeon.Core.Common;

namespace Sektor.DarkestDungeon.Wpf.Logging
{
    /// <summary>
    /// Bridges the core's structural <see cref="ILogger"/> to a Microsoft.Extensions.Logging
    /// <see cref="Microsoft.Extensions.Logging.ILogger"/>. Keeps the core free of NuGet dependencies
    /// while the presentation layer routes through the standard abstraction.
    /// </summary>
    public sealed class MsLoggerAdapter : Core.Common.ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger inner;

        /// <summary>Initializes a new instance of the <see cref="MsLoggerAdapter"/> class.</summary>
        /// <param name="inner">The Microsoft logger to forward to.</param>
        public MsLoggerAdapter(Microsoft.Extensions.Logging.ILogger inner)
        {
            this.inner = inner;
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            inner.LogInformation(message);
        }

        /// <inheritdoc/>
        public void Warn(string message)
        {
            inner.LogWarning(message);
        }
    }
}