using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Skills
{
    /// <summary>Move skill that changes unit position.</summary>
    public class MoveSkill : Skill
    {
        /// <summary>Gets or sets the forward move distance.</summary>
        public int MoveForward { get; set; }

        /// <summary>Gets or sets the backward move distance.</summary>
        public int MoveBackward { get; set; }

        /// <summary>Initializes a new instance of the <see cref="MoveSkill"/> class.</summary>
        /// <param name="id">The skill identifier.</param>
        /// <param name="backward">Backward move distance.</param>
        /// <param name="forward">Forward move distance.</param>
        public MoveSkill(string id, int backward, int forward)
        {
            Id = id;
            MoveBackward = backward;
            MoveForward = forward;
        }
    }
}
