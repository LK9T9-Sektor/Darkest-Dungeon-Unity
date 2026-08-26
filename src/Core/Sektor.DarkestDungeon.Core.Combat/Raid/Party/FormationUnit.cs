using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

namespace Sektor.DarkestDungeon.Core.Combat.Raid.Party
{
    /// <summary>A combat unit standing in a formation.</summary>
    public class FormationUnit : ICombatUnit
    {
        /// <inheritdoc/>
        public int Rank { get; set; }

        /// <inheritdoc/>
        public Team Team { get; set; }

        /// <inheritdoc/>
        public ICharacter Character { get; set; }

        /// <inheritdoc/>
        public IFormationUnitInfo CombatInfo { get; }

        /// <inheritdoc/>
        public IFormationParty Party { get; set; }

        /// <inheritdoc/>
        public int Size { get { return Character.Size; } }

        /// <inheritdoc/>
        public bool IsCorpse
        {
            get { return Character.IsMonster && Character.MonsterTypes != null && Character.MonsterTypes.Contains(MonsterType.Corpse); }
        }

        /// <inheritdoc/>
        public bool IsTargetable { get; set; }

        /// <inheritdoc/>
        public List<IEffectEvent> EventQueue { get; }

        /// <summary>Initializes a new instance of the <see cref="FormationUnit"/> class.</summary>
        /// <param name="character">The character.</param>
        /// <param name="team">The team.</param>
        public FormationUnit(ICharacter character, Team team)
        {
            Character = character;
            Team = team;
            CombatInfo = new FormationUnitInfo();
            EventQueue = new List<IEffectEvent>();
            IsTargetable = true;
        }

        /// <summary>Prepares the unit for battle with the given combat id.</summary>
        /// <param name="combatId">The combat id.</param>
        public void PrepareForBattle(int combatId)
        {
            ((FormationUnitInfo)CombatInfo).PrepareForBattle(combatId);
            EventQueue.Clear();
        }
    }
}