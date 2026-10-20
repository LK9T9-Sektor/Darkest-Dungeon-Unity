using System;
using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>
    /// Self-contained state snapshot taken at key moments (battle start, turn end, room entry -
    /// see <see cref="StateSnapshotContexts"/>); the next event must apply to the closest
    /// preceding snapshot without the full history (spec §4.1).
    /// </summary>
    /// <param name="Context">Why the snapshot was taken (see <see cref="StateSnapshotContexts"/>).</param>
    /// <param name="Globals">Global session parameters.</param>
    /// <param name="Combatants">All battle participants (friend and foe).</param>
    /// <param name="Subsystem">The subsystem owning the context (combat/map).</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable to snapshots (<c>null</c>).</param>
    /// <param name="Battle">Battle-loop state captured with the snapshot, or <c>null</c> outside a battle.</param>
    public sealed record StateSnapshot(
        string Context,
        SceneGlobals Globals,
        IReadOnlyList<CombatantState> Combatants,
        string Subsystem,
        GameInput? Input = null,
        GameRng? Rng = null,
        BattleState? Battle = null)
        : GameEvent(default, default, 0, Subsystem, Input, Rng);
}
