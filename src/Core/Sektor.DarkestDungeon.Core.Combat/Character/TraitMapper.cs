using System;
using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Maps raw JsonTraits.json entries into core <see cref="Trait"/> objects.</summary>
    public static class TraitMapper
    {
        /// <summary>Parses the raw trait entries.</summary>
        /// <param name="data">The raw trait entries.</param>
        /// <returns>The parsed traits.</returns>
        public static List<Trait> Parse(List<JsonTrait> data)
        {
            var result = new List<Trait>();
            if (data == null)
                return result;

            foreach (var json in data)
            {
                OverstressType type;
                if (json == null || json.id == null || !Enum.TryParse(json.overstress_type, true, out type))
                    continue;

                var trait = new Trait { Id = json.id, Type = type };
                if (json.buff_ids != null)
                    trait.BuffIds.AddRange(json.buff_ids);
                result.Add(trait);
            }
            return result;
        }
    }
}