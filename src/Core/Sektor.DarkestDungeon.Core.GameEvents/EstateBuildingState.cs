namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>One estate building and its upgrade progress.</summary>
    /// <param name="Id">The building id (for example <c>abbey</c>).</param>
    /// <param name="Name">The display name id.</param>
    /// <param name="UpgradeRatio">The purchased fraction of all building upgrades (0..1).</param>
    public sealed record EstateBuildingState(
        string Id,
        string Name,
        float UpgradeRatio);
}
