using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>Random target selection desire for duels, mirroring Darkest Dungeon's "random_target".</summary>
    public class DuelTargetSelectionRandom : TargetSelectionDesire
    {
        /// <summary>Initializes a new instance of the <see cref="DuelTargetSelectionRandom"/> class.</summary>
        /// <param name="chance">The proportional selection chance.</param>
        public DuelTargetSelectionRandom(int chance)
        {
            Type = TargetDesireType.Random;
            Chance = chance;
            GenerateFromDataSet(new Dictionary<string, object>
            {
                { "specific_combat_skill_id", string.Empty },
                { "is_enemy_target_desire", true },
                { "is_friendly_target_desire", false },
            });
        }
    }
}