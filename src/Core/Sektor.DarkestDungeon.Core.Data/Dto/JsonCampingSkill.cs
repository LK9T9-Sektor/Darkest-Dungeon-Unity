using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>A camping skill definition.</summary>
    public class JsonCampingSkill
    {
        /// <summary>Gets or sets the skill id.</summary>
        public string id { get; set; }

        /// <summary>Gets or sets the skill level.</summary>
        public int level { get; set; }

        /// <summary>Gets or sets the camping point cost.</summary>
        public int cost { get; set; }

        /// <summary>Gets or sets the use limit per camping session.</summary>
        public int use_limit { get; set; }

        /// <summary>Gets or sets the resolved camping effects.</summary>
        public List<JsonCampingSkillEffect> effects { get; set; }

        /// <summary>Gets or sets the hero classes that can learn the skill.</summary>
        public List<string> hero_classes { get; set; }

        /// <summary>Gets or sets the upgrade requirements by level.</summary>
        public List<JsonCampingUpgradeRequirement> upgrade_requirements { get; set; }

        /// <summary>Gets or sets the buff ids applied while camping.</summary>
        public List<string> camping_buff_ids { get; set; }
    }
}