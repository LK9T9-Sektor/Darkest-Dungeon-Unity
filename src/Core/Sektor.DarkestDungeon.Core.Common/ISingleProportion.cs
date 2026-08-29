namespace Sektor.DarkestDungeon.Core.Common
{
    /// <summary>Contract for objects selectable by a fractional weighted chance.</summary>
    public interface ISingleProportion
    {
        /// <summary>Gets or sets the selection chance.</summary>
        float Chance { get; set; }
    }
}