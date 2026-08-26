using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;
using Sektor.DarkestDungeon.Core.Combat.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Skill selection desire that forces a specific skill on a specific round.</summary>
    public sealed class SkillSelectionPerformingTurn : SkillSelectionDesire
    {
        private string CombatSkillId { get; set; }
        private int PerformingTurn { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SkillSelectionPerformingTurn"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public SkillSelectionPerformingTurn(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override bool IsRestricted(ICombatUnit performer, IBattleContext battleContext)
        {
            if (base.IsRestricted(performer, battleContext))
                return true;

            if (PerformingTurn != battleContext.BattleGround.Round.RoundNumber)
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
                    case "performing_turn":
                        PerformingTurn = (int)(long)dataSet["performing_turn"];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
