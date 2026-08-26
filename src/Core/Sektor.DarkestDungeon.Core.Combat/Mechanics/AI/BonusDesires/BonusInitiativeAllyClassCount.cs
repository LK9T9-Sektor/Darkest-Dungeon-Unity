using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Bonus initiative desire that grants a bonus based on ally class count.</summary>
    public sealed class BonusInitiativeAllyClassCount : BonusInitiativeDesire
    {
        private string AllyBaseClass { get; set; }
        private int? AllyCountMin { get; set; }
        private int? AllyCountMax { get; set; }
        private int? MonstersMin { get; set; }
        private int? MonstersMax { get; set; }

        /// <summary>Initializes a new instance of the <see cref="BonusInitiativeAllyClassCount"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public BonusInitiativeAllyClassCount(Dictionary<string, object> dataSet)
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

            int allyCount = battleContext.BattleGround.MonsterParty.Units
                .FindAll(unit => unit.Character.Class == AllyBaseClass).Count;
            if (allyCount == 0) return false;

            if (AllyCountMin > allyCount)
                return false;
            if (AllyCountMax < allyCount)
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
                    case "ally_base_class_id":
                        AllyBaseClass = (string)dataSet["ally_base_class_id"];
                        break;
                    case "ally_count_min":
                        AllyCountMin = (int)(long)dataSet["ally_count_min"];
                        break;
                    case "ally_count_max":
                        AllyCountMax = (int)(long)dataSet["ally_count_max"];
                        break;
                    case "monsters_min":
                        MonstersMin = (int)(long)dataSet["monsters_min"];
                        break;
                    case "monsters_max":
                        MonstersMax = (int)(long)dataSet["monsters_max"];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
