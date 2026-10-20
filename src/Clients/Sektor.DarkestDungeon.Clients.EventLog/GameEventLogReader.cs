using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Clients.EventLog
{
    /// <summary>
    /// Reads a recorded run directory (<c>manifest.json</c> plus per-subsystem JSONL files) into a
    /// <see cref="GameEventCapture"/>. The reader validates the schema version, resolves only the
    /// contract types through <see cref="GameEventTypeBinder"/>, checks that every event matches the
    /// subsystem of its file, and verifies the global sequence is exactly 1..N without gaps or
    /// duplicates. All failures are returned as <see cref="Result{T}"/> errors.
    /// </summary>
    public sealed class GameEventLogReader
    {
        private static readonly string[] SupportedSchemaVersions = { "1.0.0", "1.1.0", JsonEventLogger.CurrentSchemaVersion };

        private static readonly JsonSerializerSettings ReadSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = GameEventTypeBinder.Instance,
        };

        /// <summary>Reads and validates one recorded run directory.</summary>
        /// <param name="runDirectory">The directory containing the manifest and JSONL files.</param>
        /// <returns>The parsed capture, or a failure describing the first contract violation.</returns>
        public Result<GameEventCapture> Read(string runDirectory)
        {
            if (string.IsNullOrWhiteSpace(runDirectory))
                return Result<GameEventCapture>.Failure("Run directory must be a non-empty path.");

            try
            {
                string manifestPath = Path.Combine(runDirectory, GameEventFileNames.Manifest);
                if (!File.Exists(manifestPath))
                    return Result<GameEventCapture>.Failure($"Manifest not found: '{manifestPath}'.");

                GameEventManifest? manifest = JsonConvert.DeserializeObject<GameEventManifest>(
                    File.ReadAllText(manifestPath),
                    ReadSettings);
                if (manifest == null)
                    return Result<GameEventCapture>.Failure($"Manifest '{manifestPath}' is empty.");

                if (Array.IndexOf(SupportedSchemaVersions, manifest.SchemaVersion) < 0)
                {
                    return Result<GameEventCapture>.Failure(
                        $"Unsupported schema version '{manifest.SchemaVersion}' (supported: {string.Join(", ", SupportedSchemaVersions)}).");
                }

                if (manifest.Files == null || manifest.Files.Count == 0)
                    return Result<GameEventCapture>.Failure("Manifest lists no event files.");

                var events = new List<GameEvent>();
                foreach (string fileName in manifest.Files)
                {
                    if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
                    {
                        return Result<GameEventCapture>.Failure(
                            $"Manifest contains an invalid event file name '{fileName}'.");
                    }

                    string? subsystem = ResolveSubsystem(fileName);
                    if (subsystem == null)
                    {
                        return Result<GameEventCapture>.Failure(
                            $"Unexpected event file name '{fileName}' (expected '<subsystem>{GameEventFileNames.SessionSuffix}').");
                    }

                    Result readResult = ReadFile(runDirectory, fileName, subsystem, events);
                    if (!readResult.IsSuccess)
                        return Result<GameEventCapture>.Failure(readResult.ErrorMessage);
                }

                events.Sort((left, right) => left.Sequence.CompareTo(right.Sequence));
                for (int index = 0; index < events.Count; index++)
                {
                    long expected = index + 1;
                    if (events[index].Sequence != expected)
                    {
                        return Result<GameEventCapture>.Failure(
                            $"Sequence gap or duplicate: expected {expected}, found {events[index].Sequence}.");
                    }
                }

                return Result<GameEventCapture>.Success(new GameEventCapture(manifest, events));
            }
            catch (Exception exception) when (
                exception is IOException
                || exception is UnauthorizedAccessException
                || exception is JsonException
                || exception is NotSupportedException
                || exception is ArgumentException)
            {
                return Result<GameEventCapture>.Failure(exception.Message);
            }
        }

        private static Result ReadFile(string runDirectory, string fileName, string subsystem, List<GameEvent> events)
        {
            string path = Path.Combine(runDirectory, fileName);
            if (!File.Exists(path))
                return Result.Failure($"Event file not found: '{path}'.");

            string[] lines = File.ReadAllLines(path);
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                GameEvent? gameEvent;
                try
                {
                    gameEvent = JsonConvert.DeserializeObject<GameEvent>(line, ReadSettings);
                }
                catch (JsonException exception)
                {
                    return Result.Failure($"Cannot read '{fileName}' line {index + 1}: {Describe(exception)}");
                }

                if (gameEvent == null)
                    return Result.Failure($"Cannot read '{fileName}' line {index + 1}: the event is empty.");

                if (!string.Equals(gameEvent.Subsystem, subsystem, StringComparison.Ordinal))
                {
                    return Result.Failure(
                        $"'{fileName}' line {index + 1} has subsystem '{gameEvent.Subsystem}' (expected '{subsystem}').");
                }

                events.Add(gameEvent);
            }

            return Result.Success();
        }

        private static string Describe(Exception exception)
        {
            return exception.InnerException == null
                ? exception.Message
                : exception.Message + " " + exception.InnerException.Message;
        }

        private static string? ResolveSubsystem(string fileName)
        {
            if (!fileName.EndsWith(GameEventFileNames.SessionSuffix, StringComparison.Ordinal))
                return null;

            string subsystem = fileName.Substring(0, fileName.Length - GameEventFileNames.SessionSuffix.Length);
            return string.IsNullOrWhiteSpace(subsystem) ? null : subsystem;
        }
    }
}
