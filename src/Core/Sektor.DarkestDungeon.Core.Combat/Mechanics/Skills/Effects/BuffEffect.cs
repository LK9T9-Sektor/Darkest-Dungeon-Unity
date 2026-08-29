using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Applies a set of buffs/debuffs to a target.</summary>
    public class BuffEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Buff; } }

        /// <summary>Gets the buffs to apply.</summary>
        public List<Buff> Buffs { get; }

        /// <summary>Gets the content buff ids to resolve and apply (via the battle context).</summary>
        public List<string> BuffIds { get; }

        /// <summary>Initializes a new instance of the <see cref="BuffEffect"/> class.</summary>
        public BuffEffect()
        {
            Buffs = new List<Buff>();
            BuffIds = new List<string>();
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            var buffs = ResolveBuffs(battleContext);
            if (buffs.Count == 0)
                return false;

            if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
            {
                if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
                {
                    ApplyBuff(target, effect, buffs);
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
                        ApplyBuff(target, effect, buffs);
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                if (buffs[0].IsPositive())
                {
                    ApplyBuff(target, effect, buffs);
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
                        ApplyBuff(target, effect, buffs);
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null)
                return false;

            var buffs = ResolveBuffs(battleContext);
            if (buffs.Count == 0)
                return false;

            if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
            {
                if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
                {
                    ApplyBuff(target, effect, buffs);
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
                        ApplyBuff(target, effect, buffs);
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
                if (buffs[0].IsPositive())
                {
                    ApplyBuff(target, effect, buffs);
                    battleContext.Events.ShowPopup(target, PopupType.Buff);
                    battleContext.Events.UpdateOverlay(target);
                    return true;
                }
                else
                {
                    float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                        (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                    debuffChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
                    if (performer != null && !performer.Character.IsMonster)
                        debuffChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

                    debuffChance = performer == target ? 1 : Clamp01(debuffChance, 0.95f);

                    if (RandomSolver.CheckSuccess(debuffChance))
                    {
                        ApplyBuff(target, effect, buffs);
                        battleContext.Events.ShowPopup(target, PopupType.Debuff);
                        battleContext.Events.UpdateOverlay(target);
                        return true;
                    }
                    battleContext.Events.ShowPopup(target, PopupType.DebuffResist);
                    return false;
                }
            }
        }

        private List<Buff> ResolveBuffs(IBattleContext battleContext)
        {
            var resolved = new List<Buff>(Buffs);
            if (battleContext == null)
                return resolved;

            foreach (var buffId in BuffIds)
            {
                var buff = battleContext.GetBuff(buffId);
                if (buff != null && !resolved.Contains(buff))
                    resolved.Add(buff);
            }
            return resolved;
        }

        private void ApplyBuff(ICombatUnit target, Effect effect, List<Buff> buffs)
        {
            if (effect.IntegerParams[EffectIntParams.Curio].HasValue)
                foreach (var buff in buffs)
                    target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Camp, BuffSourceType.Adventure));
            else if (effect.IntegerParams[EffectIntParams.Duration].HasValue)
            {
                if (effect.IntegerParams[EffectIntParams.Duration].Value == -1)
                    foreach (var buff in buffs)
                        target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Camp, BuffSourceType.Adventure));
                else
                    foreach (var buff in buffs)
                        target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Round,
                            BuffSourceType.Adventure, effect.IntegerParams[EffectIntParams.Duration].Value));
            }
            else
            {
                foreach (var buff in buffs)
                    target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Round,
                        BuffSourceType.Adventure, 3));
            }
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