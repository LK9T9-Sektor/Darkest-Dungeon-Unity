using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;

namespace Sektor.DarkestDungeon.Wpf.Combat
{
    /// <summary>A hero pick: class id + deterministic seed.</summary>
    public class DuelHeroPick
    {
        /// <summary>Gets the class id.</summary>
        public string ClassId { get; }

        /// <summary>Gets the deterministic seed.</summary>
        public int Seed { get; }

        /// <summary>Initializes a new instance of the <see cref="DuelHeroPick"/> class.</summary>
        /// <param name="classId">The class id.</param>
        /// <param name="seed">The seed.</param>
        public DuelHeroPick(string classId, int seed)
        {
            ClassId = classId;
            Seed = seed;
        }
    }

    /// <summary>Orchestrates a duel: builds parties, runs the core <see cref="BattleSolver"/>, advances turns.</summary>
    public class DuelController
    {
        /// <summary>Gets the hero party (local side).</summary>
        public FormationParty HeroParty { get; private set; } = new FormationParty();

        /// <summary>Gets the monster party (rival side).</summary>
        public FormationParty MonsterParty { get; private set; } = new FormationParty();

        /// <summary>Gets the battlefield.</summary>
        public BattleGround? BattleGround { get; private set; }

        /// <summary>Gets the battle context.</summary>
        public DuelBattleContext? Context { get; private set; }

        /// <summary>Gets the battle solver.</summary>
        public BattleSolver? Solver { get; private set; }

        /// <summary>Gets the event sink.</summary>
        public DuelBattleEvents Events { get; } = new DuelBattleEvents();

        /// <summary>Gets a value indicating whether the duel has been started.</summary>
        public bool IsStarted { get { return BattleGround != null; } }

        /// <summary>Gets a value indicating whether the duel has finished.</summary>
        public bool IsFinished { get { return BattleGround != null && BattleGround.IsBattleEnded(); } }

        /// <summary>Starts the duel with the given parties and session seed.</summary>
        /// <param name="localPicks">The local hero picks.</param>
        /// <param name="rivalPicks">The rival hero picks.</param>
        /// <param name="sessionSeed">The deterministic session seed.</param>
        public void StartDuel(IReadOnlyList<DuelHeroPick> localPicks, IReadOnlyList<DuelHeroPick> rivalPicks, int sessionSeed)
        {
            HeroParty = new FormationParty();
            MonsterParty = new FormationParty();

            int combatId = 1;
            foreach (var pick in localPicks)
            {
                var heroClass = DuelClasses.Get(pick.ClassId);
                if (heroClass == null)
                    continue;
                var hero = HeroGeneration.GenerateHero(heroClass, pick.Seed);
                var unit = new FormationUnit(hero, Team.Heroes);
                unit.PrepareForBattle(combatId++);
                HeroParty.AddUnit(unit);
            }

            foreach (var pick in rivalPicks)
            {
                var heroClass = DuelClasses.Get(pick.ClassId);
                if (heroClass == null)
                    continue;
                var hero = HeroGeneration.GenerateHero(heroClass, pick.Seed);
                var unit = new FormationUnit(hero, Team.Monsters);
                unit.PrepareForBattle(combatId++);
                MonsterParty.AddUnit(unit);
            }

            BattleGround = new BattleGround(HeroParty, MonsterParty);
            Context = new DuelBattleContext(BattleGround, Events);
            Solver = new BattleSolver(Context);

            RandomSolver.SetRandomSeed(sessionSeed);
            BattleGround.BattleStatus = BattleStatus.Fighting;
        }

        /// <summary>Gets the available targets of a skill for a unit.</summary>
        /// <param name="unit">The acting unit.</param>
        /// <param name="skill">The skill.</param>
        /// <returns>The target units.</returns>
        public List<ICombatUnit> GetAvailableTargets(ICombatUnit unit, CombatSkill skill)
        {
            return Context != null ? Context.GetSkillAvailableTargets(unit, skill) : new List<ICombatUnit>();
        }

        /// <summary>Checks whether a skill is usable by a unit.</summary>
        /// <param name="unit">The acting unit.</param>
        /// <param name="skill">The skill.</param>
        /// <returns>True if usable.</returns>
        public bool IsSkillUsable(ICombatUnit unit, CombatSkill skill)
        {
            return Context != null && Context.IsSkillUsable(unit, skill);
        }

        /// <summary>Executes a skill against a target and advances the round.</summary>
        /// <param name="unit">The acting unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="skill">The skill.</param>
        /// <returns>True if the skill was executed.</returns>
        public bool ExecuteSkill(ICombatUnit unit, ICombatUnit target, CombatSkill skill)
        {
            if (Solver == null || BattleGround == null)
                return false;

            Solver.SkillResult.Reset();
            Solver.ExecuteSkill(unit, target, skill, null);

            AdvanceRound();
            return true;
        }

        private void AdvanceRound()
        {
            if (BattleGround == null)
                return;

            BattleGround.Round.RoundNumber++;
            foreach (var unit in HeroParty.Units)
            {
                ((Character)unit.Character).UpdateRound();
                ((FormationUnitInfo)unit.CombatInfo).UpdateNextTurn();
            }
            foreach (var unit in MonsterParty.Units)
            {
                ((Character)unit.Character).UpdateRound();
                ((FormationUnitInfo)unit.CombatInfo).UpdateNextTurn();
            }
        }
    }
}