namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A spine animation reference actually played (or expected) for an event.</summary>
    /// <param name="Skeleton">The logical skeleton id (animation set folder).</param>
    /// <param name="Animation">The animation name inside the skeleton.</param>
    /// <param name="Track">The animation track.</param>
    /// <param name="Loop">Whether the animation loops.</param>
    public sealed record AnimationRef(string Skeleton, string Animation, int Track = 0, bool Loop = false);
}
