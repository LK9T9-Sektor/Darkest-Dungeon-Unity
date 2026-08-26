namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of a character attribute.</summary>
    public interface IAttribute
    {
        /// <summary>Gets the modified value of the attribute.</summary>
        float ModifiedValue { get; }
    }
}
