using System;
using System.IO;

namespace Sektor.DarkestDungeon.Clients.EventLog
{
    /// <summary>
    /// Resolves the shared capture folders used by the Unity recorder and the WPF replay client.
    /// Captures live in <c>&lt;repository&gt;\captures\run_&lt;timestamp&gt;</c> when the repository
    /// root can be found; otherwise the caller-provided fallback directory is used.
    /// </summary>
    public static class GameEventCaptureLocator
    {
        /// <summary>The folder under the repository root that holds recorded runs.</summary>
        public const string CapturesFolderName = "captures";

        /// <summary>The prefix of one recorded run directory.</summary>
        public const string RunFolderPrefix = "run_";

        /// <summary>The repository marker file used to locate the project root.</summary>
        public const string RepositoryMarkerFileName = "Darkest-Dungeon-Unity.slnx";

        /// <summary>Finds the repository root by walking up from <paramref name="startDirectory"/>.</summary>
        /// <param name="startDirectory">The directory to start from (for example <c>AppContext.BaseDirectory</c>).</param>
        /// <returns>The directory containing <see cref="RepositoryMarkerFileName"/>, or <c>null</c>.</returns>
        public static string? FindRepositoryRoot(string startDirectory)
        {
            if (string.IsNullOrWhiteSpace(startDirectory))
                return null;

            DirectoryInfo? current;
            try
            {
                current = new DirectoryInfo(Path.GetFullPath(startDirectory));
            }
            catch (Exception exception) when (exception is ArgumentException || exception is NotSupportedException || exception is PathTooLongException)
            {
                return null;
            }

            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, RepositoryMarkerFileName)))
                    return current.FullName;

                current = current.Parent;
            }

            return null;
        }

        /// <summary>Returns the shared <c>captures</c> folder, or the fallback when no repository is found.</summary>
        /// <param name="startDirectory">The directory to start the repository search from.</param>
        /// <param name="fallbackDirectory">The directory used when no repository marker is found.</param>
        /// <returns>The shared capture root or <paramref name="fallbackDirectory"/>.</returns>
        public static string ResolveCaptureRoot(string startDirectory, string fallbackDirectory)
        {
            string? repositoryRoot = FindRepositoryRoot(startDirectory);
            return repositoryRoot != null
                ? Path.Combine(repositoryRoot, CapturesFolderName)
                : fallbackDirectory;
        }

        /// <summary>Returns the newest <c>run_*</c> directory that contains a manifest.</summary>
        /// <param name="captureRoot">The shared capture root to scan.</param>
        /// <returns>The newest run directory, or <c>null</c> when none exists.</returns>
        public static string? FindLatestRunDirectory(string captureRoot)
        {
            if (string.IsNullOrWhiteSpace(captureRoot) || !Directory.Exists(captureRoot))
                return null;

            string[] directories;
            try
            {
                directories = Directory.GetDirectories(captureRoot, RunFolderPrefix + "*");
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return null;
            }

            string? latest = null;
            foreach (string directory in directories)
            {
                string name = Path.GetFileName(directory);
                if (string.IsNullOrEmpty(name) || name.Length <= RunFolderPrefix.Length)
                    continue;
                if (!File.Exists(Path.Combine(directory, GameEventFileNames.Manifest)))
                    continue;

                if (latest == null || string.CompareOrdinal(name, Path.GetFileName(latest)) > 0)
                    latest = directory;
            }

            return latest;
        }

        /// <summary>Returns the newest recorded run for replay, or the fallback directory.</summary>
        /// <param name="startDirectory">The directory to start the repository search from.</param>
        /// <param name="fallbackDirectory">The replay directory used when no recorded run exists.</param>
        /// <returns>The newest run directory or <paramref name="fallbackDirectory"/>.</returns>
        public static string ResolveReplayDirectory(string startDirectory, string fallbackDirectory)
        {
            string captureRoot = ResolveCaptureRoot(startDirectory, fallbackDirectory);
            return FindLatestRunDirectory(captureRoot) ?? fallbackDirectory;
        }
    }
}
