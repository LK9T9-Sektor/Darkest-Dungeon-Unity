using System.Collections.Generic;
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

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Target selection desire that targets stressed enemies.</summary>
    public sealed class TargetSelectionStress : TargetSelectionDesire
    {
        /// <summary>Initializes a new instance of the <see cref="TargetSelectionStress"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public TargetSelectionStress(Dictionary<string, object> dataSet)
        {
            Type = TargetDesireType.Stress;

            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override List<ICombatUnit> FilterTargets(ICombatUnit performer, List<ICombatUnit> possibleTargets)
        {
            var availableTargets = base.FilterTargets(performer, possibleTargets);

            availableTargets.RemoveAll(target => !target.Character.IsStressed);
            return availableTargets;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "is_greater_comparison":
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
