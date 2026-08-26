using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Enums;
using Sektor.DarkestDungeon.Core.Combat.Interfaces;
using Sektor.DarkestDungeon.Core.Combat.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.AI
{
    /// <summary>Skill selection desire that picks a capture skill to fill an empty captor slot.</summary>
    public sealed class SkillSelectionFillEmptyCaptor : SkillSelectionDesire
    {
        private bool CanTargetDeathsDoor { get; set; }
        private bool CanTargetLastHero { get; set; }
        private bool FirstInitiativeOnly { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SkillSelectionFillEmptyCaptor"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public SkillSelectionFillEmptyCaptor(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        protected override bool IsRestricted(ICombatUnit performer, IBattleContext battleContext)
        {
            if (base.IsRestricted(performer, battleContext))
                return true;

            if (FirstInitiativeOnly && performer.CombatInfo.CurrentInitiative != 1)
                return true;

            if (!CanTargetDeathsDoor && battleContext.BattleGround.NonDeathsDoorHeroes == 0)
                return true;

            if (!CanTargetLastHero && battleContext.BattleGround.HeroNumber == 1)
                return true;

            if (performer.Party.Units.All(unit => unit.Character.EmptyCaptor == null))
                return true;

            return false;
        }

        /// <inheritdoc/>
        protected override bool IsValidSkill(ICombatUnit performer, CombatSkill skill, IBattleContext battleContext)
        {
            if (!base.IsValidSkill(performer, skill, battleContext))
                return false;

            return skill.Effects.Any(effect => effect.SubEffects.Any(IsValidSubEffect));
        }

        /// <inheritdoc/>
        protected override bool IsValidTarget(ICombatUnit target)
        {
            if (!CanTargetDeathsDoor && target.Character.AtDeathsDoor)
                return false;

            return true;
        }

        /// <inheritdoc/>
        protected override bool IsValidTargetDesire(TargetSelectionDesire desire)
        {
            return desire.Type == TargetDesireType.FillEmptyCaptor;
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "can_target_deaths_door":
                        CanTargetDeathsDoor = (bool)dataSet["can_target_deaths_door"];
                        break;
                    case "can_target_last_hero":
                        CanTargetLastHero = (bool)dataSet["can_target_last_hero"];
                        break;
                    case "first_initiative_only":
                        FirstInitiativeOnly = (bool)dataSet[token.Key];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }

        private bool IsValidSubEffect(SubEffect subEffect)
        {
            return subEffect.Type == EffectSubType.Capture;
        }
    }
}
