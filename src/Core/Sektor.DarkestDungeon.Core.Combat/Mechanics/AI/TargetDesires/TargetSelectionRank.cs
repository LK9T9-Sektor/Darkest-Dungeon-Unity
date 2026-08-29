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
    /// <summary>Target selection desire that targets specific formation ranks.</summary>
    public sealed class TargetSelectionRank : TargetSelectionDesire
    {
        /// <summary>Initializes a new instance of the <see cref="TargetSelectionRank"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public TargetSelectionRank(Dictionary<string, object> dataSet)
        {
            Type = TargetDesireType.Rank;

            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override List<ICombatUnit> FilterTargets(ICombatUnit performer, List<ICombatUnit> possibleTargets)
        {
            var availableTargets = base.FilterTargets(performer, possibleTargets);

            availableTargets.RemoveAll(target => !target.Party.MarkedRanks.Contains(target.Rank));
            return availableTargets;
        }
    }
}
