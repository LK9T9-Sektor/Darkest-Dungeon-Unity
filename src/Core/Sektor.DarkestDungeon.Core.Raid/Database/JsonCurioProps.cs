using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Raid.Database
{
    /// <summary>Root document of Data\Curios\Obstacles.json and Data\Curios\Traps.json.</summary>
    public class JsonCurioProps
    {
        /// <summary>Gets or sets the curio prop definitions.</summary>
        public List<JsonCurioProp> props { get; set; }
    }
}