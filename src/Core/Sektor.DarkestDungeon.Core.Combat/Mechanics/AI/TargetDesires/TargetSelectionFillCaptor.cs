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

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Target selection desire that targets enemies for capture effects.</summary>
    public sealed class TargetSelectionFillCaptor : TargetSelectionDesire
    {
        /// <summary>Initializes a new instance of the <see cref="TargetSelectionFillCaptor"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public TargetSelectionFillCaptor(Dictionary<string, object> dataSet)
        {
            Type = TargetDesireType.FillEmptyCaptor;

            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        public override bool SelectTarget(ICombatUnit performer, MonsterBrainDecision decision)
        {
            if (decision.SelectedSkill.Effects.All(effect => !effect.SubEffects.Any(subeffect => subeffect.Type == EffectSubType.Capture)))
                return false;

            return base.SelectTarget(performer, decision);
        }

        /// <inheritdoc/>
        protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
            {
                switch (token.Key)
                {
                    case "can_target_deaths_door":
                        Parameters[TargetSelectParameter.CanTargetDeathsDoor] = (bool)token.Value;
                        break;
                    case "can_target_last_hero":
                        Parameters[TargetSelectParameter.CanTargetLastHero] = (bool)token.Value;
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
