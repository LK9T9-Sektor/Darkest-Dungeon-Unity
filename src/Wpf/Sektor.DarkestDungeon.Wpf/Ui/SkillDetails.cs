using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects;
using Sektor.DarkestDungeon.Wpf.Data;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Builds the human-readable tooltip description of a combat skill.</summary>
    public static class SkillDetails
    {
        /// <summary>Builds a multi-line description (damage/heal, accuracy, crit, ranks, effects).</summary>
        /// <param name="skill">The combat skill.</param>
        /// <returns>The description.</returns>
        public static string Build(CombatSkill skill)
        {
            var lines = new List<string>();
            if (skill.Heal != null)
                lines.Add("Heals " + skill.Heal.MinAmount + "-" + skill.Heal.MaxAmount);
            else if (skill.Category == SkillCategory.Damage)
            {
                string damage = skill.DamageMin > 0 && skill.DamageMax > 0
                    ? (int)skill.DamageMin + "-" + (int)skill.DamageMax
                    : (skill.DamageMod > 0 ? "+" : "") + (int)(skill.DamageMod * 100) + "%";
                lines.Add("Damage " + damage);
            }

            if (skill.Accuracy > 0)
                lines.Add("ACC " + (int)(skill.Accuracy * 100) + "%");
            if (skill.CritMod != 0)
                lines.Add("Crit " + (int)(skill.CritMod * 100) + "%");
            if (skill.LimitPerTurn != null)
                lines.Add("Limit " + skill.LimitPerTurn + " per turn");

            var effects = BuildEffects(skill);
            if (effects.Count > 0)
            {
                lines.Add("Effects:");
                lines.AddRange(effects);
            }

            lines.Add("Launch ranks: " + FormatRanks(skill.LaunchRanks));
            lines.Add("Target ranks: " + FormatRanks(skill.TargetRanks));
            return string.Join("\n", lines);
        }

        /// <summary>Builds the effect lines (buffs/debuffs/statuses) a skill applies.</summary>
        /// <param name="skill">The combat skill.</param>
        /// <returns>The effect lines.</returns>
        public static List<string> BuildEffects(CombatSkill skill)
        {
            var lines = new List<string>();
            foreach (var effect in skill.Effects)
            {
                string note = TargetNote(effect.TargetType);
                foreach (var subEffect in effect.SubEffects)
                    lines.AddRange(BuildSubEffectLines(subEffect, effect, note));
            }
            return lines;
        }

        /// <summary>Formats a formation rank set as readable text.</summary>
        /// <param name="set">The formation set.</param>
        /// <returns>The ranks text.</returns>
        public static string FormatRanks(FormationSet set)
        {
            if (set.IsSelfTarget)
                return "self";
            if (set.IsSelfFormation)
                return "party" + (set.Ranks.Count > 0 ? " (" + string.Join(",", set.Ranks) + ")" : string.Empty);
            if (set.IsRandomTarget)
                return "random";
            return set.Ranks.Count == 0 ? "-" : string.Join(",", set.Ranks);
        }

        private static string TargetNote(EffectTargetType targetType)
        {
            switch (targetType)
            {
                case EffectTargetType.Performer:
                    return "(self) ";
                case EffectTargetType.PerformersOther:
                    return "(party) ";
                case EffectTargetType.TargetGroup:
                    return "(all) ";
                default:
                    return string.Empty;
            }
        }

        private static List<string> BuildSubEffectLines(SubEffect subEffect, Effect effect, string note)
        {
            var lines = new List<string>();
            switch (subEffect)
            {
                case BleedEffect bleed:
                    lines.Add(note + FormatDot("Bleed", bleed.DotAmount, effect));
                    break;
                case PoisonEffect poison:
                    lines.Add(note + FormatDot("Blight", poison.DotAmount, effect));
                    break;
                case StunEffect:
                    lines.Add(note + "Stun");
                    break;
                case UnstunEffect:
                    lines.Add(note + "Remove stun");
                    break;
                case TagEffect:
                    lines.Add(note + "Mark");
                    break;
                case UntagEffect:
                    lines.Add(note + "Remove mark");
                    break;
                case ImmobilizeEffect:
                    lines.Add(note + "Immobilize");
                    break;
                case UnimmobilizeEffect:
                    lines.Add(note + "Remove immobilize");
                    break;
                case StressEffect stress:
                    lines.Add(note + "Stress +" + stress.Amount);
                    break;
                case StressHealEffect stressHeal:
                    lines.Add(note + "Stress heal " + stressHeal.Amount);
                    break;
                case CureEffect:
                    lines.Add(note + "Cure bleed & blight");
                    break;
                case PullEffect pull:
                    lines.Add(note + "Pull " + pull.Param);
                    break;
                case PushEffect push:
                    lines.Add(note + "Push " + push.Param);
                    break;
                case ShuffleTargetEffect:
                    lines.Add(note + "Shuffle ranks");
                    break;
                case RiposteEffect riposte:
                    lines.Add(note + "Riposte");
                    AppendStatBuffLines(lines, riposte.StatAddBuffs, riposte.StatMultBuffs);
                    break;
                case CombatStatBuffEffect statBuff:
                    AppendStatBuffLines(lines, statBuff.StatAddBuffs, statBuff.StatMultBuffs);
                    break;
                case BuffEffect buffEffect:
                    AppendBuffLines(lines, buffEffect.Buffs);
                    foreach (var buffId in buffEffect.BuffIds)
                    {
                        var buff = Data.BuffCatalog.Get(buffId);
                        if (buff != null)
                            lines.Add(BuffLine(buff));
                    }
                    break;
                case GuardEffect:
                    lines.Add(note + "Guard");
                    break;
                case ClearGuardEffect:
                    lines.Add(note + "Remove guard");
                    break;
                case SetModeEffect mode:
                    lines.Add(note + "Transform (" + mode.Mode + ")");
                    break;
                case KillEffect:
                case KillEnemyTypeEffect:
                    lines.Add(note + "Kill");
                    break;
            }
            return lines;
        }

        private static void AppendStatBuffLines(List<string> lines, Dictionary<AttributeType, float> adds, Dictionary<AttributeType, float> mults)
        {
            foreach (var stat in adds)
                lines.Add(BuffLine(new Buff(BuffType.StatAdd, stat.Key, stat.Value)));
            foreach (var stat in mults)
                lines.Add(BuffLine(new Buff(BuffType.StatMultiply, stat.Key, stat.Value)));
        }

        private static void AppendBuffLines(List<string> lines, List<Buff> buffs)
        {
            foreach (var buff in buffs)
                lines.Add(BuffLine(buff));
        }

        private static string BuffLine(Buff buff)
        {
            return (buff.IsPositive() ? "Buff: " : "Debuff: ") + BuffDetails.FormatDescription(buff);
        }

        private static string FormatDot(string label, int amount, Effect effect)
        {
            string text = amount > 0 ? label + " " + amount : label;
            int? duration = effect.IntegerParams[EffectIntParams.Duration];
            if (duration.HasValue && duration.Value > 0)
                text += " (" + duration.Value + " rounds)";
            return text;
        }
    }
}