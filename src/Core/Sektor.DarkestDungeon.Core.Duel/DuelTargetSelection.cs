using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>Target selection desire for duels: prefers the enemy with the lowest hit points.</summary>
    public class DuelTargetSelection : TargetSelectionDesire
    {
        /// <summary>Selects the lowest-health target (keeps all targets for multi-target skills).</summary>
        /// <param name="performer">The acting unit.</param>
        /// <param name="decision">The decision being populated.</param>
        /// <returns>True when a target was selected.</returns>
        public override bool SelectTarget(ICombatUnit performer, MonsterBrainDecision decision)
        {
            var targets = decision.TargetInfo.Targets;
            if (targets.Count == 0)
                return false;

            if (decision.SelectedSkill != null && decision.SelectedSkill.TargetRanks.IsMultitarget)
                return true;

            var chosen = targets[0];
            for (int i = 1; i < targets.Count; i++)
            {
                if (targets[i].Character.HealthRatio < chosen.Character.HealthRatio)
                    chosen = targets[i];
            }

            decision.TargetInfo.Targets = new List<ICombatUnit> { chosen };
            return true;
        }
    }
}