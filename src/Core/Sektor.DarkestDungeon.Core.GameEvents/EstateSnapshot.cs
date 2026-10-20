using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>
    /// Self-contained estate state taken on entry and after every estate action: resources, roster,
    /// buildings, quests and the active town event.
    /// </summary>
    /// <param name="Week">The campaign week.</param>
    /// <param name="Resources">Gold and heirloom amounts.</param>
    /// <param name="Roster">Heroes currently in the estate roster.</param>
    /// <param name="Buildings">Estate buildings and their upgrade progress.</param>
    /// <param name="Quests">Quests available on the quest board.</param>
    /// <param name="TownEventId">The active town event id, or <c>null</c>.</param>
    /// <param name="TownEventTone">The active town event tone, or <c>null</c>.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable to estate snapshots (<c>null</c>).</param>
    public sealed record EstateSnapshot(
        int Week,
        IReadOnlyList<EstateResourceAmount> Resources,
        IReadOnlyList<EstateHeroState> Roster,
        IReadOnlyList<EstateBuildingState> Buildings,
        IReadOnlyList<EstateQuestState> Quests,
        string? TownEventId = null,
        string? TownEventTone = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Estate, Input, Rng);
}
