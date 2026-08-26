using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

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

        private int StackParameter { get; set; }

        /// <summary>Initializes a new instance of the <see cref="EffectEvent"/> class.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="effect">The parent effect.</param>
        /// <param name="subEffect">The sub-effect to apply.</param>
        public EffectEvent(ICombatUnit performer, ICombatUnit target, Effect effect, SubEffect subEffect)
        {
            Performer = performer;
            Target = target;
            Effect = effect;
            SubEffect = subEffect;
        }

        /// <summary>Fuses with another event.</summary>
        /// <param name="nextEvent">The next event to fuse with.</param>
        public void Fuse(IEffectEvent nextEvent)
        {
            StackParameter += nextEvent.SubEffect.Fuse(nextEvent.Performer, nextEvent.Target, nextEvent.Effect);
        }

        /// <summary>Fuses with itself to get the base stack parameter.</summary>
        public void FuseSelf()
        {
            StackParameter = SubEffect.Fuse(Performer, Target, Effect);
        }

        /// <summary>Executes the event.</summary>
        public void Execute()
        {
            if (StackParameter > 0)
                SubEffect.ApplyFused(Performer, Target, Effect, StackParameter);
            else
                SubEffect.ApplyQueued(Performer, Target, Effect);
        }
    }
}
