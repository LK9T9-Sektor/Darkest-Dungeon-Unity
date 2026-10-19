using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>File system catalog over the linked spine hero content (<c>Content\Spine\&lt;Hero&gt;\&lt;Anim&gt;</c>).</summary>
    public sealed class SpineDemoCatalog : ISpineDemoCatalog
    {
        private const string IdleAnimation = "Idle";

        private readonly string rootDirectory;

        /// <summary>Initializes a new instance of the <see cref="SpineDemoCatalog"/> class.</summary>
        /// <param name="rootDirectory">The spine content root directory (contains one folder per hero).</param>
        public SpineDemoCatalog(string rootDirectory)
        {
            this.rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetCharacters()
        {
            if (!Directory.Exists(rootDirectory))
                return new List<string>();

            return Directory.GetDirectories(rootDirectory)
                .Select(path => Path.GetFileName(path)!)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetAnimations(string character)
        {
            string directory = Path.Combine(rootDirectory, character);
            if (!Directory.Exists(directory))
                return new List<string>();

            return Directory.GetDirectories(directory)
                .Select(path => Path.GetFileName(path)!)
                .OrderBy(name => string.Equals(name, IdleAnimation, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <inheritdoc/>
        public string GetContentDirectory(string character, string animation)
            => Path.Combine(rootDirectory, character, animation);
    }
}
