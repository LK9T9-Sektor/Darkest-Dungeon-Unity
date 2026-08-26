using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Enums;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;
using Sektor.DarkestDungeon.Core.Combat.Random;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Target selection desire that targets the enemy with the lowest resistance.</summary>
    public sealed class TargetSelectionResistance : TargetSelectionDesire
    {
        private AttributeType ResistanceType { get; set; }

        /// <summary>Initializes a new instance of the <see cref="TargetSelectionResistance"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public TargetSelectionResistance(Dictionary<string, object> dataSet)
        {
            Type = TargetDesireType.Resistance;

            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        public override bool SelectTarget(ICombatUnit performer, MonsterBrainDecision decision)
        {
            if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation ||
                decision.SelectedSkill.TargetRanks.IsSelfTarget))
            {
                var availableTargets = FilterTargets(performer, decision.TargetInfo.Targets);
                return ChooseTargets(availableTargets, decision);
            }

            return base.SelectTarget(performer, decision);
        }

        /// <inheritdoc/>
        protected override bool ChooseTargets(List<ICombatUnit> availableTargets, MonsterBrainDecision decision)
        {
            if (availableTargets.Count > 0)
            {
                decision.TargetInfo.Targets.Clear();

                if (decision.SelectedSkill.TargetRanks.IsMultitarget)
                {
                    decision.TargetInfo.Targets.AddRange(availableTargets);
                    return true;
                }
                else
                {
                    float lowestRes = float.MaxValue;
                    ICombatUnit lowestResTarget = null;
                    foreach (var target in availableTargets)
                    {
                        if (target.Character.GetSingleAttribute(ResistanceType).ModifiedValue < lowestRes)
                        {
                            lowestRes = target.Character.GetSingleAttribute(ResistanceType).ModifiedValue;
                            lowestResTarget = target;
                        }
                    }
                    decision.TargetInfo.Targets.Add(lowestResTarget);
                    availableTargets.Remove(lowestResTarget);

                    if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                        RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                    {
                        lowestRes = 500f;
                        lowestResTarget = null;
                        foreach (var target in availableTargets)
                        {
                            if (target.Character.GetSingleAttribute(ResistanceType).ModifiedValue < lowestRes)
                            {
                                lowestRes = target.Character.GetSingleAttribute(ResistanceType).ModifiedValue;
                                lowestResTarget = target;
                            }
                        }
                        if (lowestResTarget != null)
                            decision.TargetInfo.Targets.Add(lowestResTarget);
                        return true;
                    }
                    return true;
                }
            }
            return false;
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
                    case "can_target_deaths_door":
                        Parameters[TargetSelectParameter.CanTargetDeathsDoor] = (bool)token.Value;
                        break;
                    case "can_target_last_hero":
                        Parameters[TargetSelectParameter.CanTargetLastHero] = (bool)token.Value;
                        break;
                    case "resistance_type_id":
                        ResistanceType = CharacterHelper.StringToAttributeType((string)token.Value);
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
