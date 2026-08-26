using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Combat context evaluated by buff rules (replaces the Unity RaidRuleInfo).</summary>
    public class BattleRulesContext
    {
        /// <summary>Gets the unit the buffs apply to.</summary>
        public ICombatUnit Unit { get; }

        /// <summary>Gets the target unit of the current skill.</summary>
        public ICombatUnit Target { get; }

        /// <summary>Gets the battlefield.</summary>
        public IBattleGround BattleGround { get; }

        /// <summary>Gets the skill being used (may be null).</summary>
        public CombatSkill Skill { get; }

        /// <summary>Gets the current torch amount.</summary>
        public int TorchAmount { get; }

        /// <summary>Gets a value indicating whether the party is camping.</summary>
        public bool IsDoingCamping { get; }

        /// <summary>Gets a value indicating whether the unit is riposting.</summary>
        public bool IsRiposting { get; }

        /// <summary>Gets the dungeon region identifier (may be null).</summary>
        public string Dungeon { get; }

        /// <summary>Gets a value indicating whether the raid is in a corridor (hall).</summary>
        public bool IsInHall { get; }

        /// <summary>Initializes a new instance of the <see cref="BattleRulesContext"/> class.</summary>
        /// <param name="unit">The unit the buffs apply to.</param>
        /// <param name="target">The target unit of the skill.</param>
        /// <param name="battleGround">The battlefield.</param>
        /// <param name="skill">The skill being used.</param>
        /// <param name="torchAmount">The current torch amount.</param>
        /// <param name="isDoingCamping">Whether the party is camping.</param>
        /// <param name="isRiposting">Whether the unit is riposting.</param>
        /// <param name="dungeon">The dungeon region identifier.</param>
        /// <param name="isInHall">Whether the raid is in a corridor.</param>
        public BattleRulesContext(
            ICombatUnit unit,
            ICombatUnit target,
            IBattleGround battleGround,
            CombatSkill skill,
            int torchAmount,
            bool isDoingCamping,
            bool isRiposting,
            string dungeon,
            bool isInHall)
        {
            Unit = unit;
            Target = target;
            BattleGround = battleGround;
            Skill = skill;
            TorchAmount = torchAmount;
            IsDoingCamping = isDoingCamping;
            IsRiposting = isRiposting;
            Dungeon = dungeon;
            IsInHall = isInHall;
        }
    }
}