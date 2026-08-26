using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
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

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>Effect container that applies sub-effects to targets.</summary>
    public class Effect
    {
        /// <summary>Gets or sets the effect name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the target type.</summary>
        public EffectTargetType TargetType { get; set; }

        /// <summary>Gets the boolean parameters.</summary>
        public Dictionary<EffectBoolParams, bool?> BooleanParams { get; }

        /// <summary>Gets the integer parameters.</summary>
        public Dictionary<EffectIntParams, int?> IntegerParams { get; }

        /// <summary>Gets the list of sub-effects.</summary>
        public List<SubEffect> SubEffects { get; }

        /// <summary>Initializes a new instance of the <see cref="Effect"/> class.</summary>
        public Effect()
        {
            SubEffects = new List<SubEffect>();
            BooleanParams = new Dictionary<EffectBoolParams, bool?>();
            IntegerParams = new Dictionary<EffectIntParams, int?>();

            foreach (EffectBoolParams effectBool in Enum.GetValues(typeof(EffectBoolParams)))
                BooleanParams.Add(effectBool, null);
            foreach (EffectIntParams effectInteger in Enum.GetValues(typeof(EffectIntParams)))
                IntegerParams.Add(effectInteger, null);
        }

        /// <summary>Applies the effect to the target.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="skillResult">The skill result.</param>
        /// <param name="torchHandler">Optional torch handler for global effects.</param>
        public void Apply(ICombatUnit performer, ICombatUnit target, SkillResult skillResult, ITorchHandler torchHandler = null)
        {
            if (BooleanParams[EffectBoolParams.ApplyOnce].HasValue)
                if (BooleanParams[EffectBoolParams.ApplyOnce].Value)
                    if (skillResult.AppliedEffects.Contains(this))
                        return;

            if (BooleanParams[EffectBoolParams.OnMiss] == false)
                if (skillResult.Current.Type == SkillResultType.Miss || skillResult.Current.Type == SkillResultType.Dodge)
                    return;

            if (BooleanParams[EffectBoolParams.CanApplyAfterDeath] == false)
                if (skillResult.Current.IsZeroed)
                    return;

            switch (TargetType)
            {
                case EffectTargetType.Performer:
                    foreach (var subEffect in SubEffects)
                        subEffect.Apply(performer, performer, this);
                    break;
                case EffectTargetType.Target:
                    foreach (var subEffect in SubEffects)
                        subEffect.Apply(performer, target, this);
                    break;
                case EffectTargetType.PerformersOther:
                    foreach (var unit in performer.Party.Units)
                    {
                        if (unit != performer)
                            foreach (var subEffect in SubEffects)
                                subEffect.Apply(performer, unit, this);
                    }
                    break;
                case EffectTargetType.TargetGroup:
                    foreach (var unit in target.Party.Units)
                    {
                        foreach (var subEffect in SubEffects)
                            subEffect.Apply(performer, unit, this);
                    }
                    break;
                case EffectTargetType.Global:
                    if (IntegerParams[EffectIntParams.Torch].HasValue && torchHandler != null)
                    {
                        if (IntegerParams[EffectIntParams.Torch] < 0)
                            torchHandler.DecreaseTorch(-IntegerParams[EffectIntParams.Torch].Value);
                        else
                            torchHandler.IncreaseTorch(IntegerParams[EffectIntParams.Torch].Value);
                    }
                    foreach (var subEffect in SubEffects)
                        subEffect.Apply(performer, target, this);
                    break;
            }

            skillResult.AppliedEffects.Add(this);
        }

        /// <summary>Applies target conditions.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        public void ApplyTargetConditions(ICombatUnit performer, ICombatUnit target)
        {
            switch (TargetType)
            {
                case EffectTargetType.Performer:
                    foreach (var subEffect in SubEffects)
                        subEffect.ApplyTargetConditions(performer, performer, target, this);
                    break;
                case EffectTargetType.Target:
                    foreach (var subEffect in SubEffects)
                        subEffect.ApplyTargetConditions(performer, target, target, this);
                    break;
                case EffectTargetType.PerformersOther:
                    foreach (var unit in performer.Party.Units)
                    {
                        if (unit != performer)
                            foreach (var subEffect in SubEffects)
                                subEffect.ApplyTargetConditions(performer, unit, unit, this);
                    }
                    break;
                case EffectTargetType.TargetGroup:
                    foreach (var unit in target.Party.Units)
                    {
                        foreach (var subEffect in SubEffects)
                            subEffect.ApplyTargetConditions(performer, unit, unit, this);
                    }
                    break;
                case EffectTargetType.Global:
                    foreach (var subEffect in SubEffects)
                        subEffect.ApplyTargetConditions(performer, target, performer, this);
                    break;
            }
        }

        /// <summary>Applies the effect independently.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="target">The target unit.</param>
        public void ApplyIndependent(ICombatUnit performer, ICombatUnit target)
        {
            SubEffects.ForEach(sub => sub.Apply(performer, target, this));
        }

        /// <summary>Gets the tooltip text for this effect.</summary>
        /// <returns>The tooltip string.</returns>
        public string Tooltip()
        {
            string toolTip = "";
            foreach (var subEffect in SubEffects)
            {
                string subTooltip = subEffect.Tooltip(this);
                if (subTooltip.Length > 0)
                {
                    if (toolTip.Length > 0)
                        toolTip += "\n" + subTooltip;
                    else
                        toolTip += subTooltip;
                }
            }
            return toolTip;
        }
    }
}
