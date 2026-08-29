using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Raid.Party
{
    /// <summary>
    /// Orders a formation's units for display on its side of the field. The front rank (1) always
    /// faces the enemy: the side anchored on the left (the hero party) reads back-to-front from the
    /// viewer's seat, the right side reads front-to-back. Mirrors the legacy Unity <c>FormationRanks</c>
    /// facing flag so the presentation cutover maps one-to-one.
    /// </summary>
    public sealed class FormationDisplayOrder
    {
        private readonly bool _facesRight;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormationDisplayOrder"/> class.
        /// </summary>
        /// <param name="facesRight">Whether this formation anchors on the left and faces the enemy (hero side).</param>
        public FormationDisplayOrder(bool facesRight)
        {
            this._facesRight = facesRight;
        }

        /// <summary>Gets the order for the hero side (left anchor, front facing the enemy on the right).</summary>
        /// <returns>The hero-side order rule.</returns>
        public static FormationDisplayOrder HeroSide()
        {
            return new FormationDisplayOrder(true);
        }

        /// <summary>Gets the order for the monster side (right anchor, front facing the enemy on the left).</summary>
        /// <returns>The monster-side order rule.</returns>
        public static FormationDisplayOrder MonsterSide()
        {
            return new FormationDisplayOrder(false);
        }

        /// <summary>
        /// Orders the party's units left-to-right on screen, from the rearmost rank toward the front
        /// for the hero side and from the front toward the rear for the monster side.
        /// </summary>
        /// <param name="party">The formation party.</param>
        /// <returns>The units in display order.</returns>
        public List<ICombatUnit> OrderLeftToRight(IFormationParty party)
        {
            var ordered = party.Units.OrderBy(unit => unit.Rank).ToList();
            if (_facesRight)
                ordered.Reverse();
            return ordered;
        }
    }
}