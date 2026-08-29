using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>Target selection desire for duels that only targets marked enemies (Darkest Dungeon "marked_target").</summary>
    public class DuelTargetSelectionMarked : TargetSelectionDesire
    {
        /// <summary>Initializes a new instance of the <see cref="DuelTargetSelectionMarked"/> class.</summary>
        /// <param name="chance">The proportional selection chance.</param>
        public DuelTargetSelectionMarked(int chance)
        {
            Type = TargetDesireType.Marked;
            Chance = chance;
            GenerateFromDataSet(new Dictionary<string, object>
            {
                { "specific_combat_skill_id", string.Empty },
                { "is_enemy_target_desire", true },
                { "is_friendly_target_desire", false },
            });
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