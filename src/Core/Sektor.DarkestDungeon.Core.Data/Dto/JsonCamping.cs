using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Data.Dto
{
    /// <summary>Root document of Data\JsonCamping.json.</summary>
    public class JsonCamping
    {
        /// <summary>Gets or sets the camping configuration.</summary>
        public JsonCampingConfiguration configuration { get; set; }

        /// <summary>Gets or sets the camping skills.</summary>
        public List<JsonCampingSkill> skills { get; set; }
    }
}