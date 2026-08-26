namespace Sektor.DarkestDungeon.Core.Combat.Interfaces
{
    /// <summary>Abstraction of a character attribute.</summary>
    public interface IAttribute
    {
        /// <summary>Gets the modified value of the attribute.</summary>
        float ModifiedValue { get; }
    }
}
