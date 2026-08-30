using System;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Logging;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    [TestFixture]
    public class FileLoggerTests
    {
        [Test]
        public void Log_WritesTimestampedRowToFile()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ddwpf_logger_tests");
            string filePath = Path.Combine(directory, "test_" + Guid.NewGuid().ToString("N") + ".log");
            try
            {
                var provider = new FileLoggerProvider(filePath);
                Microsoft.Extensions.Logging.ILogger logger = provider.CreateLogger("Duel");
                logger.LogInformation("hello world");

                Assert.That(File.Exists(filePath), Is.True);
                string content = File.ReadAllText(filePath);
                Assert.That(content, Does.Contain("hello world"));
                Assert.That(content, Does.Contain("INFORMATION"));
                Assert.That(content, Does.Contain("Duel"));
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                if (Directory.Exists(directory) && !Directory.EnumerateFiles(directory).Any())
                    Directory.Delete(directory);
            }
        }

        [Test]
        public void Warn_WritesWarningRowToFile()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ddwpf_logger_tests");
            string filePath = Path.Combine(directory, "test_" + Guid.NewGuid().ToString("N") + ".log");
            try
            {
                var provider = new FileLoggerProvider(filePath);
                Microsoft.Extensions.Logging.ILogger logger = provider.CreateLogger("Duel");

                var adapter = new MsLoggerAdapter(logger);
                adapter.Warn("careful");

                Assert.That(File.Exists(filePath), Is.True);
                string content = File.ReadAllText(filePath);
                Assert.That(content, Does.Contain("careful"));
                Assert.That(content, Does.Contain("WARNING"));
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                if (Directory.Exists(directory) && !Directory.EnumerateFiles(directory).Any())
                    Directory.Delete(directory);
            }
        }

        [Test]
        public void BelowMinLevel_IsNotWritten()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ddwpf_logger_tests");
            string filePath = Path.Combine(directory, "test_" + Guid.NewGuid().ToString("N") + ".log");
            try
            {
                var logger = new FileLogger("Duel", filePath, LogLevel.Warning);
                logger.LogInformation("should be skipped");

                Assert.That(File.Exists(filePath), Is.False);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                if (Directory.Exists(directory) && !Directory.EnumerateFiles(directory).Any())
                    Directory.Delete(directory);
            }
        }
    }
}