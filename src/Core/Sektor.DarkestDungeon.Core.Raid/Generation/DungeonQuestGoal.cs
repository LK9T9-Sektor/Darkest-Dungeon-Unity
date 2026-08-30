using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>
    /// Primitive quest-goal description used by dungeon generation to place goal rooms (boss or
    /// curio) on the map. Carries only the data the generator needs — no campaign models.
    /// </summary>
    public class DungeonQuestGoal
    {
        /// <summary>Gets or sets the goal type ("kill_monster", "activate", "gather").</summary>
        public string Type { get; set; }

        /// <summary>Gets the monster ids of a "kill_monster" goal (the boss is chosen from the matching encounter).</summary>
        public List<string> MonsterNameIds { get; }

        /// <summary>Gets or sets the curio name of an "activate"/"gather" goal.</summary>
        public string CurioName { get; set; }

        /// <summary>Gets or sets the number of curio rooms of an "activate"/"gather" goal.</summary>
        public int Amount { get; set; }

        /// <summary>Gets or sets the item id of a "gather" goal.</summary>
        public string ItemId { get; set; }

        /// <summary>Gets or sets the item amount of a "gather" goal.</summary>
        public int ItemAmount { get; set; }

        /// <summary>Initializes a new instance of the <see cref="DungeonQuestGoal"/> class.</summary>
        public DungeonQuestGoal()
        {
            MonsterNameIds = new List<string>();
        }
    }
}