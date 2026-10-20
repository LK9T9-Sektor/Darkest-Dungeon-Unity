using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>
    /// Exact outcome of one skill for every affected target; <see cref="AttackExecuted"/> stays the
    /// visual event, while this event is the mechanical source of truth (spec §4.2).
    /// </summary>
    /// <param name="AttackerId">The attacking unit id.</param>
    /// <param name="SkillName">The skill id.</param>
    /// <param name="Targets">One result per target, including misses and zero-effect targets.</param>
    /// <param name="Effects">Applied effect names at skill level.</param>
    /// <param name="Input">The cause (usually a player click), or <c>null</c>.</param>
    /// <param name="Rng">The roll behind the outcome, or <c>null</c> when not instrumented.</param>
    public sealed record SkillResolved(
        string AttackerId,
        string SkillName,
        IReadOnlyList<SkillTargetResult> Targets,
        IReadOnlyList<string>? Effects = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
