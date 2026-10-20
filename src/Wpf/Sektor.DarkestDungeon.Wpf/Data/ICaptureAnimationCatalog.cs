using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Resolves the spine content directories used by the capture replay screen.</summary>
    public interface ICaptureAnimationCatalog
    {
        /// <summary>
        /// Gets the content directory of a recorded animation, falling back to the skeleton's idle
        /// set when the exact animation set is not available.
        /// </summary>
        /// <param name="animation">The animation recorded in the capture, or <c>null</c>.</param>
        /// <returns>The absolute directory, or <c>null</c> when the skeleton has no linked content.</returns>
        string? GetAnimationDirectory(AnimationRef? animation);

        /// <summary>Gets the idle content directory of a skeleton.</summary>
        /// <param name="skeleton">The logical skeleton id (for example <c>crusader</c>).</param>
        /// <returns>The absolute directory, or <c>null</c> when the skeleton has no linked content.</returns>
        string? GetIdleDirectory(string skeleton);
    }
}
