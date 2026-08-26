using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Battle;
using Sektor.DarkestDungeon.Core.Combat.Enums;

namespace Sektor.DarkestDungeon.Core.Combat.Interfaces
{
    /// <summary>Abstraction of the battlefield state.</summary>
    public interface IBattleGround
    {
        /// <summary>Gets the hero party.</summary>
        IFormationParty HeroParty { get; }

        /// <summary>Gets the monster party.</summary>
        IFormationParty MonsterParty { get; }

        /// <summary>Gets the current round.</summary>
        Round Round { get; }

        /// <summary>Gets or sets the last skill used.</summary>
        string LastSkillUsed { get; set; }

        /// <summary>Gets or sets the stalling round number.</summary>
        int StallingRoundNumber { get; set; }

        /// <summary>Gets or sets the battle status.</summary>
        BattleStatus BattleStatus { get; set; }

        /// <summary>Gets the surprise status.</summary>
        SurpriseStatus SurpriseStatus { get; }

        /// <summary>Gets the number of heroes.</summary>
        int HeroNumber { get; }

        /// <summary>Gets the number of marked heroes.</summary>
        int MarkedHeroes { get; }

        /// <summary>Gets the number of virtued heroes.</summary>
        int VirtuedHeroes { get; }

        /// <summary>Gets the number of non-virtued heroes.</summary>
        int NonVirtuedHeroes { get; }

        /// <summary>Gets the number of non-deaths-door heroes.</summary>
        int NonDeathsDoorHeroes { get; }

        /// <summary>Gets the number of living monsters (excluding corpses).</summary>
        int MonsterNumber { get; }

        /// <summary>Gets the number of guarded monsters.</summary>
        int GuardedMonsters { get; }

        /// <summary>Gets the total size of living monsters.</summary>
        int MonsterSize { get; }

        /// <summary>Gets the control count.</summary>
        int ControlCount { get; }

        /// <summary>Gets the list of combat IDs.</summary>
        List<int> CombatIds { get; }

        /// <summary>Gets the list of last damaged unit IDs.</summary>
        List<string> LastDamaged { get; }

        /// <summary>Checks if the battle has ended.</summary>
        bool IsBattleEnded();

        /// <summary>Checks if the battle is one-sided.</summary>
        bool IsBattleOnesided();
    }
}
