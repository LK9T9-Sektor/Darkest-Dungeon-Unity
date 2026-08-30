using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Clears the guard/guarded statuses of a target.</summary>
    public class ClearGuardEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.ClearGuard; } }

        private bool ClearGuarding { get; set; }
        private bool ClearGuarded { get; set; }

        /// <summary>Sets which guard sides are cleared by this effect.</summary>
        /// <param name="clearGuarding">Whether the guarding (performer) status is cleared.</param>
        /// <param name="clearGuarded">Whether the guarded (target) status is cleared.</param>
        public void SetFlags(bool clearGuarding, bool clearGuarded)
        {
            ClearGuarding = clearGuarding;
            ClearGuarded = clearGuarded;
        }

        /// <inheritdoc/>
        public override void Apply(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
            {
                if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                    ApplyInstant(performer, target, effect, battleContext);
                else
                    target.EventQueue.Add(new EffectEvent(performer, target, effect, this, battleContext));
            }
            else
                ApplyInstant(performer, target, effect, battleContext);
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            if (ClearGuarding)
                ((IResetableStatusEffect)target.Character.GetStatusEffect(StatusType.Guard)).ResetStatus();
            if (ClearGuarded)
                ((IResetableStatusEffect)target.Character.GetStatusEffect(StatusType.Guarded)).ResetStatus();

            battleContext.Events.UpdateOverlay(target);

            return false;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            return ApplyInstant(performer, target, effect, battleContext);
        }
    }
}