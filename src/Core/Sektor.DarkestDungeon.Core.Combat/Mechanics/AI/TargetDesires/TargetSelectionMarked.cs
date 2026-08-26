using System.Collections.Generic;
using System.Linq;
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
    /// <summary>Target selection desire that targets only marked enemies.</summary>
    public sealed class TargetSelectionMarked : TargetSelectionDesire
    {
        /// <summary>Initializes a new instance of the <see cref="TargetSelectionMarked"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public TargetSelectionMarked(Dictionary<string, object> dataSet)
        {
            Type = TargetDesireType.Marked;

            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        public override bool SelectTarget(ICombatUnit performer, MonsterBrainDecision decision)
        {
            if (decision.TargetInfo.Targets.All(target => !target.Character.GetStatusEffect(StatusType.Marked).IsApplied))
                return false;

            return base.SelectTarget(performer, decision);
        }

        /// <inheritdoc/>
        protected override List<ICombatUnit> FilterTargets(ICombatUnit performer, List<ICombatUnit> possibleTargets)
        {
            var availableTargets = base.FilterTargets(performer, possibleTargets);

            availableTargets.RemoveAll(target => !target.Character.GetStatusEffect(StatusType.Marked).IsApplied);
            return availableTargets;
        }
    }
}
