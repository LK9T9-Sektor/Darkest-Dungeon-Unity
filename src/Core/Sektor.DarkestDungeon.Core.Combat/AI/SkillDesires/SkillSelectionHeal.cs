using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Enums;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;
using Sektor.DarkestDungeon.Core.Combat.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Skill selection desire that picks a heal skill when an ally's HP is low.</summary>
    public sealed class SkillSelectionHeal : SkillSelectionDesire
    {
        private string CombatSkillId { get; set; }
        private float HpRatioThreshold { get; set; }
        private bool FirstInitiativeOnly { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SkillSelectionHeal"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public SkillSelectionHeal(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override bool IsRestricted(ICombatUnit performer, IBattleContext battleContext)
        {
            if (base.IsRestricted(performer, battleContext))
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

            if (string.IsNullOrEmpty(CombatSkillId))
                return skill.Id == CombatSkillId;

            return skill.Heal != null;
        }

        /// <inheritdoc/>
        protected override bool IsValidTarget(ICombatUnit target)
        {
            return target.Character.HealthRatio < HpRatioThreshold;
        }

        /// <inheritdoc/>
        protected override bool IsValidTargetDesire(TargetSelectionDesire desire)
        {
            return desire.Type == TargetDesireType.Health;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "hp_ratio_treshold":
                        HpRatioThreshold = (float)(double)dataSet["hp_ratio_treshold"];
                        break;
                    case "first_initiative_only":
                        FirstInitiativeOnly = (bool)dataSet[token.Key];
                        break;
                    case "combat_skill_id":
                        CombatSkillId = (string)dataSet[token.Key];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
