namespace Sektor.DarkestDungeon.Core.Common
{
    /// <summary>Contract for objects selectable by weighted random chance.</summary>
    public interface IProportionValue
    {
        /// <summary>Gets or sets the selection chance.</summary>
        int Chance { get; set; }
    }
}