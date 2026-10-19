using System;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>
    /// A skill attack was executed - the key event for visual reproduction: who attacked whom,
    /// with which skill and which spine animations the sides play (spec §4.2).
    /// </summary>
    /// <param name="AttackerId">The attacking unit id.</param>
    /// <param name="TargetId">The target unit id.</param>
    /// <param name="SkillName">The skill id.</param>
    /// <param name="AttackerAnimation">The animation the attacker plays, or <c>null</c> when unknown.</param>
    /// <param name="TargetAnimation">The reaction animation of the target, or <c>null</c> when unknown.</param>
    /// <param name="Input">The cause (usually a player click), or <c>null</c>.</param>
    /// <param name="Rng">The roll behind the outcome, or <c>null</c> when not instrumented.</param>
    public sealed record AttackExecuted(
        string AttackerId,
        string TargetId,
        string SkillName,
        AnimationRef? AttackerAnimation = null,
        AnimationRef? TargetAnimation = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Combat, Input, Rng);
}
