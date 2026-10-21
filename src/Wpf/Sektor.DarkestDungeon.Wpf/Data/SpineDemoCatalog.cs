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
        private const string SkeletonExtension = ".skel";
        private const string SpriteMarker = ".sprite.";
        private const string CacheKeySeparator = "|";

        private readonly string rootDirectory;
        private readonly Dictionary<string, string?> animationSets = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

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

        /// <inheritdoc/>
        public string? FindAnimationSet(string character, string animationId)
        {
            if (string.IsNullOrWhiteSpace(character) || string.IsNullOrWhiteSpace(animationId))
                return null;

            string key = character + CacheKeySeparator + animationId;
            string? cached;
            if (animationSets.TryGetValue(key, out cached))
                return cached;

            string? found = null;
            string characterDirectory = Path.Combine(rootDirectory, character);
            if (Directory.Exists(characterDirectory))
            {
                // The skeleton file name carries the animation id (crusader.sprite.zealous_accusation.skel);
                // the enclosing folder may differ (ZealosAccusation - a legacy content typo).
                string suffix = SpriteMarker + animationId.Trim() + SkeletonExtension;
                foreach (string setDirectory in Directory.GetDirectories(characterDirectory))
                {
                    foreach (string file in Directory.EnumerateFiles(setDirectory, "*" + SkeletonExtension))
                    {
                        if (Path.GetFileName(file).EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        {
                            found = Path.GetFileName(setDirectory);
                            break;
                        }
                    }

                    if (found != null)
                        break;
                }
            }

            animationSets[key] = found;
            return found;
        }
    }
}
