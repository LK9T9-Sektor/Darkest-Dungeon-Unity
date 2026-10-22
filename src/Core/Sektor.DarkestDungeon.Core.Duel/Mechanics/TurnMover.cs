using System;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Core.Duel.Mechanics
{
    /// <summary>
    /// Swaps a unit with an ally standing in an adjacent rank (manual move). Respects immobilization
    /// and formation bounds; re-assigns ranks after the swap.
    /// </summary>
    public class TurnMover
    {
        private readonly FormationParty heroParty;
        private readonly FormationParty monsterParty;

        /// <summary>Initializes a new instance of the <see cref="TurnMover"/> class.</summary>
        /// <param name="heroParty">The hero party.</param>
        /// <param name="monsterParty">The monster party.</param>
        public TurnMover(FormationParty heroParty, FormationParty monsterParty)
        {
            this.heroParty = heroParty;
            this.monsterParty = monsterParty;
        }

        /// <summary>Swaps a unit with the ally standing in an adjacent rank.</summary>
        /// <param name="unit">The moving unit.</param>
        /// <param name="newRank">The destination rank (must be adjacent).</param>
        /// <returns>True if the move was performed.</returns>
        public bool TryMove(ICombatUnit unit, int newRank)
        {
            if (!CanMove(unit, newRank))
                return false;

            var party = unit.Team == Team.Heroes ? heroParty : monsterParty;
            int fromIndex = party.Units.IndexOf(unit);
            int toIndex = party.Units.FindIndex(candidate => candidate.Rank == newRank);
            var swap = party.Units[fromIndex];
            party.Units[fromIndex] = party.Units[toIndex];
            party.Units[toIndex] = swap;
            party.RecalculateRanks();
            return true;
        }

        /// <summary>Checks whether a unit can swap into the destination rank without mutating the formation.</summary>
        /// <param name="unit">The moving unit.</param>
        /// <param name="newRank">The destination rank (must be adjacent).</param>
        /// <returns>True when the move is legal.</returns>
        public bool CanMove(ICombatUnit unit, int newRank)
        {
            if (unit == null || unit.CombatInfo.IsImmobilized)
                return false;

            var party = unit.Team == Team.Heroes ? heroParty : monsterParty;
            if (newRank < 1 || newRank > party.Units.Count || Math.Abs(newRank - unit.Rank) != 1)
                return false;

            int fromIndex = party.Units.IndexOf(unit);
            int toIndex = party.Units.FindIndex(candidate => candidate.Rank == newRank);
            return fromIndex >= 0 && toIndex >= 0;
        }
    }
}