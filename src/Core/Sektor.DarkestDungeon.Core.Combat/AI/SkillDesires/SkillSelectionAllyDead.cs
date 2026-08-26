using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Enums;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;
using Sektor.DarkestDungeon.Core.Combat.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Skill selection desire that activates when a specific ally type is dead.</summary>
    public sealed class SkillSelectionAllyDead : SkillSelectionDesire
    {
        private bool FirstInitiativeOnly { get; set; }
        private string CombatSkillId { get; set; }
        private string AllyBaseClassId { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SkillSelectionAllyDead"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public SkillSelectionAllyDead(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override bool IsRestricted(ICombatUnit performer, IBattleContext battleContext)
        {
            if (base.IsRestricted(performer, battleContext))
                return true;

            if (performer.Party.Units.Any(unit => unit.Character.Class == AllyBaseClassId))
                return true;

            if (FirstInitiativeOnly && performer.CombatInfo.CurrentInitiative != 1)
                return true;

            return false;
        }

        /// <inheritdoc/>
        protected override bool IsValidSkill(ICombatUnit performer, CombatSkill skill, IBattleContext battleContext)
        {
            if (!base.IsValidSkill(performer, skill, battleContext))
                return false;

            return skill.Id == CombatSkillId;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "combat_skill_id":
                        CombatSkillId = (string)dataSet["combat_skill_id"];
                        break;
                    case "ally_base_class_id":
                        AllyBaseClassId = (string)dataSet["ally_base_class_id"];
                        break;
                    case "first_initiative_only":
                        FirstInitiativeOnly = (bool)dataSet[token.Key];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
