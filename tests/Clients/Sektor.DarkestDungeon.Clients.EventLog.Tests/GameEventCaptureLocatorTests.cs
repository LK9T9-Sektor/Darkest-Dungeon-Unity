using System;
using System.IO;

using NUnit.Framework;

using Sektor.DarkestDungeon.Clients.EventLog;

namespace Sektor.DarkestDungeon.Clients.EventLog.Tests
{
    /// <summary>Verifies repository discovery and capture-run selection shared by Unity and WPF.</summary>
    [TestFixture]
    public class GameEventCaptureLocatorTests
    {
        private string root;

        [SetUp]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), "eventlog-capture-locator-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null && Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }

        /// <summary>The repository marker is found from a nested WPF/Unity directory.</summary>
        [Test]
        public void FindRepositoryRoot_FromNestedDirectory_ReturnsRepositoryRoot()
        {
            File.WriteAllText(Path.Combine(root, GameEventCaptureLocator.RepositoryMarkerFileName), string.Empty);
            string nested = Path.Combine(root, "src", "Wpf", "bin", "Debug", "net8.0-windows");
            Directory.CreateDirectory(nested);

            string repositoryRoot = GameEventCaptureLocator.FindRepositoryRoot(nested);

            Assert.That(repositoryRoot, Is.EqualTo(root));
        }

        /// <summary>A directory outside the repository has no marker and no root.</summary>
        [Test]
        public void FindRepositoryRoot_WithoutMarker_ReturnsNull()
        {
            string nested = Path.Combine(root, "outside");
            Directory.CreateDirectory(nested);

            Assert.That(GameEventCaptureLocator.FindRepositoryRoot(nested), Is.Null);
        }

        /// <summary>The shared root is the project captures folder when the repository is found.</summary>
        [Test]
        public void ResolveCaptureRoot_WithRepository_ReturnsProjectCaptures()
        {
            File.WriteAllText(Path.Combine(root, GameEventCaptureLocator.RepositoryMarkerFileName), string.Empty);
            string fallback = Path.Combine(root, "fallback");

            string captureRoot = GameEventCaptureLocator.ResolveCaptureRoot(root, fallback);

            Assert.That(captureRoot, Is.EqualTo(Path.Combine(root, GameEventCaptureLocator.CapturesFolderName)));
        }

        /// <summary>Without a repository root the caller fallback is kept.</summary>
        [Test]
        public void ResolveCaptureRoot_WithoutRepository_ReturnsFallback()
        {
            string fallback = Path.Combine(root, "fallback");

            string captureRoot = GameEventCaptureLocator.ResolveCaptureRoot(root, fallback);

            Assert.That(captureRoot, Is.EqualTo(fallback));
        }

        /// <summary>The newest run with a manifest wins over older or incomplete run folders.</summary>
        [Test]
        public void FindLatestRunDirectory_ReturnsNewestRunWithManifest()
        {
            string captureRoot = Path.Combine(root, GameEventCaptureLocator.CapturesFolderName);
            string older = CreateRun(captureRoot, "run_2026-10-20_10-00-00", withManifest: true);
            string newest = CreateRun(captureRoot, "run_2026-10-21_10-00-00", withManifest: true);
            CreateRun(captureRoot, "run_2026-10-22_10-00-00", withManifest: false);
            Directory.CreateDirectory(Path.Combine(captureRoot, "other"));

            string latest = GameEventCaptureLocator.FindLatestRunDirectory(captureRoot);

            Assert.That(older, Is.Not.EqualTo(newest));
            Assert.That(latest, Is.EqualTo(newest));
        }

        /// <summary>An empty or missing captures folder yields no run.</summary>
        [Test]
        public void FindLatestRunDirectory_WithoutRuns_ReturnsNull()
        {
            string captureRoot = Path.Combine(root, GameEventCaptureLocator.CapturesFolderName);
            Directory.CreateDirectory(captureRoot);

            Assert.That(GameEventCaptureLocator.FindLatestRunDirectory(captureRoot), Is.Null);
            Assert.That(GameEventCaptureLocator.FindLatestRunDirectory(Path.Combine(root, "missing")), Is.Null);
        }

        /// <summary>The WPF replay picks the newest run when one exists.</summary>
        [Test]
        public void ResolveReplayDirectory_WithRuns_ReturnsNewestRun()
        {
            File.WriteAllText(Path.Combine(root, GameEventCaptureLocator.RepositoryMarkerFileName), string.Empty);
            string captureRoot = Path.Combine(root, GameEventCaptureLocator.CapturesFolderName);
            CreateRun(captureRoot, "run_2026-10-20_10-00-00", withManifest: true);
            string newest = CreateRun(captureRoot, "run_2026-10-21_10-00-00", withManifest: true);
            string reference = Path.Combine(root, "reference");

            string replayDirectory = GameEventCaptureLocator.ResolveReplayDirectory(root, reference);

            Assert.That(replayDirectory, Is.EqualTo(newest));
        }

        /// <summary>Without a recorded run WPF falls back to the bundled reference capture.</summary>
        [Test]
        public void ResolveReplayDirectory_WithoutRuns_ReturnsReference()
        {
            string reference = Path.Combine(root, "reference");

            string replayDirectory = GameEventCaptureLocator.ResolveReplayDirectory(root, reference);

            Assert.That(replayDirectory, Is.EqualTo(reference));
        }

        private static string CreateRun(string captureRoot, string name, bool withManifest)
        {
            string directory = Path.Combine(captureRoot, name);
            Directory.CreateDirectory(directory);
            if (withManifest)
                File.WriteAllText(Path.Combine(directory, GameEventFileNames.Manifest), "{}");
            return directory;
        }
    }
}
