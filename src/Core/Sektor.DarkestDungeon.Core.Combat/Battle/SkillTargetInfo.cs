using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Enums;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;
using Sektor.DarkestDungeon.Core.Combat.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Battle
{
    /// <summary>Targeting context for a skill execution.</summary>
    public class SkillTargetInfo
    {
        /// <summary>Gets or sets the list of target units.</summary>
        public List<ICombatUnit> Targets { get; set; }

        /// <summary>Gets or sets the target type.</summary>
        public SkillTargetType Type { get; set; }

        /// <summary>Gets the character mode.</summary>
        public ICharacterMode Mode { get; private set; }

        /// <summary>Gets the combat skill.</summary>
        public CombatSkill Skill { get; private set; }

        /// <summary>Gets the skill art info.</summary>
        public SkillArtInfo SkillArtInfo { get; private set; }

        /// <summary>Updates the skill info from the performer.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="skill">The skill being used.</param>
        /// <returns>This instance for chaining.</returns>
        public SkillTargetInfo UpdateSkillInfo(ICombatUnit performer, CombatSkill skill)
        {
            Mode = performer.Character.Mode;
            Skill = skill;
            SkillArtInfo = performer.Character.SkillArtInfo.Find(info => info.SkillId == skill.Id);

            if (skill.LimitPerBattle.HasValue)
                performer.CombatInfo.SkillsUsedInBattle.Add(skill.Id);
            if (skill.LimitPerTurn.HasValue)
                performer.CombatInfo.SkillsUsedThisTurn.Add(skill.Id);

            return this;
        }

        /// <summary>Initializes a new instance of the <see cref="SkillTargetInfo"/> class.</summary>
        /// <param name="targets">The target units.</param>
        /// <param name="type">The target type.</param>
        public SkillTargetInfo(List<ICombatUnit> targets, SkillTargetType type)
        {
            Targets = targets;
            Type = type;
        }

        /// <summary>Initializes a new instance of the <see cref="SkillTargetInfo"/> class.</summary>
        /// <param name="target">The single target unit.</param>
        /// <param name="type">The target type.</param>
        public SkillTargetInfo(ICombatUnit target, SkillTargetType type)
        {
            Targets = new List<ICombatUnit> { target };
            Type = type;
        }

        /// <summary>Initializes a new instance of the <see cref="SkillTargetInfo"/> class.</summary>
        /// <param name="type">The target type.</param>
        public SkillTargetInfo(SkillTargetType type)
        {
            Targets = new List<ICombatUnit>();
            Type = type;
        }
    }
}
