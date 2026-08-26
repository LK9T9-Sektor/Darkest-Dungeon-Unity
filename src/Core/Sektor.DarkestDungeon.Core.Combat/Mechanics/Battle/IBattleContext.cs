using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>Abstraction of the battle context providing computed properties.</summary>
    public interface IBattleContext
    {
        /// <summary>Gets the battlefield.</summary>
        IBattleGround BattleGround { get; }

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
    }
}
