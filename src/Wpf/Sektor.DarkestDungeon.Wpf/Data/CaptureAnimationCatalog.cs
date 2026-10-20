using System;
using System.IO;
using System.Text;

using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>
    /// Maps the logical skeleton/animation ids of a capture to the linked spine content tree
    /// (<c>Content\Spine\&lt;Skeleton&gt;\&lt;AnimationSet&gt;</c>). Missing sets fall back to idle;
    /// skeletons without linked content (monsters) return <c>null</c> and are shown as cards.
    /// </summary>
    public sealed class CaptureAnimationCatalog : ICaptureAnimationCatalog
    {
        private const string IdleAnimation = "Idle";
        private const char IdSeparator = '_';

        private readonly string rootDirectory;

        /// <summary>Initializes a new instance of the <see cref="CaptureAnimationCatalog"/> class.</summary>
        /// <param name="rootDirectory">The spine content root (contains one folder per skeleton).</param>
        public CaptureAnimationCatalog(string rootDirectory)
        {
            this.rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        }

        /// <inheritdoc/>
        public string? GetAnimationDirectory(AnimationRef? animation)
        {
            if (animation == null)
                return null;

            string? idleDirectory = GetIdleDirectory(animation.Skeleton);
            if (idleDirectory == null)
                return null;

            string directory = Path.Combine(
                rootDirectory,
                ToPascalCase(animation.Skeleton),
                ToPascalCase(animation.Animation));
            return Directory.Exists(directory) ? directory : idleDirectory;
        }

        /// <inheritdoc/>
        public string? GetIdleDirectory(string skeleton)
        {
            if (string.IsNullOrWhiteSpace(skeleton))
                return null;

            string directory = Path.Combine(rootDirectory, ToPascalCase(skeleton), IdleAnimation);
            return Directory.Exists(directory) ? directory : null;
        }

        private static string ToPascalCase(string id)
        {
            string[] parts = id.Split(IdSeparator);
            var builder = new StringBuilder(id.Length);
            foreach (string part in parts)
            {
                if (part.Length == 0)
                    continue;

                builder.Append(char.ToUpperInvariant(part[0]));
                builder.Append(part, 1, part.Length - 1);
            }

            return builder.ToString();
        }
    }
}
