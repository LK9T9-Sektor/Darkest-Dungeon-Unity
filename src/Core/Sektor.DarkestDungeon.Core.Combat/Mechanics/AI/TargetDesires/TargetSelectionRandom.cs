using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Target selection desire that picks random targets with configurable filters.</summary>
    public sealed class TargetSelectionRandom : TargetSelectionDesire
    {
        /// <summary>Initializes a new instance of the <see cref="TargetSelectionRandom"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public TargetSelectionRandom(Dictionary<string, object> dataSet)
        {
            Type = TargetDesireType.Random;

            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "can_target_deaths_door":
                        Parameters[TargetSelectParameter.CanTargetDeathsDoor] = (bool)token.Value;
                        break;
                    case "can_target_last_hero":
                        Parameters[TargetSelectParameter.CanTargetLastHero] = (bool)token.Value;
                        break;
                    case "can_target_not_overstressed":
                        Parameters[TargetSelectParameter.CanTargetNotOverstressed] = (bool)token.Value;
                        break;
                    case "can_target_afflicted":
                        Parameters[TargetSelectParameter.CanTargetAfflicted] = (bool)token.Value;
                        break;
                    case "can_target_virtued":
                        Parameters[TargetSelectParameter.CanTargetVirtued] = (bool)token.Value;
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
