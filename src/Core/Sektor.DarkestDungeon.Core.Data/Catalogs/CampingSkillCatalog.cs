using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Content.Camping;
using Sektor.DarkestDungeon.Core.Data.Dto;

namespace Sektor.DarkestDungeon.Core.Data.Catalogs
{
    /// <summary>Loads camping skill definitions from the campaign JsonCamping.json content into <see cref="CampingSkill"/> instances.</summary>
    public sealed class CampingSkillCatalog
    {
        private readonly Dictionary<string, CampingSkill> _byId = new Dictionary<string, CampingSkill>();

        private CampingSkillCatalog()
        {
        }

        /// <summary>Gets all camping skills ordered by their wire order.</summary>
        public IReadOnlyCollection<CampingSkill> All { get { return _byId.Values; } }

        /// <summary>Builds a camping skill catalog from the parsed JsonCamping document.</summary>
        /// <param name="data">The parsed JsonCamping.json document.</param>
        /// <returns>The camping skill catalog.</returns>
        public static CampingSkillCatalog Load(JsonCamping data)
        {
            var catalog = new CampingSkillCatalog();
            if (data?.skills == null)
                return catalog;

            foreach (JsonCampingSkill jsonSkill in data.skills)
            {
                var skill = new CampingSkill(
                    jsonSkill.id,
                    jsonSkill.level,
                    jsonSkill.cost,
                    jsonSkill.use_limit,
                    CopyList(jsonSkill.hero_classes),
                    CopyList(jsonSkill.camping_buff_ids));
                catalog._byId[skill.Id] = skill;
            }

            return catalog;
        }

        /// <summary>Gets a camping skill by id, or null when the id is unknown.</summary>
        /// <param name="id">The skill id.</param>
        /// <returns>The camping skill or null.</returns>
        public CampingSkill Get(string id)
        {
            CampingSkill skill;
            return _byId.TryGetValue(id, out skill) ? skill : null;
        }

        private static List<string> CopyList(List<string> source)
        {
            return source == null ? new List<string>() : new List<string>(source);
        }
    }
}