using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
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
