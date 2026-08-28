using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Wpf.Ui
{
    /// <summary>Builds the human-readable tooltip description of a combat skill.</summary>
    public static class SkillDetails
    {
        /// <summary>Builds a multi-line description (damage/heal, accuracy, crit, ranks).</summary>
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

            lines.Add("Launch ranks: " + FormatRanks(skill.LaunchRanks));
            lines.Add("Target ranks: " + FormatRanks(skill.TargetRanks));
            return string.Join("\n", lines);
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
    }
}