namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Abstraction of the stun status effect.</summary>
    public interface IStunStatusEffect : IStatusEffect
    {
        /// <summary>Gets or sets a value indicating whether the stun is applied.</summary>
        bool StunApplied { get; set; }
    }
}