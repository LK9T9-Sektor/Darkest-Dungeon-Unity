namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of a quirk/disease applied to a hero.</summary>
    public interface IQuirk
    {
        /// <summary>Gets the quirk identifier.</summary>
        string Id { get; }
    }
}