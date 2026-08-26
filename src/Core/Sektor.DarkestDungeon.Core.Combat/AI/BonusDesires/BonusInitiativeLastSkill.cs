using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Bonus initiative desire that grants a bonus when the last used skill matches.</summary>
    public sealed class BonusInitiativeLastSkill : BonusInitiativeDesire
    {
        private string LastCombatSkill { get; set; }
        private int? MonstersSizeLimit { get; set; }

        /// <summary>Initializes a new instance of the <see cref="BonusInitiativeLastSkill"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public BonusInitiativeLastSkill(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        public override bool CheckBonusInitiative(ICombatUnit performer, IBattleContext battleContext)
        {
            if (MonstersSizeLimit != null)
                if (MonstersSizeLimit.Value < battleContext.BattleGround.MonsterSize)
                    return false;

            if (LastCombatSkill == null || battleContext.BattleGround.LastSkillUsed == null
                || battleContext.BattleGround.LastSkillUsed != LastCombatSkill)
                return false;

            battleContext.BattleGround.LastSkillUsed = null;

            return true;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "last_combat_skill_id":
                        LastCombatSkill = (string)dataSet["last_combat_skill_id"];
                        break;
                    case "monsters_size_limit":
                        MonstersSizeLimit = (int)(long)dataSet["monsters_size_limit"];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
