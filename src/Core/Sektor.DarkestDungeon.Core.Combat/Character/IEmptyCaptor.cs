namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of the empty captor component of a monster.</summary>
    public interface IEmptyCaptor
    {
        /// <summary>Gets the base class of the performer that can fill the captor.</summary>
        string PerformerBaseClass { get; }

        /// <summary>Gets the monster class id of the filled captor.</summary>
        string FullMonsterClass { get; }
    }
}