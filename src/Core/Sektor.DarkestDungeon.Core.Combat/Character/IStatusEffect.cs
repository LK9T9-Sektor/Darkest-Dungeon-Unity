namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of a status effect.</summary>
    public interface IStatusEffect
    {
        /// <summary>Gets a value indicating whether the effect is currently applied.</summary>
        bool IsApplied { get; }
    }
}
