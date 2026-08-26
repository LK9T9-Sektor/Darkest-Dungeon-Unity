using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Enums;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Target selection desire that targets allies of a specific class.</summary>
    public sealed class TargetSelectionAllyClass : TargetSelectionDesire
    {
        private string AllyBaseClass { get; set; }

        /// <summary>Initializes a new instance of the <see cref="TargetSelectionAllyClass"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public TargetSelectionAllyClass(Dictionary<string, object> dataSet)
        {
            Type = TargetDesireType.AllyClass;

            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override List<ICombatUnit> FilterTargets(ICombatUnit performer, List<ICombatUnit> possibleTargets)
        {
            var availableTargets = base.FilterTargets(performer, possibleTargets);

            availableTargets.RemoveAll(target => target.Character.Class != AllyBaseClass);
            return availableTargets;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "ally_base_class_id":
                        AllyBaseClass = (string)token.Value;
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
