using System.Collections.Generic;

using Sektor.DarkestDungeon.Core.Content.Character;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Maps raw buff content entries into domain buff definitions.</summary>
    public static class BuffContentMapper
    {
        /// <summary>Converts the raw buff entries into domain buff definitions.</summary>
        /// <param name="jsonBuffs">The raw buff entries.</param>
        /// <returns>The domain buff definitions.</returns>
        public static List<BuffContent> Parse(List<JsonBuff> jsonBuffs)
        {
            var buffs = new List<BuffContent>();
            if (jsonBuffs == null)
                return buffs;

            foreach (var json in jsonBuffs)
            {
                var buff = new BuffContent
                {
                    Id = json.id,
                    StatType = json.stat_type,
                    AttributeTypeName = json.stat_sub_type,
                    Amount = json.amount,
                    RemoveIfNotActive = json.remove_if_not_active,
                    RuleTypeName = json.rule_type,
                    IsFalseRule = json.is_false_rule,
                    RuleFloat = json.rule_data != null ? json.rule_data.@float : 0f,
                    RuleString = json.rule_data != null ? json.rule_data.@string : string.Empty,
                };
                buffs.Add(buff);
            }

            return buffs;
        }
    }
}