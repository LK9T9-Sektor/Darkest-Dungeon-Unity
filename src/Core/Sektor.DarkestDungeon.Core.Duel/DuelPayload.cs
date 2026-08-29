namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>Wire protocol of duel inputs exchanged between the two sides (lockstep).</summary>
    public static class DuelPayload
    {
        /// <summary>The pass action keyword.</summary>
        public const string Pass = "pass";

        /// <summary>The move action keyword.</summary>
        public const string Move = "move";

        private const string Separator = "|";

        /// <summary>Encodes a skill cast against a target combat id ("skillId|targetId").</summary>
        /// <param name="skillId">The skill id.</param>
        /// <param name="targetId">The target combat id.</param>
        /// <returns>The payload.</returns>
        public static string Skill(string skillId, int targetId)
        {
            return skillId + Separator + targetId;
        }

        /// <summary>Encodes a rank move ("move|rank").</summary>
        /// <param name="rank">The destination rank.</param>
        /// <returns>The payload.</returns>
        public static string MoveAction(int rank)
        {
            return Move + Separator + rank;
        }

        /// <summary>Encodes a pass ("pass|0").</summary>
        /// <returns>The payload.</returns>
        public static string PassAction()
        {
            return Pass + Separator + "0";
        }
    }
}