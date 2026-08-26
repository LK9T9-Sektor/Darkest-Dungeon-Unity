using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Applies conditional combat stat buffs/debuffs based on status or monster type.</summary>
    public class CombatStatBuffEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.StatBuff; } }

        /// <summary>Gets or sets the target status required for the buff to apply.</summary>
        public StatusType TargetStatusValue { get; set; }

        /// <summary>Gets or sets the target monster type required for the buff to apply.</summary>
        public MonsterType TargetMonsterType { get; set; }

        /// <summary>Gets the additive stat buffs.</summary>
        public Dictionary<AttributeType, float> StatAddBuffs { get; }

        /// <summary>Gets the multiplicative stat buffs.</summary>
        public Dictionary<AttributeType, float> StatMultBuffs { get; }

        /// <inheritdoc/>
        public override StatusType TargetStatus { get { return TargetStatusValue; } }

        /// <summary>Initializes a new instance of the <see cref="CombatStatBuffEffect"/> class.</summary>
        public CombatStatBuffEffect()
        {
            StatAddBuffs = new Dictionary<AttributeType, float>();
            StatMultBuffs = new Dictionary<AttributeType, float>();
        }

        /// <summary>Determines whether the first buff is positive.</summary>
        /// <returns>True if the buff is positive.</returns>
        public bool IsPositive()
        {
            KeyValuePair<AttributeType, float> buff;

            if (StatAddBuffs.Count > 0)
                buff = StatAddBuffs.First();
            else if (StatMultBuffs.Count > 0)
                buff = StatMultBuffs.First();
            else
                return false;

            if (buff.Key == AttributeType.StressDmgPercent || buff.Key == AttributeType.StressDmgReceivedPercent)
            {
                if (buff.Value > 0)
                    return false;
                return true;
            }
            if (buff.Value >= 0)
                return true;
            return false;
        }

        /// <inheritdoc/>
        public override void Apply(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (TargetStatusValue == StatusType.None && TargetMonsterType == MonsterType.None)
            {
                if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
                {
                    if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                        ApplyInstant(performer, target, effect, battleContext);
                    else
                        target.EventQueue.Add(new EffectEvent(performer, target, effect, this, battleContext));
                }
                else
                    target.EventQueue.Add(new EffectEvent(performer, target, effect, this, battleContext));
            }
        }

        /// <inheritdoc/>
        public override void ApplyTargetConditions(ICombatUnit performer, ICombatUnit target, ICombatUnit primaryTarget, Effect effect, IBattleContext battleContext)
        {
            if ((TargetStatusValue == StatusType.None && TargetMonsterType == MonsterType.None) == false)
            {
                if (primaryTarget == null)
                    return;

                if (TargetMonsterType != MonsterType.None)
                {
                    if (primaryTarget.Character.IsMonster == false)
                        return;
                    if (!primaryTarget.Character.MonsterTypes.Contains(TargetMonsterType))
                        return;
                }

                if (TargetStatusValue != StatusType.None)
                {
                    if (!primaryTarget.Character.GetStatusEffect(TargetStatusValue).IsApplied)
                        return;
                }
                ApplyConditional(target);
            }
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || (StatAddBuffs.Count == 0 && StatMultBuffs.Count == 0))
                return false;

            if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
            {
                if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
                {
                    ApplyBuff(target, effect);
                    return true;
                }
                else
                {
                    float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                        (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                    debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                    if (performer != null && !performer.Character.IsMonster)
                        debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                    debuffChance = Clamp01(debuffChance, 0.95f);
                    if (performer == target)
                        debuffChance = 1;

                    if (RandomSolver.CheckSuccess(debuffChance))
                    {
                        ApplyBuff(target, effect);
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                if (IsPositive())
                {
                    ApplyBuff(target, effect);
                    return true;
                }
                else
                {
                    float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                        (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                    debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                    if (performer != null && !performer.Character.IsMonster)
                        debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                    debuffChance = Clamp01(debuffChance, 0.95f);
                    if (performer == target)
                        debuffChance = 1;

                    if (RandomSolver.CheckSuccess(debuffChance))
                    {
                        ApplyBuff(target, effect);
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || (StatAddBuffs.Count == 0 && StatMultBuffs.Count == 0))
                return false;

            if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
            {
                if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
                {
                    ApplyBuff(target, effect);
                    battleContext.Events.ShowPopup(target, PopupType.Buff);
                    battleContext.Events.UpdateOverlay(target);
                    return true;
                }
                else
                {
                    float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                        (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                    debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                    if (performer != null && !performer.Character.IsMonster)
                        debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                    debuffChance = performer == target ? 1 : Clamp01(debuffChance, 0.95f);

                    if (RandomSolver.CheckSuccess(debuffChance))
                    {
                        ApplyBuff(target, effect);
                        battleContext.Events.ShowPopup(target, PopupType.Debuff);
                        battleContext.Events.UpdateOverlay(target);
                        return true;
                    }
                    battleContext.Events.ShowPopup(target, PopupType.DebuffResist);
                    return false;
                }
            }
            else
            {
                if (IsPositive())
                {
                    ApplyBuff(target, effect);
                    battleContext.Events.ShowPopup(target, PopupType.Buff);
                    battleContext.Events.UpdateOverlay(target);
                    return true;
                }
                else
                {
                    float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                        (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                    debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                    if (performer != null && !performer.Character.IsMonster)
                        debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                    debuffChance = performer == target ? 1 : Clamp01(debuffChance, 0.95f);

                    if (RandomSolver.CheckSuccess(debuffChance))
                    {
                        ApplyBuff(target, effect);
                        battleContext.Events.ShowPopup(target, PopupType.Debuff);
                        battleContext.Events.UpdateOverlay(target);
                        return true;
                    }
                    battleContext.Events.ShowPopup(target, PopupType.DebuffResist);
                    return false;
                }
            }
        }

        private void ApplyBuff(ICombatUnit target, Effect effect)
        {
            if (effect.IntegerParams[EffectIntParams.Curio].HasValue)
            {
                foreach (var statInfo in StatAddBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, statInfo.Key, statInfo.Value),
                        BuffDurationType.Camp, BuffSourceType.Adventure));
                foreach (var statInfo in StatMultBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, statInfo.Key, statInfo.Value),
                        BuffDurationType.Camp, BuffSourceType.Adventure));
            }
            else if (effect.IntegerParams[EffectIntParams.Duration].HasValue)
            {
                if (effect.IntegerParams[EffectIntParams.Duration].Value == -1)
                {
                    foreach (var statInfo in StatAddBuffs)
                        target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, statInfo.Key, statInfo.Value),
                            BuffDurationType.Camp, BuffSourceType.Adventure));
                    foreach (var statInfo in StatMultBuffs)
                        target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, statInfo.Key, statInfo.Value),
                            BuffDurationType.Camp, BuffSourceType.Adventure));
                }
                else
                {
                    foreach (var statInfo in StatAddBuffs)
                        target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, statInfo.Key, statInfo.Value),
                            BuffDurationType.Round, BuffSourceType.Adventure, effect.IntegerParams[EffectIntParams.Duration].Value));
                    foreach (var statInfo in StatMultBuffs)
                        target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, statInfo.Key, statInfo.Value),
                            BuffDurationType.Round, BuffSourceType.Adventure, effect.IntegerParams[EffectIntParams.Duration].Value));
                }
            }
            else
            {
                foreach (var statInfo in StatAddBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, statInfo.Key, statInfo.Value),
                        BuffDurationType.Round, BuffSourceType.Adventure, 3));
                foreach (var statInfo in StatMultBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, statInfo.Key, statInfo.Value),
                        BuffDurationType.Round, BuffSourceType.Adventure, 3));
            }
        }

        private void ApplyConditional(ICombatUnit target)
        {
            foreach (var statInfo in StatAddBuffs)
                target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, BuffRule.Always, statInfo.Key, statInfo.Value),
                    BuffDurationType.Round, BuffSourceType.Condition));
            foreach (var statInfo in StatMultBuffs)
                target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, BuffRule.Always, statInfo.Key, statInfo.Value),
                    BuffDurationType.Round, BuffSourceType.Condition));
        }

        private static float Clamp01(float value, float max)
        {
            if (value < 0)
                return 0;
            if (value > max)
                return max;
            return value;
        }
    }
}