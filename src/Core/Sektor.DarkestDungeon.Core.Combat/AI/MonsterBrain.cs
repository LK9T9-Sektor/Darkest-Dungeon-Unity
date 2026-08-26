using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>
    /// Core representation of a monster's AI brain, holding cooldowns and desire sets.
    /// </summary>
    public class MonsterBrain
    {
        /// <summary>Gets or sets the brain identifier.</summary>
        public string Id { get; set; }

        /// <summary>Gets the list of skill cooldowns.</summary>
        public List<SkillCooldown> SkillCooldowns { get; }

        /// <summary>Gets the list of skill selection desires.</summary>
        public List<SkillSelectionDesire> SkillDesireSet { get; }

        /// <summary>Gets the list of target selection desires.</summary>
        public List<TargetSelectionDesire> TargetDesireSet { get; }

        /// <summary>Gets the list of bonus initiative desires.</summary>
        public List<BonusInitiativeDesire> BonusDesireSet { get; }

        /// <summary>Initializes a new instance of the <see cref="MonsterBrain"/> class.</summary>
        public MonsterBrain()
        {
            SkillCooldowns = new List<SkillCooldown>();
            SkillDesireSet = new List<SkillSelectionDesire>();
            TargetDesireSet = new List<TargetSelectionDesire>();
            BonusDesireSet = new List<BonusInitiativeDesire>();
        }
    }
}
