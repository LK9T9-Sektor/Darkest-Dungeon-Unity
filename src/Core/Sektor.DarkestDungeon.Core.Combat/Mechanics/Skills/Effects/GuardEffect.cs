using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Applies a guard effect so the performer guards the target.</summary>
    public class GuardEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.GuardAlly; } }

        private bool SwapTargets { get; set; }

        /// <inheritdoc/>
        public override void Apply(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
            {
                if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                {
                    if (SwapTargets)
                        ApplyInstant(target, performer, effect, battleContext);
                    else
                        ApplyInstant(performer, target, effect, battleContext);
                }
                else
                    target.EventQueue.Add(new EffectEvent(performer, target, effect, this, battleContext));
            }
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this, battleContext));
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            var targetGuardStatus = (IGuardStatusEffect)target.Character.GetStatusEffect(StatusType.Guard);
            var targetGuardedStatus = (IGuardedStatusEffect)target.Character.GetStatusEffect(StatusType.Guarded);

            var performerGuardStatus = (IGuardStatusEffect)performer.Character.GetStatusEffect(StatusType.Guard);
            var performerGuardedStatus = (IGuardedStatusEffect)performer.Character.GetStatusEffect(StatusType.Guarded);

            if (performerGuardedStatus.IsApplied)
                performerGuardedStatus.ResetStatus();

            if (performerGuardStatus.IsApplied)
            {
                if (performerGuardStatus.Targets.Contains(target))
                {
                    targetGuardedStatus.GuardDuration = effect.IntegerParams[EffectIntParams.Duration] ?? 1;
                }
                else
                {
                    if (targetGuardStatus.IsApplied)
                        targetGuardStatus.ResetStatus();

                    if (targetGuardedStatus.IsApplied)
                        targetGuardedStatus.ResetStatus();

                    targetGuardedStatus.GuardDuration = effect.IntegerParams[EffectIntParams.Duration] ?? 1;
                    targetGuardedStatus.Guard = performer;
                    performerGuardStatus.Targets.Add(target);
                    battleContext.Events.UpdateOverlay(target);
                }
            }
            else
            {
                if (targetGuardStatus.IsApplied)
                    targetGuardStatus.ResetStatus();

                if (targetGuardedStatus.IsApplied)
                    targetGuardedStatus.ResetStatus();

                targetGuardedStatus.GuardDuration = effect.IntegerParams[EffectIntParams.Duration] ?? 1;
                targetGuardedStatus.Guard = performer;
                performerGuardStatus.Targets.Add(target);
                battleContext.Events.UpdateOverlay(performer);
                battleContext.Events.UpdateOverlay(target);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (SwapTargets)
            {
                if (ApplyInstant(target, performer, effect, battleContext))
                {
                    battleContext.Events.ShowPopup(performer, PopupType.Guard);
                    battleContext.Events.UpdateOverlay(performer);
                    return true;
                }
            }
            else
            {
                if (ApplyInstant(performer, target, effect, battleContext))
                {
                    battleContext.Events.ShowPopup(target, PopupType.Guard);
                    battleContext.Events.UpdateOverlay(target);
                    return true;
                }
            }
            return false;
        }
    }
}