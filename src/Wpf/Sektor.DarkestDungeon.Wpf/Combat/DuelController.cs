using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Orchestrates a duel over a deterministic lockstep simulation. Both sides build identical
    /// formations (Heroes = host party, Monsters = client party); the host inputs for Heroes,
    /// the client inputs for Monsters. Turn order follows Unity's speed-based initiative.
    /// </summary>
    public class DuelController
    {
        /// <summary>Gets the hero party (host's party).</summary>
        public FormationParty HeroParty { get; private set; } = new FormationParty();

        /// <summary>Gets the monster party (client's party).</summary>
        public FormationParty MonsterParty { get; private set; } = new FormationParty();

        /// <summary>Gets the battlefield.</summary>
        public BattleGround? BattleGround { get; private set; }

        /// <summary>Gets the battle context.</summary>
        public DuelBattleContext? Context { get; private set; }

        /// <summary>Gets the battle solver.</summary>
        public BattleSolver? Solver { get; private set; }

        /// <summary>Gets the event sink.</summary>
        public DuelBattleEvents Events { get; } = new DuelBattleEvents();

        /// <summary>Gets a value indicating whether this side is the host.</summary>
        public bool IsHost { get; private set; }

        /// <summary>Gets the current turn flow phase.</summary>
        public DuelPhase Phase { get; private set; } = DuelPhase.NotStarted;

        /// <summary>Gets a value indicating whether it is the local player's turn to act.</summary>
        public bool IsLocalTurn
        {
            get
            {
                return (IsHost && Phase == DuelPhase.WaitingForHostAction)
                    || (!IsHost && Phase == DuelPhase.WaitingForClientAction);
            }
        }

        /// <summary>Gets a value indicating whether the duel has been started.</summary>
        public bool IsStarted { get { return BattleGround != null; } }

        /// <summary>Gets a value indicating whether the duel has finished.</summary>
        public bool IsFinished { get { return BattleGround != null && BattleGround.IsBattleEnded(); } }

        /// <summary>Gets the acting unit of the current turn.</summary>
        public ICombatUnit? CurrentUnit
        {
            get
            {
                if (BattleGround == null)
                    return null;
                if (BattleGround.Round.SelectedUnit != null)
                    return BattleGround.Round.SelectedUnit;
                if (BattleGround.Round.OrderedUnits.Count == 0)
                    return null;
                return BattleGround.Round.OrderedUnits[0];
            }
        }

        /// <summary>Starts the duel with identical formations on both sides.</summary>
        /// <param name="hostPicks">The host party picks (Heroes team).</param>
        /// <param name="clientPicks">The client party picks (Monsters team).</param>
        /// <param name="sessionSeed">The deterministic session seed.</param>
        /// <param name="isHost">Whether this side is the host.</param>
        public void StartDuel(IReadOnlyList<DuelHeroPick> hostPicks, IReadOnlyList<DuelHeroPick> clientPicks, int sessionSeed, bool isHost)
        {
            IsHost = isHost;
            HeroParty = new FormationParty();
            MonsterParty = new FormationParty();

            int combatId = 1;
            foreach (var pick in hostPicks)
                AddHero(pick, Team.Heroes, ref combatId);
            foreach (var pick in clientPicks)
                AddHero(pick, Team.Monsters, ref combatId);

            BattleGround = new BattleGround(HeroParty, MonsterParty);
            Context = new DuelBattleContext(BattleGround, Events);
            Solver = new BattleSolver(Context);

            RandomSolver.SetRandomSeed(sessionSeed);
            BattleGround.BattleStatus = BattleStatus.Fighting;
            Phase = DuelPhase.NotStarted;
        }

        /// <summary>Starts the battle: computes the first round order and begins the first turn.</summary>
        public void StartBattle()
        {
            if (BattleGround == null)
                return;

            BattleGround.Round.StartBattle(BattleGround);
            BeginTurn();
        }

        /// <summary>Begins the current turn.</summary>
        public void BeginTurn()
        {
            if (BattleGround == null || Solver == null)
                return;

            if (BattleGround.IsBattleEnded())
            {
                Phase = DuelPhase.Finished;
                return;
            }

            var current = CurrentUnit;
            if (current == null)
            {
                NextRound();
                return;
            }

            if (current.Team == Team.Heroes)
            {
                BattleGround.Round.PreHeroTurn(current, BattleGround);
                Phase = DuelPhase.WaitingForHostAction;
            }
            else
            {
                BattleGround.Round.PreMonsterTurn(current, BattleGround);
                Phase = DuelPhase.WaitingForClientAction;
            }
        }

        /// <summary>Executes the acting unit's skill and returns the wire payload to broadcast.</summary>
        /// <param name="skillId">The skill id.</param>
        /// <param name="targetId">The target combat id.</param>
        /// <returns>The wire payload, or null if invalid or not the local turn.</returns>
        public string? ExecuteLocalSkill(string skillId, int targetId)
        {
            if (!IsLocalTurn || BattleGround == null || Solver == null)
                return null;

            var unit = CurrentUnit;
            var target = GetUnitByCombatId(targetId);
            var skill = FindSkill(unit, skillId);
            if (unit == null || target == null || skill == null || !IsSkillUsable(unit, skill))
                return null;

            ExecuteSkill(unit, target, skill);
            CompleteTurn();

            return skillId + "|" + targetId;
        }

        /// <summary>Applies a remote action payload ("skillId|targetId").</summary>
        /// <param name="payload">The payload.</param>
        public void ApplyRemoteSkill(string payload)
        {
            if (IsLocalTurn || BattleGround == null || Solver == null)
                return;

            var parts = payload.Split('|');
            if (parts.Length != 2)
                return;

            var unit = CurrentUnit;
            var target = GetUnitByCombatId(int.Parse(parts[1]));
            var skill = FindSkill(unit, parts[0]);
            if (unit == null || target == null || skill == null)
                return;

            ExecuteSkill(unit, target, skill);
            CompleteTurn();
        }

        /// <summary>Completes the current turn and advances to the next unit or round.</summary>
        public void CompleteTurn()
        {
            if (BattleGround == null)
                return;

            if (BattleGround.Round.SelectedUnit != null && BattleGround.Round.SelectedUnit.Team == Team.Heroes)
                BattleGround.Round.PostHeroTurn();
            else
                BattleGround.Round.PostMonsterTurn();

            if (BattleGround.IsBattleEnded())
            {
                Phase = DuelPhase.Finished;
                return;
            }

            if (BattleGround.Round.OrderedUnits.Count == 0)
                NextRound();
            else
                BeginTurn();
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

        /// <summary>Looks up a unit by its combat id in both parties.</summary>
        /// <param name="combatId">The combat id.</param>
        /// <returns>The unit or null.</returns>
        public ICombatUnit? GetUnitByCombatId(int combatId)
        {
            return HeroParty.Units.FirstOrDefault(u => u.CombatInfo.CombatId == combatId)
                ?? MonsterParty.Units.FirstOrDefault(u => u.CombatInfo.CombatId == combatId);
        }

        /// <summary>Executes a skill against a target (core solver).</summary>
        /// <param name="unit">The acting unit.</param>
        /// <param name="target">The target unit.</param>
        /// <param name="skill">The skill.</param>
        public void ExecuteSkill(ICombatUnit unit, ICombatUnit target, CombatSkill skill)
        {
            if (Solver == null || BattleGround == null)
                return;

            Solver.SkillResult.Reset();
            Solver.ExecuteSkill(unit, target, skill, null);
            CheckDeaths();
        }

        private void AddHero(DuelHeroPick pick, Team team, ref int combatId)
        {
            var heroClass = DuelClasses.Get(pick.ClassId);
            if (heroClass == null)
                return;
            var hero = HeroGeneration.GenerateHero(heroClass, pick.Seed);
            var unit = new FormationUnit(hero, team);
            unit.PrepareForBattle(combatId++);
            if (team == Team.Heroes)
                HeroParty.AddUnit(unit);
            else
                MonsterParty.AddUnit(unit);
        }

        private void NextRound()
        {
            if (BattleGround == null)
                return;

            foreach (var unit in HeroParty.Units)
                ((Character)unit.Character).UpdateRound();
            foreach (var unit in MonsterParty.Units)
                ((Character)unit.Character).UpdateRound();

            BattleGround.Round.NextRound(BattleGround);
            BeginTurn();
        }

        private void CheckDeaths()
        {
            foreach (var unit in HeroParty.Units.Concat(MonsterParty.Units))
            {
                if (unit.Character.HealthRatio <= 0)
                    ((FormationUnitInfo)unit.CombatInfo).IsDead = true;
            }
        }

        private static CombatSkill? FindSkill(ICombatUnit? unit, string skillId)
        {
            if (unit == null || unit.Character.CurrentCombatSkills == null)
                return null;
            return unit.Character.CurrentCombatSkills.FirstOrDefault(skill => skill.Id == skillId);
        }
    }
}