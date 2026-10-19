namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>
    /// The probability roll behind an event. All fields are nullable: a <c>null</c> roll means the
    /// producing code path is not instrumented yet (allowed by the spec checklist; the reason is
    /// recorded in <c>docs\SPEC_GAME_EVENTS_SCHEMA.md</c> per field).
    /// </summary>
    /// <param name="Seed">The seed of the roll, when known.</param>
    /// <param name="Roll">The raw roll value (0..1), when known.</param>
    /// <param name="Crit">Whether the roll crit, when known.</param>
    /// <param name="Modifier">The chance modifier used for the roll, when known.</param>
    public sealed record GameRng(long? Seed = null, float? Roll = null, bool? Crit = null, float? Modifier = null);
}
