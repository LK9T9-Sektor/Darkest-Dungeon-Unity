using Sektor.DarkestDungeon.Core.Combat.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Interfaces
{
    /// <summary>Abstraction of a queued effect event.</summary>
    public interface IEffectEvent
    {
        /// <summary>Gets the sub-effect.</summary>
        SubEffect SubEffect { get; }

        /// <summary>Gets the performing unit.</summary>
        ICombatUnit Performer { get; }

        /// <summary>Gets the target unit.</summary>
        ICombatUnit Target { get; }

        /// <summary>Gets the parent effect.</summary>
        Effect Effect { get; }

        /// <summary>Fuses with another event.</summary>
        /// <param name="nextEvent">The next event to fuse with.</param>
        void Fuse(IEffectEvent nextEvent);

        /// <summary>Fuses with itself to get the base stack parameter.</summary>
        void FuseSelf();

        /// <summary>Executes the event.</summary>
        void Execute();
    }
}
