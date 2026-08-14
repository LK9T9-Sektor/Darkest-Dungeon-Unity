namespace Sektor.DarkestDungeon.Core.Content.Raid
{
    /// <summary>Contract for objects selectable by weighted random chance.</summary>
    public interface IProportionValue
    {
        /// <summary>Gets or sets the selection chance.</summary>
        int Chance { get; set; }
    }

    /// <summary>Contract for objects selectable by a fractional weighted chance.</summary>
    public interface ISingleProportion
    {
        /// <summary>Gets or sets the selection chance.</summary>
        float Chance { get; set; }
    }
}
