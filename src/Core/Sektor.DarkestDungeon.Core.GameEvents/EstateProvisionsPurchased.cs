namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>A provision was bought or sold back in the estate.</summary>
    /// <param name="ItemId">The item id.</param>
    /// <param name="Amount">The item amount.</param>
    /// <param name="GoldCost">The gold paid (positive) or received (negative).</param>
    /// <param name="IsSellback">Whether this was a sell-back action.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record EstateProvisionsPurchased(
        string ItemId,
        int Amount,
        int GoldCost,
        bool IsSellback,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Estate, Input, Rng);
}
