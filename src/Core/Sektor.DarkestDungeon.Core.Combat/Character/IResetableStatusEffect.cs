namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of a status effect that can be reset.</summary>
    public interface IResetableStatusEffect : IStatusEffect
    {
        /// <summary>Resets the status to its default state.</summary>
        void ResetStatus();
    }
}