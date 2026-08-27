using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Content.Database
{
    /// <summary>Root object of the JsonBuffs.json content file.</summary>
    public class JsonBuffData
    {
        /// <summary>Gets or sets the raw buff entries.</summary>
        public List<JsonBuff> buffs { get; set; }
    }
}