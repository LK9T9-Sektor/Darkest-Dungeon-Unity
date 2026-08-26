using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Bonus initiative desire that always grants a bonus with optional constraints.</summary>
    public sealed class BonusInitiativeGuaranteed : BonusInitiativeDesire
    {
        private int? MonstersMin { get; set; }
        private int? MonstersMax { get; set; }
        private int? MonstersSizeLimit { get; set; }

        /// <summary>Initializes a new instance of the <see cref="BonusInitiativeGuaranteed"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public BonusInitiativeGuaranteed(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        public override bool CheckBonusInitiative(ICombatUnit performer, IBattleContext battleContext)
        {
            if (MonstersMin != null)
                if (MonstersMin.Value > battleContext.BattleGround.MonsterNumber)
                    return false;
            if (MonstersMax != null)
                if (MonstersMax.Value < battleContext.BattleGround.MonsterNumber)
                    return false;
            if (MonstersSizeLimit != null)
                if (MonstersSizeLimit.Value < battleContext.BattleGround.MonsterSize)
                    return false;

            return true;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "monsters_min":
                        MonstersMin = (int)(long)dataSet["monsters_min"];
                        break;
                    case "monsters_max":
                        MonstersMax = (int)(long)dataSet["monsters_max"];
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
