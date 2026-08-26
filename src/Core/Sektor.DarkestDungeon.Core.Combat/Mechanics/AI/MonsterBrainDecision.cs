using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>
    /// Represents a decision made by a monster brain: pass or perform a skill.
    /// </summary>
    public class MonsterBrainDecision
    {
        /// <summary>Gets or sets the decision type.</summary>
        public BrainDecisionType Decision { get; set; }

        /// <summary>Gets or sets the selected combat skill.</summary>
        public CombatSkill SelectedSkill { get; set; }

        /// <summary>Gets the target information for the decision.</summary>
        public SkillTargetInfo TargetInfo { get; }

        /// <summary>Initializes a new instance of the <see cref="MonsterBrainDecision"/> class.</summary>
        /// <param name="decision">The decision type.</param>
        public MonsterBrainDecision(BrainDecisionType decision)
        {
            Decision = decision;
            TargetInfo = new SkillTargetInfo(SkillTargetType.Self);
        }
    }
}
