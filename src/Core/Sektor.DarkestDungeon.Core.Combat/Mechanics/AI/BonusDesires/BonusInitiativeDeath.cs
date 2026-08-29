using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Bonus initiative desire that grants a bonus when the performer is at zero health.</summary>
    public sealed class BonusInitiativeDeath : BonusInitiativeDesire
    {
        /// <summary>Initializes a new instance of the <see cref="BonusInitiativeDeath"/> class.</summary>
        /// <param name="dataSet">The data set to initialize from.</param>
        public BonusInitiativeDeath(Dictionary<string, object> dataSet)
        {
            GenerateFromDataSet(dataSet);
        }

        /// <inheritdoc/>
        public override bool CheckBonusInitiative(ICombatUnit performer, IBattleContext battleContext)
        {
            return performer.Character.HasZeroHealth;
        }
    }
}
