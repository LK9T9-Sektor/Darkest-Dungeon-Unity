using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;
using Sektor.DarkestDungeon.Core.Combat.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Skill selection desire that picks the monster's preferable skill.</summary>
    public sealed class SkillSelectionPreferred : SkillSelectionDesire
    {
        /// <summary>Initializes a new instance of the <see cref="SkillSelectionPreferred"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public SkillSelectionPreferred(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override bool IsRestricted(ICombatUnit performer, IBattleContext battleContext)
        {
            if (base.IsRestricted(performer, battleContext))
                return true;

            if (performer.Character.PreferableSkill < 0)
                return true;

            if (performer.Character.PreferableSkill >= performer.Character.CombatSkills.Count)
                return true;

            return false;
        }

        /// <inheritdoc/>
        protected override bool IsValidSkill(ICombatUnit performer, CombatSkill skill, IBattleContext battleContext)
        {
            if (!base.IsValidSkill(performer, skill, battleContext))
                return false;

            return performer.Character.CombatSkills.IndexOf(skill) == performer.Character.PreferableSkill;
        }
    }
}
