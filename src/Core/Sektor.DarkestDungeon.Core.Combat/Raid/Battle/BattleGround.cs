using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Core.Combat.Raid.Battle
{
    /// <summary>A duel battlefield with a hero and a monster party.</summary>
    public class BattleGround : IBattleGround
    {
        /// <inheritdoc/>
        public IFormationParty HeroParty { get; }

        /// <inheritdoc/>
        public IFormationParty MonsterParty { get; }

        /// <inheritdoc/>
        public Round Round { get; }

        /// <inheritdoc/>
        public string LastSkillUsed { get; set; }

        /// <inheritdoc/>
        public int StallingRoundNumber { get; set; }

        /// <inheritdoc/>
        public BattleStatus BattleStatus { get; set; }

        /// <inheritdoc/>
        public SurpriseStatus SurpriseStatus { get; private set; }

        /// <inheritdoc/>
        public int HeroNumber { get { return HeroParty.Units.Count; } }

        /// <inheritdoc/>
        public int MarkedHeroes
        {
            get { return HeroParty.Units.FindAll(unit => unit.Character.GetStatusEffect(StatusType.Marked).IsApplied).Count; }
        }

        /// <inheritdoc/>
        public int VirtuedHeroes
        {
            get { return HeroParty.Units.FindAll(unit => unit.Character.IsVirtued).Count; }
        }

        /// <inheritdoc/>
        public int NonVirtuedHeroes
        {
            get { return HeroParty.Units.FindAll(unit => !unit.Character.IsVirtued).Count; }
        }

        /// <inheritdoc/>
        public int NonDeathsDoorHeroes
        {
            get { return HeroParty.Units.FindAll(unit => !unit.Character.AtDeathsDoor).Count; }
        }

        /// <inheritdoc/>
        public int MonsterNumber
        {
            get { return MonsterParty.Units.FindAll(unit => unit.Character.IsMonster && !unit.IsCorpse).Count; }
        }

        /// <inheritdoc/>
        public int GuardedMonsters
        {
            get { return MonsterParty.Units.FindAll(unit => unit.Character.GetStatusEffect(StatusType.Guarded).IsApplied).Count; }
        }

        /// <inheritdoc/>
        public int MonsterSize
        {
            get { return MonsterParty.Units.FindAll(unit => unit.Character.IsMonster && !unit.IsCorpse).Sum(unit => unit.Size); }
        }

        /// <inheritdoc/>
        public int ControlCount { get { return Controls.Count; } }

        /// <inheritdoc/>
        public List<int> CombatIds { get; }

        /// <inheritdoc/>
        public List<string> LastDamaged { get; }

        /// <summary>Gets the control records.</summary>
        public List<object> Controls { get; }

        /// <summary>Initializes a new instance of the <see cref="BattleGround"/> class.</summary>
        /// <param name="heroParty">The hero party.</param>
        /// <param name="monsterParty">The monster party.</param>
        public BattleGround(FormationParty heroParty, FormationParty monsterParty)
        {
            HeroParty = heroParty;
            MonsterParty = monsterParty;
            Round = new Round();
            Controls = new List<object>();
            LastDamaged = new List<string>(4);
            CombatIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            SurpriseStatus = SurpriseStatus.Nothing;
        }

        /// <inheritdoc/>
        public bool IsBattleEnded()
        {
            if (Controls.Count != 0)
                return false;

            if (HeroFormationAliveCount() == 0 || MonsterFormationAliveCount() == 0 || BattleStatus == BattleStatus.Finished)
            {
                BattleStatus = BattleStatus.Finished;
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool IsBattleOnesided()
        {
            return HeroFormationAliveCount() == 0 || MonsterFormationAliveCount() == 0;
        }

        private int HeroFormationAliveCount()
        {
            return HeroParty.Units.FindAll(unit => !((FormationUnitInfo)unit.CombatInfo).IsDead).Count;
        }

        private int MonsterFormationAliveCount()
        {
            return MonsterParty.Units.FindAll(unit => !((FormationUnitInfo)unit.CombatInfo).IsDead).Count;
        }
    }
}