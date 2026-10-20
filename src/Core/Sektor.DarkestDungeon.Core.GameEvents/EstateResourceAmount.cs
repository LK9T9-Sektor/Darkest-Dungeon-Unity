namespace Sektor.DarkestDungeon.Core.GameEvents
{
    /// <summary>One estate currency or heirloom amount (for example <c>gold</c> or <c>crest</c>).</summary>
    /// <param name="Id">The resource id.</param>
    /// <param name="Amount">The resource amount.</param>
    public sealed record EstateResourceAmount(string Id, int Amount);
}
