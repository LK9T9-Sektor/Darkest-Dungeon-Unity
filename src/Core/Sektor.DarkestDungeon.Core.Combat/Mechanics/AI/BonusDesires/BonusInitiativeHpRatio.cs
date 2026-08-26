using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Bonus initiative desire that grants a bonus based on the performer's HP ratio.</summary>
    public sealed class BonusInitiativeHpRatio : BonusInitiativeDesire
    {
        private float Threshold { get; set; }
        private bool IsUnderThreshold { get; set; }
        private int? HeroesMin { get; set; }
        private int? HeroesMax { get; set; }

        /// <summary>Initializes a new instance of the <see cref="BonusInitiativeHpRatio"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public BonusInitiativeHpRatio(Dictionary<string, object> dataSet)
        {
            HeroesMin = 0;
            HeroesMax = 4;

            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        public override bool CheckBonusInitiative(ICombatUnit performer, IBattleContext battleContext)
        {
            if (HeroesMin.HasValue)
                if (HeroesMin.Value > battleContext.BattleGround.HeroNumber)
                    return false;
            if (HeroesMax.HasValue)
                if (HeroesMax.Value < battleContext.BattleGround.HeroNumber)
                    return false;

            if (IsUnderThreshold && performer.Character.HealthRatio <= Threshold)
                return true;
            if (!IsUnderThreshold && performer.Character.HealthRatio >= Threshold)
                return true;

            return false;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "heroes_min":
                        HeroesMin = (int)(long)dataSet[token.Key];
                        break;
                    case "heroes_max":
                        HeroesMax = (int)(long)dataSet[token.Key];
                        break;
                    case "health_ratio_threshold":
                        Threshold = (float)(double)dataSet[token.Key];
                        break;
                    case "is_under_threshold":
                        IsUnderThreshold = (bool)dataSet[token.Key];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
