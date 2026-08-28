using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>
    /// Plays the rival side of a duel like a Darkest Dungeon monster: a default brain with weighted
    /// skill desires (heal when an ally is wounded, otherwise a random legal skill) and weighted
    /// target desires (random / marked enemies, lowest-health ally for heals). Selection runs
    /// through the deterministic <c>RandomSolver</c> and applies skill cooldowns.
    /// </summary>
    public class DuelAi
    {
        private readonly MonsterBrain brain;

        /// <summary>Initializes a new instance of the <see cref="DuelAi"/> class with the default Darkest Dungeon brain.</summary>
        public DuelAi()
        {
            brain = BuildDefaultBrain();
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
            var skillDesires = new List<SkillSelectionDesire>(brain.SkillDesireSet);
            while (skillDesires.Count > 0)
            {
                SkillSelectionDesire desire = RandomSolver.ChooseByRandom(skillDesires);
                if (desire != null && desire.SelectSkill(performer, decision, duel.Context))
                {
                    var cooldown = brain.SkillCooldowns.Find(cd => cd.SkillId == decision.SelectedSkill.Id);
                    if (cooldown != null)
                        performer.CombatInfo.SkillCooldowns.Add(cooldown.Copy());
                    break;
                }
                skillDesires.Remove(desire);
            }

            if (decision.Decision != BrainDecisionType.Perform || decision.SelectedSkill == null || decision.TargetInfo.Targets.Count == 0)
                return DuelPayload.PassAction();

            return DuelPayload.Skill(decision.SelectedSkill.Id, decision.TargetInfo.Targets[0].CombatInfo.CombatId);
        }

        private static MonsterBrain BuildDefaultBrain()
        {
            var brain = new MonsterBrain();
            brain.SkillDesireSet.Add(new DuelSkillSelectionHeal(brain, 0.5f, 100));
            brain.SkillDesireSet.Add(new DuelSkillSelection(brain, 1));
            brain.TargetDesireSet.Add(new DuelTargetSelectionRandom(2));
            brain.TargetDesireSet.Add(new DuelTargetSelectionMarked(1));
            brain.TargetDesireSet.Add(new DuelTargetSelectionHealth(greater: false, enemy: false, friendly: true, chance: 100));
            return brain;
        }
    }
}