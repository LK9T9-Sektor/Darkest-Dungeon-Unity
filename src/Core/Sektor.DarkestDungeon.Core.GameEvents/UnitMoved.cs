namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A unit changed formation rank.</summary>
    /// <param name="UnitId">The moved unit id.</param>
    /// <param name="FromRank">The rank before the move.</param>
    /// <param name="ToRank">The rank after the move.</param>
    /// <param name="Cause">What caused the move (for example <c>skill</c> or <c>surprise</c>).</param>
    /// <param name="SwapWithId">The unit swapped with, when known.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">The roll behind the move, or <c>null</c>.</param>
    public sealed record UnitMoved(
        string UnitId,
        int FromRank,
        int ToRank,
        string Cause,
        string? SwapWithId = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
