using System.Collections.Generic;
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
    /// <summary>Bonus initiative desire that grants a bonus when a specific ally was recently damaged.</summary>
    public sealed class BonusInitiativeAllyLastDamaged : BonusInitiativeDesire
    {
        private string AllyBaseClass { get; set; }
        private bool IgnoreIfStun { get; set; }

        /// <summary>Initializes a new instance of the <see cref="BonusInitiativeAllyLastDamaged"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public BonusInitiativeAllyLastDamaged(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        public override bool CheckBonusInitiative(ICombatUnit performer, IBattleContext battleContext)
        {
            if (IgnoreIfStun && performer.Character.GetStatusEffect(StatusType.Stun).IsApplied)
                return false;

            if (AllyBaseClass != null && battleContext.BattleGround.LastDamaged.Contains(AllyBaseClass))
            {
                battleContext.BattleGround.LastDamaged.Clear();
                return true;
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
                    case "ally_base_class_id":
                        AllyBaseClass = (string)dataSet["ally_base_class_id"];
                        break;
                    case "ignore_if_stun":
                        IgnoreIfStun = (bool)dataSet["ignore_if_stun"];
                        break;
                    default:
                        ProcessBaseDataToken(token);
                        break;
                }
            }
        }
    }
}
