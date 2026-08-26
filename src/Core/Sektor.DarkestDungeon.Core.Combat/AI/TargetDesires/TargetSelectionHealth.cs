using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Enums;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Target selection desire that targets allies by lowest health or specific class.</summary>
    public sealed class TargetSelectionHealth : TargetSelectionDesire
    {
        private string AllyBaseClassId { get; set; }

        /// <summary>Initializes a new instance of the <see cref="TargetSelectionHealth"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public TargetSelectionHealth(Dictionary<string, object> dataSet)
        {
            Type = TargetDesireType.Health;

            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override List<ICombatUnit> FilterTargets(ICombatUnit performer, List<ICombatUnit> possibleTargets)
        {
            var availableTargets = base.FilterTargets(performer, possibleTargets);

            if (!string.IsNullOrEmpty(AllyBaseClassId))
                availableTargets.RemoveAll(target => target.Character.Class != AllyBaseClassId);

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
                    case "ally_base_class_id":
                        AllyBaseClassId = (string)token.Value;
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
