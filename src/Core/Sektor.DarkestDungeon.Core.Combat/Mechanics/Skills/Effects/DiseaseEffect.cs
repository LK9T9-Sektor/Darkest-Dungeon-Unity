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

        private readonly string diseaseId;

        /// <summary>Gets the disease quirk id, or null when a random disease is applied.</summary>
        public string DiseaseId { get { return diseaseId; } }

        /// <summary>Initializes a new instance of the <see cref="DiseaseEffect"/> class.</summary>
        /// <param name="diseaseId">The disease quirk id to apply, or null for a random disease.</param>
        public DiseaseEffect(string diseaseId)
        {
            this.diseaseId = diseaseId;
        }

        /// <inheritdoc/>
        public override bool ApplyInstant(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Character.IsMonster)
                return false;

            IQuirk disease;
            if (!TryResolveDisease(target, effect, battleContext, out disease))
                return false;

            if (!target.Character.AddQuirk(disease))
                return false;

            return true;
        }

        /// <inheritdoc/>
        public override bool ApplyQueued(ICombatUnit performer, ICombatUnit target, Effect effect, IBattleContext battleContext)
        {
            if (target == null || target.Character.IsMonster)
                return false;

            if (!RollDiseaseChance(target))
            {
                battleContext.Events.ShowPopup(target, PopupType.DiseaseResist);
                return false;
            }

            IQuirk disease;
            if (!TryResolveDisease(target, effect, battleContext, out disease))
                return false;

            if (!target.Character.AddQuirk(disease))
                return false;

            battleContext.Events.SetHalo(target, "disease");
            battleContext.Events.ShowPopup(target, PopupType.Disease, disease.Id);
            return true;
        }

        private bool TryResolveDisease(ICombatUnit target, Effect effect, IBattleContext battleContext, out IQuirk disease)
        {
            disease = null;

            float diseaseTriggerChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;
            if (!RandomSolver.CheckSuccess(diseaseTriggerChance))
                return false;

            if (!RollDiseaseChance(target))
                return false;

            disease = ResolveDisease(target, battleContext);
            return disease != null;
        }

        private bool RollDiseaseChance(ICombatUnit target)
        {
            float diseaseChance = 1 - target.Character.GetSingleAttribute(AttributeType.Disease).ModifiedValue;
            return RandomSolver.CheckSuccess(diseaseChance);
        }

        private IQuirk ResolveDisease(ICombatUnit target, IBattleContext battleContext)
        {
            if (diseaseId != null)
                return battleContext.GetQuirk(diseaseId);

            // The random disease pool is not exposed by the core yet; until then a random disease is
            // resolved through the character (returns null while no pool is wired).
            return target.Character.AddRandomDisease();
        }
    }
}