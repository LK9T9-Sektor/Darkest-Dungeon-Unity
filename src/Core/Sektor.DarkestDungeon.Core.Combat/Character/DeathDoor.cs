using System.Collections.Generic;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>
    /// Death's door data of a hero class: the debuffs applied on entering death's door, the
    /// recovery (mortality) debuffs after being healed off death's door and the heart-attack
    /// recovery debuffs.
    /// </summary>
    public class DeathDoor
    {
        /// <summary>Gets the death's door debuff ids (applied while at death's door).</summary>
        public List<string> Buffs { get; }

        /// <summary>Gets the mortality recovery debuff ids (applied after healing off death's door).</summary>
        public List<string> RecoveryBuffs { get; }

        /// <summary>Gets the heart-attack recovery debuff ids.</summary>
        public List<string> HeartAttackBuffs { get; }

        /// <summary>Initializes a new instance of the <see cref="DeathDoor"/> class.</summary>
        /// <param name="buffs">The death's door debuff ids.</param>
        /// <param name="recoveryBuffs">The mortality recovery debuff ids.</param>
        /// <param name="heartAttackBuffs">The heart-attack recovery debuff ids.</param>
        public DeathDoor(IReadOnlyList<string> buffs = null, IReadOnlyList<string> recoveryBuffs = null, IReadOnlyList<string> heartAttackBuffs = null)
        {
            Buffs = buffs == null ? new List<string>() : new List<string>(buffs);
            RecoveryBuffs = recoveryBuffs == null ? new List<string>() : new List<string>(recoveryBuffs);
            HeartAttackBuffs = heartAttackBuffs == null ? new List<string>() : new List<string>(heartAttackBuffs);
        }
    }
}