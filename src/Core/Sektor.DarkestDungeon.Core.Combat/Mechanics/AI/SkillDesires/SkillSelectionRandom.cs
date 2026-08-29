using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Skill selection desire that selects any valid skill at random.</summary>
    public sealed class SkillSelectionRandom : SkillSelectionDesire
    {
        /// <summary>Initializes a new instance of the <see cref="SkillSelectionRandom"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public SkillSelectionRandom(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }
    }
}
