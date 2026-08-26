using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>Abstraction of the battle context providing computed properties.</summary>
    public interface IBattleContext
    {
        /// <summary>Gets the battlefield.</summary>
        IBattleGround BattleGround { get; }

        /// <summary>Gets the battle events service for effect feedback.</summary>
        IBattleEvents Events { get; }

        /// <summary>Gets the number of alive monsters (excluding corpses).</summary>
        int MonsterNumber { get; }

        /// <summary>Gets the number of heroes.</summary>
        int HeroNumber { get; }

        /// <summary>Gets the number of marked heroes.</summary>
        int MarkedHeroes { get; }

        /// <summary>Gets the number of afflicted heroes.</summary>
        int AfflictedHeroes { get; }

        /// <summary>Gets the number of virtued heroes.</summary>
        int VirtuedHeroes { get; }

        /// <summary>Gets the number of heroes at death's door.</summary>
        int DeathsDoorHeroes { get; }

        /// <summary>Gets the current torch light level.</summary>
        int TorchMeter { get; }

        /// <summary>Gets the current round number.</summary>
        int RoundNumber { get; }

        /// <summary>Gets the camping time left (for camping skills).</summary>
        int CampingTimeLeft { get; }

        /// <summary>Gets the list of alive hero units.</summary>
        IReadOnlyList<ICombatUnit> AliveHeroes { get; }

        /// <summary>Gets the list of alive monster units (excluding corpses).</summary>
        IReadOnlyList<ICombatUnit> AliveMonsters { get; }

        /// <summary>Gets the list of all hero units.</summary>
        IReadOnlyList<ICombatUnit> AllHeroes { get; }

        /// <summary>Gets the list of all monster units.</summary>
        IReadOnlyList<ICombatUnit> AllMonsters { get; }

        /// <summary>Resolves the available targets for a skill.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="skill">The skill being used.</param>
        /// <returns>The list of available target units.</returns>
        List<ICombatUnit> GetSkillAvailableTargets(ICombatUnit performer, CombatSkill skill);

        /// <summary>Checks whether a skill is usable by the given unit.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="skill">The skill to check.</param>
        /// <returns>True if the skill is usable.</returns>
        bool IsSkillUsable(ICombatUnit performer, CombatSkill skill);

        /// <summary>Applies the combat unit buff rules for a skill.</summary>
        /// <param name="unit">The unit receiving the buffs.</param>
        /// <param name="other">The opposing unit.</param>
        /// <param name="skill">The skill being used.</param>
        /// <param name="isRiposte">Whether the skill is a riposte skill.</param>
        void ApplyCombatUnitRules(ICombatUnit unit, ICombatUnit other, CombatSkill skill, bool isRiposte);

        /// <summary>Applies the idle unit buff rules.</summary>
        /// <param name="unit">The unit.</param>
        void ApplyIdleUnitRules(ICombatUnit unit);

        /// <summary>Applies an effect by its identifier to a unit.</summary>
        /// <param name="effectId">The effect identifier.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="independent">Whether the effect applies independently.</param>
        void ApplyEffectById(string effectId, ICombatUnit target, bool independent);
    }
}
