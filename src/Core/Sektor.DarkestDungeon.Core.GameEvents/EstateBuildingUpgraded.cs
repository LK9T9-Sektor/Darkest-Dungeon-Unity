using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>One estate building upgrade was purchased.</summary>
    /// <param name="BuildingId">The building id.</param>
    /// <param name="TreeId">The upgrade tree id.</param>
    /// <param name="UpgradeId">The purchased upgrade id.</param>
    /// <param name="UpgradeRatio">The building's purchased fraction after the purchase (0..1).</param>
    /// <param name="Cost">The purchase cost, or <c>null</c> when not captured.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record EstateBuildingUpgraded(
        string BuildingId,
        string TreeId,
        string UpgradeId,
        float UpgradeRatio,
        IReadOnlyList<EstateResourceAmount>? Cost = null,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Estate, Input, Rng);
}
