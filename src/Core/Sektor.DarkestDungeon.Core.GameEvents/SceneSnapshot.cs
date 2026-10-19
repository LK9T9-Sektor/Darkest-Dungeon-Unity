using System;
using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Full self-contained scene snapshot taken once on level load (spec §4.1).</summary>
    /// <param name="SceneName">The logical scene/level name.</param>
    /// <param name="Hierarchy">The flat node list (parent ids, no nesting).</param>
    /// <param name="Globals">Global session parameters.</param>
    /// <param name="Heroes">The hero units.</param>
    /// <param name="Party">Party member ids in formation order.</param>
    /// <param name="Input">The cause, or <c>null</c> (system event).</param>
    /// <param name="Rng">Not applicable to snapshots (<c>null</c>).</param>
    public sealed record SceneSnapshot(
        string SceneName,
        IReadOnlyList<SceneNode> Hierarchy,
        SceneGlobals Globals,
        IReadOnlyList<CombatantState> Heroes,
        IReadOnlyList<string> Party,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Map, Input, Rng);
}
