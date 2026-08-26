using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Enums;
using Sektor.DarkestDungeon.Core.Content.Raid;

namespace Sektor.DarkestDungeon.Core.Combat.Skills
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

    /// <summary>A single camping effect.</summary>
    public class CampEffect : ISingleProportion
    {
        /// <summary>Gets or sets the selection target type.</summary>
        public CampTargetType Selection { get; set; }

        /// <summary>Gets or sets the requirement.</summary>
        public CampEffectRequirement Requirement { get; set; }

        /// <summary>Gets or sets the effect type.</summary>
        public CampEffectType Type { get; set; }

        /// <summary>Gets or sets the subtype.</summary>
        public string Subtype { get; set; }

        /// <summary>Gets or sets the amount.</summary>
        public float Amount { get; set; }

        /// <summary>Gets or sets the chance (0-1).</summary>
        public float Chance { get; set; }

        /// <summary>Gets or sets the code.</summary>
        public string Code { get; set; }
    }
}
