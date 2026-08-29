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
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Skill selection desire that exploits a status effect on targets.</summary>
    public sealed class SkillSelectionStatus : SkillSelectionDesire
    {
        private StatusType EffectStatus { get; set; }
        private string CombatSkillId { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SkillSelectionStatus"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public SkillSelectionStatus(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override bool IsValidSkill(ICombatUnit performer, CombatSkill skill, IBattleContext battleContext)
        {
            if (!base.IsValidSkill(performer, skill, battleContext))
                return false;

            if (CombatSkillId != null)
                return skill.Id == CombatSkillId;

            return skill.Effects.Any(effect => effect.SubEffects.Any(IsValidSubEffect));
        }

        /// <inheritdoc/>
        protected override bool IsValidTarget(ICombatUnit target)
        {
            return target.Character.GetStatusEffect(EffectStatus).IsApplied;
        }

        /// <inheritdoc/>
        protected override bool IsValidTargetDesire(TargetSelectionDesire desire)
        {
            return desire.Type == TargetDesireType.Marked;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "effect_key_status":
                        switch ((string)dataSet["effect_key_status"])
                        {
                            case "tagged":
                                EffectStatus = StatusType.Marked;
                                break;
                            case "poisoned":
                                EffectStatus = StatusType.Poison;
                                break;
                            case "bleeding":
                                EffectStatus = StatusType.Bleeding;
                                break;
                            case "stunned":
                                EffectStatus = StatusType.Stun;
                                break;
                        }
                        break;
                    case "combat_skill_id":
                        CombatSkillId = (string)dataSet["combat_skill_id"];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }

        private bool IsValidSubEffect(SubEffect subEffect)
        {
            if (subEffect.Type != EffectSubType.StatBuff)
                return false;

            return subEffect.TargetStatus == EffectStatus;
        }
    }
}
