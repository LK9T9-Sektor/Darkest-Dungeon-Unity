using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>A hero class definition: base stats, resistances, skills, modes and tags.</summary>
    public class HeroClass
    {
        /// <summary>Gets or sets the numeric class index.</summary>
        public int IndexId { get; set; }

        /// <summary>Gets or sets the string class identifier.</summary>
        public string StringId { get; set; }

        /// <summary>Gets or sets the base attributes (HP, speed, accuracy, damage, ...).</summary>
        public Dictionary<AttributeType, float> Attributes { get; set; }

        /// <summary>Gets or sets the resistances.</summary>
        public Dictionary<AttributeType, float> Resistances { get; set; }

        /// <summary>Gets or sets the combat skills.</summary>
        public List<CombatSkill> CombatSkills { get; set; }

        /// <summary>Gets or sets the camping skills.</summary>
        public List<CampingSkill> CampingSkills { get; set; }

        /// <summary>Gets or sets the skill art info.</summary>
        public List<SkillArtInfo> SkillArtInfo { get; set; }

        /// <summary>Gets or sets the riposte skill.</summary>
        public CombatSkill RiposteSkill { get; set; }

        /// <summary>Gets or sets the available modes.</summary>
        public List<CharacterMode> Modes { get; set; }

        /// <summary>Gets or sets the class tags (e.g. "religious").</summary>
        public List<string> Tags { get; set; }

        /// <summary>Gets or sets a value indicating whether the player may select the active combat skills.</summary>
        public bool CanSelectCombatSkills { get; set; }

        /// <summary>Gets or sets the maximum number of active combat skills the hero may bring into battle.</summary>
        public int NumberOfSelectedCombatSkills { get; set; }

        /// <summary>Initializes a new instance of the <see cref="HeroClass"/> class.</summary>
        public HeroClass()
        {
            Attributes = new Dictionary<AttributeType, float>();
            Resistances = new Dictionary<AttributeType, float>();
            CombatSkills = new List<CombatSkill>();
            CampingSkills = new List<CampingSkill>();
            SkillArtInfo = new List<SkillArtInfo>();
            Modes = new List<CharacterMode>();
            Tags = new List<string>();
        }

        /// <summary>Gets a value indicating whether the class is religious.</summary>
        public bool IsReligious { get { return Tags.Contains("religious"); } }
    }
}