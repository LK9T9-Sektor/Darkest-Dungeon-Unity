using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Utils;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Content.Database;

namespace Sektor.DarkestDungeon.Wpf.Data
{
    /// <summary>Loads the buff definitions from the bundled content file into core <see cref="Buff"/> objects.</summary>
    public static class BuffCatalog
    {
        private static readonly Dictionary<string, Buff> BuffsById = new Dictionary<string, Buff>();

        static BuffCatalog()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Content", "Buffs", "JsonBuffs.json");
            if (!File.Exists(path))
                return;

            var data = JsonConvert.DeserializeObject<JsonBuffData>(File.ReadAllText(path));
            if (data?.buffs == null)
                return;

            foreach (var content in BuffContentMapper.Parse(data.buffs))
            {
                AttributeType attribute = CharacterHelper.StringToAttributeType(content.AttributeTypeName);
                if (attribute == AttributeType.Undefined)
                    continue;

                var buff = new Buff(
                    CharacterHelper.StringToBuffType(content.StatType),
                    CharacterHelper.StringToBuffRule(content.RuleTypeName),
                    attribute,
                    content.Amount)
                {
                    Id = content.Id,
                    IsFalseRule = content.IsFalseRule,
                    SingleParam = content.RuleFloat,
                    StringParam = content.RuleString,
                };
                BuffsById[content.Id] = buff;
            }
        }

        /// <summary>Gets a buff by id, or null when the id is unknown.</summary>
        /// <param name="id">The buff id.</param>
        /// <returns>The buff or null.</returns>
        public static Buff? Get(string id)
        {
            Buff buff;
            return BuffsById.TryGetValue(id, out buff) ? buff : null;
        }
    }
}