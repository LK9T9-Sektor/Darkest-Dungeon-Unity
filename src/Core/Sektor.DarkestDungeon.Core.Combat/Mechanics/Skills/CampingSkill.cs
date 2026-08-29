using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Common;

using Sektor.DarkestDungeon.Core.Combat.Campaign;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>Camping skill definition.</summary>
    public class CampingSkill : Skill
    {
        /// <summary>Gets or sets the time cost.</summary>
        public int TimeCost { get; set; }

        /// <summary>Gets or sets the usage limit.</summary>
        public int Limit { get; set; }

        /// <summary>Gets a value indicating whether this skill has an individual target.</summary>
        public bool HasIndividualTarget
        {
            get
            {
                return Effects.Find(effect => effect.Selection == CampTargetType.Individual) != null;
            }
        }

        /// <summary>Gets or sets the required classes.</summary>
        public List<string> Classes { get; set; }

        /// <summary>Gets or sets the camp effects.</summary>
        public List<CampEffect> Effects { get; set; }

        /// <summary>Gets or sets the currency cost.</summary>
        public CurrencyCost CurrencyCost { get; set; }

        /// <summary>Initializes a new instance of the <see cref="CampingSkill"/> class.</summary>
        public CampingSkill()
        {
            Effects = new List<CampEffect>();
            Classes = new List<string>();
        }
    }
}
