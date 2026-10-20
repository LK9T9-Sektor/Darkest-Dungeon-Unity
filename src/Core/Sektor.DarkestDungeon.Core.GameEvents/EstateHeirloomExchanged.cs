namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>Heirlooms were exchanged in the estate.</summary>
    /// <param name="FromId">The spent heirloom id.</param>
    /// <param name="FromAmount">The spent amount.</param>
    /// <param name="ToId">The received heirloom id.</param>
    /// <param name="ToAmount">The received amount.</param>
    /// <param name="Input">The cause, or <c>null</c>.</param>
    /// <param name="Rng">Not applicable (<c>null</c>).</param>
    public sealed record EstateHeirloomExchanged(
        string FromId,
        int FromAmount,
        string ToId,
        int ToAmount,
        GameInput? Input = null,
        GameRng? Rng = null)
        : GameEvent(default, default, 0, SubsystemNames.Estate, Input, Rng);
}
