using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Root object of the JsonAI.json content file.</summary>
    public class JsonMonsterBrainsDatabase
    {
        /// <summary>Gets or sets the raw monster brain entries.</summary>
        public List<JsonMonsterBrains> monster_brains { get; set; }
    }
}