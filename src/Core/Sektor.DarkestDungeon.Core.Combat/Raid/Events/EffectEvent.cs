using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Raid.Events
{
    /// <summary>A queued effect event for deferred application.</summary>
    public class EffectEvent : IEffectEvent
    {
        /// <summary>Gets the sub-effect.</summary>
        public SubEffect SubEffect { get; }

        /// <summary>Gets the performing unit.</summary>
        public ICombatUnit Performer { get; }

        /// <summary>Gets the target unit.</summary>
        public ICombatUnit Target { get; }

        /// <summary>Gets the parent effect.</summary>
        public Effect Effect { get; }

        private IBattleContext BattleContext { get; }
        private int StackParameter { get; set; }

        /// <summary>Initializes a new instance of the <see cref="EffectEvent"/> class.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="effect">The parent effect.</param>
        /// <param name="subEffect">The sub-effect to apply.</param>
        /// <param name="battleContext">The battle context.</param>
        public EffectEvent(ICombatUnit performer, ICombatUnit target, Effect effect, SubEffect subEffect, IBattleContext battleContext)
        {
            Performer = performer;
            Target = target;
            Effect = effect;
            SubEffect = subEffect;
            BattleContext = battleContext;
        }

        /// <summary>Fuses with another event.</summary>
        /// <param name="nextEvent">The next event to fuse with.</param>
        public void Fuse(IEffectEvent nextEvent)
        {
            StackParameter += nextEvent.SubEffect.Fuse(nextEvent.Performer, nextEvent.Target, nextEvent.Effect, BattleContext);
        }

        /// <summary>Fuses with itself to get the base stack parameter.</summary>
        public void FuseSelf()
        {
            StackParameter = SubEffect.Fuse(Performer, Target, Effect, BattleContext);
        }

        /// <summary>Executes the event.</summary>
        public void Execute()
        {
            if (StackParameter > 0)
                SubEffect.ApplyFused(Performer, Target, Effect, StackParameter, BattleContext);
            else
                SubEffect.ApplyQueued(Performer, Target, Effect, BattleContext);
        }
    }
}