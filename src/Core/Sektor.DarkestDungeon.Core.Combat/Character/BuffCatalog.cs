using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character.Utils;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Content.Database;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Loads the buff definitions from the campaign JsonBuffs.json content into core <see cref="Buff"/> instances.</summary>
    public sealed class BuffCatalog
    {
        private readonly Dictionary<string, Buff> _buffsById =
            new Dictionary<string, Buff>(System.StringComparer.OrdinalIgnoreCase);

        private BuffCatalog()
        {
        }

        /// <summary>Gets the empty catalog (no buffs).</summary>
        public static BuffCatalog Empty { get { return new BuffCatalog(); } }

        /// <summary>Gets all buffs ordered by their wire order.</summary>
        public IReadOnlyCollection<Buff> All { get { return _buffsById.Values; } }

        /// <summary>Maps the deserialized JsonBuffs.json document into a buff catalog.</summary>
        /// <param name="data">The deserialized JsonBuffs.json document.</param>
        /// <returns>The buff catalog.</returns>
        public static BuffCatalog Load(JsonBuffData data)
        {
            var catalog = new BuffCatalog();
            if (data?.buffs == null)
                return catalog;

            foreach (BuffContent content in BuffContentMapper.Parse(data.buffs))
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
                    StringParam = content.RuleString
                };
                catalog._buffsById[content.Id] = buff;
            }

            return catalog;
        }

        /// <summary>Gets a buff by id, or null when the id is unknown.</summary>
        /// <param name="id">The buff id.</param>
        /// <returns>The buff or null.</returns>
        public Buff Get(string id)
        {
            Buff buff;
            return _buffsById.TryGetValue(id, out buff) ? buff : null;
        }
    }
}