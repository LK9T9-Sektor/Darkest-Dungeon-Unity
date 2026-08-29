using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects
{
    /// <summary>Inflicts a disease/quirk on a hero.</summary>
    public class DiseaseEffect : SubEffect
    {
        /// <inheritdoc/>
        public override EffectSubType Type { get { return EffectSubType.Disease; } }

        private bool IsRandom { get; set; }
        private IQuirk Disease { get; set; }

        /// <summary>Initializes a new instance of the <see cref="DiseaseEffect"/> class.</summary>
        /// <param name="disease">The disease to apply.</param>
        /// <param name="isRandom">Whether a random disease is applied.</param>
        public DiseaseEffect(IQuirk disease, bool isRandom)
        {
            Disease = disease;
            IsRandom = isRandom;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Character.IsMonster)
                return false;

            float diseaseTriggerChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;
            if (!RandomSolver.CheckSuccess(diseaseTriggerChance))
                return false;

            float diseaseChance = 1 - target.Character.GetSingleAttribute(AttributeType.Disease).ModifiedValue;

            if (RandomSolver.CheckSuccess(diseaseChance))
            {
                if (IsRandom == false && Disease != null)
                {
                    if (target.Character.AddQuirk(Disease))
                        return true;
                }
                else
                {
                    target.Character.AddRandomDisease();
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Character.IsMonster)
                return false;

            float diseaseTriggerChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;
            if (!RandomSolver.CheckSuccess(diseaseTriggerChance))
                return false;

            float diseaseChance = 1 - target.Character.GetSingleAttribute(AttributeType.Disease).ModifiedValue;

            if (RandomSolver.CheckSuccess(diseaseChance))
            {
                if (IsRandom == false && Disease != null)
                {
                    if (target.Character.AddQuirk(Disease))
                    {
                        battleContext.Events.SetHalo(target, "disease");
                        battleContext.Events.ShowPopup(target, PopupType.Disease, Disease.Id);
                        return true;
                    }
                    return false;
                }
                else
                {
                    var disease = target.Character.AddRandomDisease();
                    battleContext.Events.ShowPopup(target, PopupType.Disease, disease.Id);
                    battleContext.Events.SetHalo(target, "disease");
                    return true;
                }
            }
            else
            {
                battleContext.Events.ShowPopup(target, PopupType.DiseaseResist);
                return false;
            }
        }
    }
}