using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Skill selection desire that activates when a specific ally type is alive.</summary>
    public sealed class SkillSelectionAllyAlive : SkillSelectionDesire
    {
        private string CombatSkillId { get; set; }
        private string AllyBaseClassId { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SkillSelectionAllyAlive"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public SkillSelectionAllyAlive(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override bool IsRestricted(ICombatUnit performer, IBattleContext battleContext)
        {
            if (base.IsRestricted(performer, battleContext))
                return true;

            if (performer.Party.Units.All(unit => unit.Character.Class != AllyBaseClassId))
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
        protected override bool IsValidTargetDesire(TargetSelectionDesire desire)
        {
            return desire.Type == TargetDesireType.AllyClass;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "ally_base_class_id":
                        AllyBaseClassId = (string)dataSet["ally_base_class_id"];
                        break;
                    case "combat_skill_id":
                        CombatSkillId = (string)dataSet["combat_skill_id"];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
