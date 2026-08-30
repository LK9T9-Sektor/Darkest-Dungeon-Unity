using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Common;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
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
