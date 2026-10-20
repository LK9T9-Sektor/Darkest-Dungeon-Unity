namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>One status effect applied to a combatant in a snapshot.</summary>
    /// <param name="Id">The status id (for example <c>Stun</c> or <c>Bleeding</c>).</param>
    /// <param name="IsApplied">Whether the effect is currently active.</param>
    /// <param name="Duration">Remaining duration in turns, or <c>0</c> when the status has no timer.</param>
    /// <param name="Magnitude">Damage per tick, guard duration, or another status-specific value.</param>
    /// <param name="SourceId">The source unit id when known (for example the guard target).</param>
    public sealed record CombatantStatusState(
        string Id,
        bool IsApplied,
        int Duration = 0,
        int Magnitude = 0,
        string? SourceId = null);
}
