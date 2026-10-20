using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Clients.EventLog
{
    /// <summary>
    /// Subscribes to an <see cref="IGameEventBus"/> and streams every event as one JSON line into
    /// a per-subsystem file inside a run directory (spec §1.3/§2): <c>&lt;subsystem&gt;_session.jsonl</c>,
    /// plus a <c>manifest.json</c> kept up to date as files appear. Writes are auto-flushed so a
    /// crash loses at most the event being written.
    /// </summary>
    public sealed class JsonEventLogger : IDisposable
    {
        /// <summary>The schema version written into manifest.json; bump when the event DTOs change shape.</summary>
        public const string CurrentSchemaVersion = "1.2.0";

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None,
        };

        private readonly List<SubsystemFile> files = new List<SubsystemFile>();
        private readonly IGameEventBus bus;
        private readonly IDisposable subscription;
        private readonly string directory;
        private readonly long? seed;
        private readonly string? notes;
        private readonly string? kind;
        private readonly DateTime startedAt = DateTime.UtcNow;
        private readonly object gate = new object();
        private bool disposed;

        /// <summary>Initializes a new instance: creates the run directory and subscribes to the bus.</summary>
        /// <param name="bus">The event bus to record.</param>
        /// <param name="directory">The run directory (created if missing); one JSONL file per subsystem.</param>
        /// <param name="seed">The run seed recorded in the manifest, when known.</param>
        /// <param name="notes">Free-form scenario notes recorded in the manifest.</param>
        /// <param name="kind">The run kind (<c>raid</c>/<c>estate</c>), when known.</param>
        /// <exception cref="ArgumentNullException">The bus is null.</exception>
        /// <exception cref="ArgumentException">The directory is empty or whitespace.</exception>
        public JsonEventLogger(
            IGameEventBus bus,
            string directory,
            long? seed = null,
            string? notes = null,
            string? kind = null)
        {
            this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("Run directory must be a non-empty path.", nameof(directory));

            this.directory = directory;
            this.seed = seed;
            this.notes = notes;
            this.kind = kind;
            Directory.CreateDirectory(directory);

            subscription = this.bus.Subscribe<GameEvent>(Append);
            WriteManifest();
        }

        /// <summary>Gets the path of the manifest file inside the run directory.</summary>
        public string ManifestPath => Path.Combine(directory, GameEventFileNames.Manifest);

        /// <summary>Gets the base names of the JSONL files written so far (in creation order).</summary>
        public IReadOnlyList<string> Files
        {
            get
            {
                lock (gate)
                {
                    var names = new List<string>(files.Count);
                    foreach (SubsystemFile file in files)
                        names.Add(file.FileName);
                    return names;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (gate)
            {
                if (disposed)
                    return;
                disposed = true;

                subscription.Dispose();
                foreach (SubsystemFile file in files)
                {
                    file.Writer.Flush();
                    file.Writer.Dispose();
                }

                WriteManifest();
            }
        }

        private void Append(GameEvent gameEvent)
        {
            if (gameEvent == null)
                return;

            string subsystem = string.IsNullOrEmpty(gameEvent.Subsystem)
                ? SubsystemNames.Unspecified
                : gameEvent.Subsystem;

            lock (gate)
            {
                if (disposed)
                    return;

                SubsystemFile file = GetOrCreateFile(subsystem);
                // Declared type must be the base: TypeNameHandling.Auto omits $type for a root
                // object serialized from its runtime type, which would make the line unreadable.
                file.Writer.WriteLine(JsonConvert.SerializeObject(gameEvent, typeof(GameEvent), SerializerSettings));
            }
        }

        private SubsystemFile GetOrCreateFile(string subsystem)
        {
            foreach (SubsystemFile existing in files)
            {
                if (string.Equals(existing.Subsystem, subsystem, StringComparison.Ordinal))
                    return existing;
            }

            string fileName = GameEventFileNames.SessionFile(subsystem);
            var created = new SubsystemFile(
                subsystem,
                fileName,
                new StreamWriter(Path.Combine(directory, fileName), append: true) { AutoFlush = true });
            files.Add(created);
            WriteManifest();
            return created;
        }

        private void WriteManifest()
        {
            var fileNames = new List<string>(files.Count);
            foreach (SubsystemFile file in files)
                fileNames.Add(file.FileName);

            var manifest = new GameEventManifest(CurrentSchemaVersion, startedAt, seed, notes, fileNames, kind);
            File.WriteAllText(
                ManifestPath,
                JsonConvert.SerializeObject(manifest, Formatting.Indented, SerializerSettings));
        }

        private sealed class SubsystemFile
        {
            public SubsystemFile(string subsystem, string fileName, StreamWriter writer)
            {
                Subsystem = subsystem;
                FileName = fileName;
                Writer = writer;
            }

            public string Subsystem { get; }

            public string FileName { get; }

            public StreamWriter Writer { get; }
        }
    }
}
