using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>
    /// Evaluates whether a buff rule is currently active for a unit. Each <see cref="BuffRule"/>
    /// has a dedicated predicate registered in a dictionary (polymorphic dispatch instead of a
    /// switch). The result tells the caller whether to apply or revert the buff.
    /// </summary>
    public static class BuffRuleEvaluator
    {
        /// <summary>Dispatches each rule type to its activation predicate.</summary>
        private static readonly Dictionary<BuffRule, Func<BuffInfo, BattleRulesContext, bool>> Rules =
            new Dictionary<BuffRule, Func<BuffInfo, BattleRulesContext, bool>>
            {
                { BuffRule.Always, (entry, ctx) => !entry.Buff.IsFalseRule },
                { BuffRule.Afflicted, (entry, ctx) => Match(ctx.Unit.Character.IsAfflicted, entry.Buff.IsFalseRule) },
                { BuffRule.Virtued, (entry, ctx) => Match(ctx.Unit.Character.IsVirtued, entry.Buff.IsFalseRule) },
                { BuffRule.DeathsDoor, (entry, ctx) => Match(ctx.Unit.Character.AtDeathsDoor, entry.Buff.IsFalseRule) },
                { BuffRule.FirstRound, (entry, ctx) =>
                    ctx.BattleGround != null
                    && ctx.BattleGround.BattleStatus == BattleStatus.Fighting
                    && Match(ctx.BattleGround.Round.RoundNumber == 0, entry.Buff.IsFalseRule) },
                { BuffRule.HpAbove, (entry, ctx) => Match(ctx.Unit.Character.HealthRatio > entry.Buff.SingleParam, entry.Buff.IsFalseRule) },
                { BuffRule.HpBelow, (entry, ctx) => Match(ctx.Unit.Character.HealthRatio < entry.Buff.SingleParam, entry.Buff.IsFalseRule) },
                { BuffRule.StressAbove, (entry, ctx) => Match(ctx.Unit.Character.Stress.CurrentValue > entry.Buff.SingleParam, entry.Buff.IsFalseRule) },
                { BuffRule.StressBelow, (entry, ctx) => Match(ctx.Unit.Character.Stress.CurrentValue < entry.Buff.SingleParam, entry.Buff.IsFalseRule) },
                { BuffRule.InRank, (entry, ctx) => Match(ctx.Unit.Rank == entry.Buff.SingleParam + 1, entry.Buff.IsFalseRule) },
                { BuffRule.Size, (entry, ctx) => ctx.Target != null && Match(ctx.Target.Size == entry.Buff.SingleParam, entry.Buff.IsFalseRule) },
                { BuffRule.LightAbove, (entry, ctx) => Match(ctx.TorchAmount > entry.Buff.SingleParam, entry.Buff.IsFalseRule) },
                { BuffRule.LightBelow, (entry, ctx) => Match(ctx.TorchAmount < entry.Buff.SingleParam, entry.Buff.IsFalseRule) },
                { BuffRule.Skill, (entry, ctx) => ctx.Skill != null && Match(ctx.Skill.Id == entry.Buff.StringParam, entry.Buff.IsFalseRule) },
                { BuffRule.Melee, (entry, ctx) => ctx.Skill != null && Match(ctx.Skill.Type == "melee", entry.Buff.IsFalseRule) },
                { BuffRule.Ranged, (entry, ctx) => ctx.Skill != null && Match(ctx.Skill.Type == "ranged", entry.Buff.IsFalseRule) },
                { BuffRule.Status, EvaluateStatus },
                { BuffRule.EnemyType, EvaluateEnemyType },
                { BuffRule.InMode, (entry, ctx) => ctx.Unit.Character.InMode
                    && Match(ctx.Unit.Character.CurrentMode != null
                        && ctx.Unit.Character.CurrentMode.Id == entry.Buff.StringParam, entry.Buff.IsFalseRule) },
                { BuffRule.Riposting, (entry, ctx) => Match(ctx.IsRiposting, entry.Buff.IsFalseRule) },
                { BuffRule.InCamp, (entry, ctx) => Match(ctx.IsDoingCamping, entry.Buff.IsFalseRule) },
                { BuffRule.InCorridor, (entry, ctx) => Match(ctx.IsInHall, entry.Buff.IsFalseRule) },
                { BuffRule.InDungeon, (entry, ctx) => ctx.Dungeon != null && Match(ctx.Dungeon == entry.Buff.StringParam, entry.Buff.IsFalseRule) },
                { BuffRule.InActivity, (entry, ctx) => false },
                { BuffRule.WalkBack, (entry, ctx) => false },
            };

        /// <summary>Determines whether the buff rule is active for the unit.</summary>
        /// <param name="buffEntry">The applied buff instance.</param>
        /// <param name="context">The combat rules context.</param>
        /// <returns>True when the buff should be applied, false when reverted.</returns>
        public static bool IsActive(BuffInfo buffEntry, BattleRulesContext context)
        {
            Func<BuffInfo, BattleRulesContext, bool> evaluator;
            if (!Rules.TryGetValue(buffEntry.Buff.RuleType, out evaluator))
                return false;
            return evaluator(buffEntry, context);
        }

        private static bool EvaluateStatus(BuffInfo buffEntry, BattleRulesContext context)
        {
            if (context.Target == null)
                return false;

            StatusType targetStatus = StringToStatusType(buffEntry.Buff.StringParam);
            if (targetStatus == StatusType.None)
                return false;
            return Match(context.Target.Character.GetStatusEffect(targetStatus).IsApplied, buffEntry.Buff.IsFalseRule);
        }

        private static bool EvaluateEnemyType(BuffInfo buffEntry, BattleRulesContext context)
        {
            if (context.Target == null || !context.Target.Character.IsMonster || context.Target.Character.MonsterTypes == null)
                return false;

            MonsterType monsterType = StringToMonsterType(buffEntry.Buff.StringParam);
            return Match(context.Target.Character.MonsterTypes.Contains(monsterType), buffEntry.Buff.IsFalseRule);
        }

        private static bool Match(bool condition, bool isFalseRule)
        {
            return isFalseRule ? !condition : condition;
        }

        private static StatusType StringToStatusType(string value)
        {
            switch (value)
            {
                case "stun": return StatusType.Stun;
                case "bleeding": return StatusType.Bleeding;
                case "poison": return StatusType.Poison;
                case "marked": return StatusType.Marked;
                case "riposte": return StatusType.Riposte;
                case "guard": return StatusType.Guard;
                case "guarded": return StatusType.Guarded;
                case "deaths_door": return StatusType.DeathsDoor;
                case "death_recovery": return StatusType.DeathRecovery;
                default: return StatusType.None;
            }
        }

        private static MonsterType StringToMonsterType(string value)
        {
            switch (value)
            {
                case "unholy": return MonsterType.Unholy;
                case "man": return MonsterType.Man;
                case "eldritch": return MonsterType.Eldritch;
                case "beast": return MonsterType.Beast;
                case "corpse": return MonsterType.Corpse;
                default: return MonsterType.None;
            }
        }
    }
}