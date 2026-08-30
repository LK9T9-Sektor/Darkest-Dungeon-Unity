using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Activates riposte on a target with optional stat buffs.</summary>
    public class RiposteEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Riposte; } }

        /// <summary>Gets the additive stat buffs.</summary>
        public Dictionary<AttributeType, float> StatAddBuffs { get; }

        /// <summary>Gets the multiplicative stat buffs.</summary>
        public Dictionary<AttributeType, float> StatMultBuffs { get; }

        /// <summary>Initializes a new instance of the <see cref="RiposteEffect"/> class.</summary>
        public RiposteEffect()
        {
            StatAddBuffs = new Dictionary<AttributeType, float>();
            StatMultBuffs = new Dictionary<AttributeType, float>();
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            var riposteStatus = (IRiposteStatusEffect)target.Character.GetStatusEffect(StatusType.Riposte);
            int duration = effect.IntegerParams[EffectIntParams.Duration] ?? BattleConstants.DefaultStatusDuration;

            if (duration == -1)
            {
                riposteStatus.DurationType = DurationType.Combat;
                duration = BattleConstants.DefaultStatusDuration;
            }

            riposteStatus.RiposteDuration = duration;

            foreach (var statInfo in StatAddBuffs)
                if (!Approximately(statInfo.Value, 0))
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, BuffRule.Riposting, statInfo.Key, statInfo.Value),
                        BuffDurationType.Round, BuffSourceType.Adventure, duration));
            foreach (var statInfo in StatMultBuffs)
                if (!Approximately(statInfo.Value, 0))
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, BuffRule.Riposting, statInfo.Key, statInfo.Value),
                        BuffDurationType.Round, BuffSourceType.Adventure, duration));

            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (ApplyInstant(performer, target, effect, battleContext))
            {
                battleContext.Events.ShowPopup(target, PopupType.Riposte);
                battleContext.Events.UpdateOverlay(target);
                return true;
            }
            return false;
        }

        private static bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 0.000001f;
        }
    }
}
