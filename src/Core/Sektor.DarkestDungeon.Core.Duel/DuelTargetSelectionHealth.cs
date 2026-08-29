using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>
    /// Health target selection desire for duels, mirroring Darkest Dungeon's "health_target":
    /// picks the lowest-health target (or highest when <c>is_greater_comparison</c>). Used for
    /// healing the most wounded ally and, optionally, focusing the most wounded enemy.
    /// </summary>
    public class DuelTargetSelectionHealth : TargetSelectionDesire
    {
        private readonly bool greater;

        /// <summary>Initializes a new instance of the <see cref="DuelTargetSelectionHealth"/> class.</summary>
        /// <param name="greater">When true, picks the highest-health target instead of the lowest.</param>
        /// <param name="enemy">Whether this desire applies to enemy-targeting skills.</param>
        /// <param name="friendly">Whether this desire applies to self-formation (heal) skills.</param>
        /// <param name="chance">The proportional selection chance.</param>
        public DuelTargetSelectionHealth(bool greater, bool enemy, bool friendly, int chance)
        {
            Type = TargetDesireType.Health;
            Chance = chance;
            this.greater = greater;
            GenerateFromDataSet(new Dictionary<string, object>
            {
                { "specific_combat_skill_id", string.Empty },
                { "is_enemy_target_desire", enemy },
                { "is_friendly_target_desire", friendly },
            });
        }

        /// <inheritdoc/>
        protected override bool ChooseTargets(List<ICombatUnit> availableTargets, MonsterBrainDecision decision)
        {
            if (availableTargets.Count == 0)
                return false;

            decision.TargetInfo.Targets.Clear();

            if (decision.SelectedSkill != null && decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }

            var chosen = availableTargets[0];
            for (int i = 1; i < availableTargets.Count; i++)
            {
                bool better = greater
                    ? availableTargets[i].Character.HealthRatio > chosen.Character.HealthRatio
                    : availableTargets[i].Character.HealthRatio < chosen.Character.HealthRatio;
                if (better)
                    chosen = availableTargets[i];
            }

            decision.TargetInfo.Targets.Add(chosen);
            return true;
        }
    }
}