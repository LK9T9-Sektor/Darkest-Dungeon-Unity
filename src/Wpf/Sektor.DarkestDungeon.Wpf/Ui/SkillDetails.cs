using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.ViewModels;

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
            var lines = new List<string> { BuildBaseInfo(skill) };

            var effects = BuildEffects(skill);
            if (effects.Count > 0)
            {
                lines.Add("Effects:");
                lines.AddRange(effects);
            }
            return string.Join("\n", lines);
        }

        /// <summary>Builds the base info of a skill (damage/heal, accuracy, crit, ranks) without effects.</summary>
        /// <param name="skill">The combat skill.</param>
        /// <returns>The base info text.</returns>
        public static string BuildBaseInfo(CombatSkill skill)
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

        /// <summary>Builds the structured buff/debuff rows a skill applies (for the tooltip table).</summary>
        /// <param name="skill">The combat skill.</param>
        /// <returns>The effect rows.</returns>
        public static List<SkillEffectRowViewModel> BuildEffectRows(CombatSkill skill)
        {
            var rows = new List<SkillEffectRowViewModel>();
            foreach (var effect in skill.Effects)
            {
                string note = TargetNote(effect.TargetType);
                foreach (var subEffect in effect.SubEffects)
                    rows.AddRange(BuildSubEffectRows(subEffect, effect, note));
            }
            return rows;
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

        private static List<SkillEffectRowViewModel> BuildSubEffectRows(SubEffect subEffect, Effect effect, string note)
        {
            var rows = new List<SkillEffectRowViewModel>();
            switch (subEffect)
            {
                case BleedEffect bleed:
                    rows.Add(new SkillEffectRowViewModel(note + "Bleed", DotText(bleed.DotAmount, effect), "Debuff"));
                    break;
                case PoisonEffect poison:
                    rows.Add(new SkillEffectRowViewModel(note + "Blight", DotText(poison.DotAmount, effect), "Debuff"));
                    break;
                case StunEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Stun", "Cannot act this round", "Debuff"));
                    break;
                case UnstunEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Remove stun", string.Empty, "Buff"));
                    break;
                case TagEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Mark", DurationText(effect), "Debuff"));
                    break;
                case UntagEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Remove mark", string.Empty, "Buff"));
                    break;
                case ImmobilizeEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Immobilize", "Cannot move ranks", "Debuff"));
                    break;
                case UnimmobilizeEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Remove immobilize", string.Empty, "Buff"));
                    break;
                case StressEffect stress:
                    rows.Add(new SkillEffectRowViewModel(note + "Stress", "+" + stress.Amount, "Debuff"));
                    break;
                case StressHealEffect stressHeal:
                    rows.Add(new SkillEffectRowViewModel(note + "Stress Heal", stressHeal.Amount.ToString(), "Heal"));
                    break;
                case CureEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Cure", "Bleed & blight", "Buff"));
                    break;
                case PullEffect pull:
                    rows.Add(new SkillEffectRowViewModel(note + "Pull", pull.Param + " rank(s)", "Debuff"));
                    break;
                case PushEffect push:
                    rows.Add(new SkillEffectRowViewModel(note + "Push", push.Param + " rank(s)", "Debuff"));
                    break;
                case ShuffleTargetEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Shuffle ranks", string.Empty, "Debuff"));
                    break;
                case RiposteEffect riposte:
                    rows.Add(new SkillEffectRowViewModel(note + "Riposte", string.Empty, "Buff"));
                    AppendStatBuffRows(rows, riposte.StatAddBuffs, riposte.StatMultBuffs);
                    break;
                case CombatStatBuffEffect statBuff:
                    AppendStatBuffRows(rows, statBuff.StatAddBuffs, statBuff.StatMultBuffs);
                    break;
                case BuffEffect buffEffect:
                    AppendBuffRows(rows, buffEffect.Buffs);
                    foreach (var buffId in buffEffect.BuffIds)
                    {
                        var buff = Data.BuffCatalog.Get(buffId);
                        if (buff != null)
                            rows.Add(BuffRow(buff));
                    }
                    break;
                case GuardEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Guard", "Protects the ally", "Buff"));
                    break;
                case ClearGuardEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Remove guard", string.Empty, "Buff"));
                    break;
                case SetModeEffect mode:
                    rows.Add(new SkillEffectRowViewModel(note + "Transform", "(" + mode.Mode + ")", "Buff"));
                    break;
                case KillEffect:
                case KillEnemyTypeEffect:
                    rows.Add(new SkillEffectRowViewModel(note + "Kill", string.Empty, "Debuff"));
                    break;
            }
            return rows;
        }

        private static void AppendStatBuffRows(List<SkillEffectRowViewModel> rows, Dictionary<AttributeType, float> adds, Dictionary<AttributeType, float> mults)
        {
            foreach (var stat in adds)
                rows.Add(BuffRow(new Buff(BuffType.StatAdd, stat.Key, stat.Value)));
            foreach (var stat in mults)
                rows.Add(BuffRow(new Buff(BuffType.StatMultiply, stat.Key, stat.Value)));
        }

        private static void AppendBuffRows(List<SkillEffectRowViewModel> rows, List<Buff> buffs)
        {
            foreach (var buff in buffs)
                rows.Add(BuffRow(buff));
        }

        private static SkillEffectRowViewModel BuffRow(Buff buff)
        {
            string tone = buff.IsPositive() ? "Buff" : "Debuff";
            return new SkillEffectRowViewModel(
                tone == "Buff" ? "Buff" : "Debuff",
                BuffDetails.FormatDescription(buff),
                tone);
        }

        private static string DotText(int amount, Effect effect)
        {
            string text = amount > 0 ? amount + " dmg" : string.Empty;
            int? duration = effect.IntegerParams[EffectIntParams.Duration];
            if (duration.HasValue && duration.Value > 0)
                text += (text.Length > 0 ? " " : string.Empty) + "(" + duration.Value + " rounds)";
            return text;
        }

        private static string DurationText(Effect effect)
        {
            int? duration = effect.IntegerParams[EffectIntParams.Duration];
            return duration.HasValue && duration.Value > 0 ? duration.Value + " rounds" : string.Empty;
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