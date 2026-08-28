using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>
    /// Plays the rival side of a duel through the core brain/desire infrastructure: picks a legal
    /// skill (via <see cref="DuelSkillSelection"/>) and a low-health target (via
    /// <see cref="DuelTargetSelection"/>). Selection uses a client-local RNG so the deterministic
    /// simulation stays in lockstep; only the chosen payload is broadcast.
    /// </summary>
    public class DuelAi
    {
        private readonly Random random = new Random();
        private readonly MonsterBrain brain;

        /// <summary>Initializes a new instance of the <see cref="DuelAi"/> class.</summary>
        public DuelAi()
        {
            brain = new MonsterBrain();
            brain.SkillDesireSet.Add(new DuelSkillSelection(random));
            brain.TargetDesireSet.Add(new DuelTargetSelection());
        }

        /// <summary>Chooses the rival's next action payload ("skillId|targetId" or "pass|0").</summary>
        /// <param name="duel">The duel the acting rival unit belongs to.</param>
        /// <returns>The wire payload to broadcast.</returns>
        public string ChooseAction(DuelController duel)
        {
            var performer = duel.CurrentUnit;
            if (performer == null || duel.Context == null)
                return DuelPayload.PassAction();

            var decision = new MonsterBrainDecision(BrainDecisionType.Pass);
            if (!TrySelectSkill(performer, decision, duel.Context))
                return DuelPayload.PassAction();
            if (decision.TargetInfo.Targets.Count == 0)
                return DuelPayload.PassAction();
            if (!TrySelectTarget(performer, decision) || decision.TargetInfo.Targets.Count == 0)
                return DuelPayload.PassAction();

            return DuelPayload.Skill(decision.SelectedSkill.Id, decision.TargetInfo.Targets[0].CombatInfo.CombatId);
        }

        private bool TrySelectSkill(ICombatUnit performer, MonsterBrainDecision decision, IBattleContext battleContext)
        {
            var desires = new List<SkillSelectionDesire>(brain.SkillDesireSet);
            while (desires.Count > 0)
            {
                SkillSelectionDesire desire = desires[random.Next(desires.Count)];
                DuelSkillSelection duelSelection = desire as DuelSkillSelection;
                if (duelSelection != null && duelSelection.SelectSkill(performer, decision, battleContext))
                    return true;
                desires.Remove(desire);
            }
            return false;
        }

        private bool TrySelectTarget(ICombatUnit performer, MonsterBrainDecision decision)
        {
            var desires = new List<TargetSelectionDesire>(brain.TargetDesireSet);
            while (desires.Count > 0)
            {
                TargetSelectionDesire desire = desires[random.Next(desires.Count)];
                if (desire.SelectTarget(performer, decision))
                    return true;
                desires.Remove(desire);
            }
            return false;
        }
    }
}