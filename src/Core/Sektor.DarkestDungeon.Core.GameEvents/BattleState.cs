using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Battle-loop state captured alongside a snapshot.</summary>
    /// <param name="Round">Current round number (0 before the first round).</param>
    /// <param name="Turn">Current turn kind (for example <c>Hero</c>), or <c>null</c> when unknown.</param>
    /// <param name="ActiveUnitId">The unit whose turn is resolving, or <c>null</c>.</param>
    /// <param name="OrderedUnitIds">Remaining turn order for the current round.</param>
    /// <param name="Surprise">The surprise status, or <c>null</c>.</param>
    /// <param name="Status">The battle status, or <c>null</c>.</param>
    /// <param name="Retreat">Whether the party is retreating.</param>
    public sealed record BattleState(
        int Round = 0,
        string? Turn = null,
        string? ActiveUnitId = null,
        IReadOnlyList<string>? OrderedUnitIds = null,
        string? Surprise = null,
        string? Status = null,
        bool Retreat = false);
}
