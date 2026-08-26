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

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>Abstract base class for individual effect behaviors.</summary>
    public abstract class SubEffect
    {
        /// <summary>Gets the effect sub-type.</summary>
        public abstract EffectSubType Type { get; }

        /// <summary>Gets a value indicating whether this effect is fusable.</summary>
        public virtual bool Fusable { get { return false; } }

        /// <summary>Gets the target status type associated with this effect (for stat buffs).</summary>
        public virtual StatusType TargetStatus { get { return StatusType.None; } }

        /// <summary>Applies the effect to the target.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="effect">The parent effect.</param>
        public virtual void Apply(ICombatUnit performer, ICombatUnit target, Effect effect)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
            {
                if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                    ApplyInstant(performer, target, effect);
                else
                    target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
            }
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }

        /// <summary>Applies the effect when queued.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="effect">The parent effect.</param>
        /// <returns>True if the effect was applied.</returns>
        public abstract bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect);

        /// <summary>Applies the effect instantly.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="effect">The parent effect.</param>
        /// <returns>True if the effect was applied.</returns>
        public abstract bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect);

        /// <summary>Applies target conditions.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="primaryTarget">The primary target.</param>
        /// <param name="effect">The parent effect.</param>
        public virtual void ApplyTargetConditions(ICombatUnit performer, ICombatUnit target, ICombatUnit primaryTarget, Effect effect)
        {
        }

        /// <summary>Applies the effect when fused.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="effect">The parent effect.</param>
        /// <param name="fuseParameter">The fuse parameter.</param>
        /// <returns>True if the effect was applied.</returns>
        public virtual bool ApplyFused(ICombatUnit performer, ICombatUnit target, Effect effect, int fuseParameter)
        {
            return false;
        }

        /// <summary>Calculates the fuse parameter.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="effect">The parent effect.</param>
        /// <returns>The fuse parameter value.</returns>
        public virtual int Fuse(ICombatUnit performer, ICombatUnit target, Effect effect)
        {
            return 0;
        }

        /// <summary>Gets the tooltip text for this effect.</summary>
        /// <param name="effect">The parent effect.</param>
        /// <returns>The tooltip string.</returns>
        public virtual string Tooltip(Effect effect)
        {
            return "";
        }
    }
}
