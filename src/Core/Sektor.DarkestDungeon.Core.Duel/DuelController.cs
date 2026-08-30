using System;
using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Statuses;
using Sektor.DarkestDungeon.Core.Combat.Content;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;
using Sektor.DarkestDungeon.Core.Combat.Raid.Party;
using Sektor.DarkestDungeon.Core.Duel.Fight;
using Sektor.DarkestDungeon.Core.Duel.Mechanics;

namespace Sektor.DarkestDungeon.Core.Duel
{
    /// <summary>
    /// Orchestrates a duel over a deterministic lockstep simulation. Both sides build identical
    /// formations (Heroes = host party, Monsters = client party); the host inputs for Heroes,
    /// the client inputs for Monsters. Turn order follows Unity's speed-based initiative.
    /// </summary>
    public class DuelController
    {
        private readonly IDuelContent content;
        private readonly ILogger logger;
        private SurpriseResolver surpriseResolver;
        private DotTickApplier dotTickApplier;
        private StunRecoveryApplier stunRecoveryApplier;
        private DeathCheck deathCheck;
        private TurnMover turnMover;

        /// <summary>Gets the hero party (host's party).</summary>
        public FormationParty HeroParty { get; private set; } = new FormationParty();

        /// <summary>Gets the monster party (client's party).</summary>
        public FormationParty MonsterParty { get; private set; } = new FormationParty();

        /// <summary>Gets the battlefield.</summary>
        public BattleGround BattleGround { get; private set; }

        /// <summary>Gets the battle context.</summary>
        public DuelBattleContext Context { get; private set; }

        /// <summary>Gets the battle solver.</summary>
        public BattleSolver Solver { get; private set; }

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
        public ICombatUnit CurrentUnit
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

        /// <summary>Initializes a new instance of the <see cref="DuelController"/> class.</summary>
        /// <param name="content">The content source for building parties from picks.</param>
        /// <param name="logger">The logger (defaults to a no-op logger).</param>
        public DuelController(IDuelContent content, ILogger logger = null)
        {
            this.content = content;
            this.logger = logger ?? NullLogger.Instance;
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
            Context = new DuelBattleContext(BattleGround, Events, content);
            Solver = new BattleSolver(Context);
            InitializeMechanics();

            Events.TorchDelta = delta => Context.TorchAmount = Math.Max(0, Math.Min(100, Context.TorchAmount + delta));

            RandomSolver.SetRandomSeed(sessionSeed);
            BattleGround.BattleStatus = BattleStatus.Fighting;
            Phase = DuelPhase.NotStarted;
        }

        /// <summary>Starts a campaign fight: heroes against campaign monsters with campaign brains.</summary>
        /// <param name="playerSide">The hero side unit specifications.</param>
        /// <param name="aiSide">The monster side unit specifications.</param>
        /// <param name="sessionSeed">The deterministic session seed.</param>
        public void StartFight(IReadOnlyList<FightUnitSpec> playerSide, IReadOnlyList<FightUnitSpec> aiSide, int sessionSeed)
        {
            IsHost = false;
            HeroParty = new FormationParty();
            MonsterParty = new FormationParty();

            int combatId = 1;
            if (playerSide != null)
                foreach (var spec in playerSide)
                    AddPlayerUnit(spec, Team.Heroes, ref combatId);
            if (aiSide != null)
                foreach (var spec in aiSide)
                    AddAiUnit(spec, Team.Monsters, ref combatId);

            BattleGround = new BattleGround(HeroParty, MonsterParty);
            Context = new DuelBattleContext(BattleGround, Events, content);
            Solver = new BattleSolver(Context);
            InitializeMechanics();

            Events.TorchDelta = delta => Context.TorchAmount = Math.Max(0, Math.Min(100, Context.TorchAmount + delta));

            RandomSolver.SetRandomSeed(sessionSeed);
            BattleGround.BattleStatus = BattleStatus.Fighting;
            Phase = DuelPhase.NotStarted;
        }

        /// <summary>Starts the battle: rolls surprise, computes the first round order and begins the first turn.</summary>
        public void StartBattle()
        {
            if (BattleGround == null)
                return;

            CheckSurprise();
            BattleGround.Round.StartBattle(BattleGround);
            BeginTurn();
        }

        private void AddPlayerUnit(FightUnitSpec spec, Team team, ref int combatId)
        {
            var heroSpec = spec as HeroFightUnitSpec;
            if (heroSpec != null)
            {
                var heroClass = content.GetHeroClass(heroSpec.ClassId);
                if (heroClass == null)
                    return;
                var hero = HeroGeneration.GenerateHero(heroClass, heroSpec.Seed);
                hero.SelectCombatSkills(heroSpec.SkillIds);
                ApplyQuirks(hero, heroSpec.QuirkIds);
                var unit = new FormationUnit(hero, team);
                unit.PrepareForBattle(combatId++);
                if (team == Team.Heroes)
                    HeroParty.AddUnit(unit);
                else
                    MonsterParty.AddUnit(unit);
                return;
            }

            var monsterSpec = spec as MonsterFightUnitSpec;
            if (monsterSpec != null)
                AddMonster(monsterSpec.MonsterId, team, ref combatId);
        }

        private void AddAiUnit(FightUnitSpec spec, Team team, ref int combatId)
        {
            var monsterSpec = spec as MonsterFightUnitSpec;
            if (monsterSpec != null)
            {
                AddMonster(monsterSpec.MonsterId, team, ref combatId);
                return;
            }

            var heroSpec = spec as HeroFightUnitSpec;
            if (heroSpec != null)
                AddPlayerUnit(heroSpec, team, ref combatId);
        }

        private void AddMonster(string monsterId, Team team, ref int combatId)
        {
            var monsterClass = content.GetMonsterClass(monsterId);
            if (monsterClass == null)
                return;

            var monster = new Monster(monsterClass);
            var brain = content.GetMonsterBrain(monsterClass.MonsterBrainId);
            monster.AssignBrain(brain);

            var unit = new FormationUnit(monster, team);
            unit.PrepareForBattle(combatId++);
            MonsterParty.AddUnit(unit);
        }

        private void InitializeMechanics()
        {
            surpriseResolver = new SurpriseResolver(HeroParty, MonsterParty, BattleGround, Context.TorchAmount);
            dotTickApplier = new DotTickApplier(Events);
            stunRecoveryApplier = new StunRecoveryApplier(content);
            deathCheck = new DeathCheck(HeroParty, MonsterParty, content, Context);
            turnMover = new TurnMover(HeroParty, MonsterParty);
        }

        private void CheckSurprise()
        {
            surpriseResolver.Resolve();
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

            current.CombatInfo.IsSurprised = false;

            if (current.CombatInfo.IsDead)
            {
                BattleGround.Round.OrderedUnits.Remove(current);
                CompleteTurn();
                return;
            }

            if (current.Team == Team.Heroes)
                BattleGround.Round.PreHeroTurn(current, BattleGround);
            else
                BattleGround.Round.PreMonsterTurn(current, BattleGround);

            dotTickApplier.Apply(current);
            deathCheck.Check();
            if (current.CombatInfo.IsDead)
            {
                CompleteTurn();
                return;
            }

            ((Character)current.Character).UpdateRound();

            if (current.Character.GetStatusEffect(StatusType.Stun).IsApplied)
            {
                ((IStunStatusEffect)current.Character.GetStatusEffect(StatusType.Stun)).StunApplied = false;
                Events.ShowPopup(current, PopupType.Unstun);
                Events.ResetHalo(current);
                stunRecoveryApplier.Apply(current);
                logger.Log("[duel] " + current.Character.Name + " was stunned; turn skipped.");
                CompleteTurn();
                return;
            }

            Phase = current.Team == Team.Heroes
                ? DuelPhase.WaitingForHostAction
                : DuelPhase.WaitingForClientAction;
        }

        /// <summary>Executes the acting unit's skill and returns the wire payload to broadcast.</summary>
        /// <param name="skillId">The skill id.</param>
        /// <param name="targetId">The target combat id.</param>
        /// <returns>The wire payload, or null if invalid or not the local turn.</returns>
        public string ExecuteLocalSkill(string skillId, int targetId)
        {
            if (!IsLocalTurn || BattleGround == null || Solver == null)
                return null;

            var unit = CurrentUnit;
            var target = GetUnitByCombatId(targetId);
            var skill = FindSkill(unit, skillId);
            if (unit == null || target == null || skill == null || !IsSkillUsable(unit, skill))
                return null;

            ExecuteSkill(unit, target, skill);
            FinishSkillAction(unit, skill);

            return DuelPayload.Skill(skillId, targetId);
        }

        private void FinishSkillAction(ICombatUnit unit, CombatSkill skill)
        {
            if (skill.IsContinueTurn && !unit.CombatInfo.IsDead)
                BeginTurn();
            else
                CompleteTurn();
        }

        /// <summary>Applies a remote action payload ("skillId|targetId", "pass|0" or "move|rank").</summary>
        /// <param name="payload">The payload.</param>
        public void ApplyRemoteSkill(string payload)
        {
            if (IsLocalTurn || BattleGround == null || Solver == null)
                return;

            var parts = payload.Split('|');
            if (parts.Length < 1)
                return;

            if (parts[0] == DuelPayload.Pass)
            {
                CompleteTurn();
                return;
            }
            if (parts[0] == DuelPayload.Move)
            {
                int rank;
                if (parts.Length == 2 && int.TryParse(parts[1], out rank) && turnMover.TryMove(CurrentUnit, rank))
                    CompleteTurn();
                return;
            }

            if (parts.Length != 2)
                return;

            var unit = CurrentUnit;
            var target = GetUnitByCombatId(int.Parse(parts[1]));
            var skill = FindSkill(unit, parts[0]);
            if (unit == null || target == null || skill == null)
                return;

            ExecuteSkill(unit, target, skill);
            FinishSkillAction(unit, skill);
        }

        /// <summary>Executes a pass: skips the acting unit's turn.</summary>
        /// <returns>The wire payload, or null if not the local turn.</returns>
        public string ExecuteLocalPass()
        {
            if (!IsLocalTurn || BattleGround == null)
                return null;

            CompleteTurn();
            return DuelPayload.PassAction();
        }

        /// <summary>Executes a move of the acting unit to an adjacent rank.</summary>
        /// <param name="newRank">The destination rank.</param>
        /// <returns>The wire payload, or null if invalid or not the local turn.</returns>
        public string ExecuteLocalMove(int newRank)
        {
            if (!IsLocalTurn || BattleGround == null)
                return null;

            var unit = CurrentUnit;
            if (unit == null || !turnMover.TryMove(unit, newRank))
                return null;

            CompleteTurn();
            return DuelPayload.MoveAction(newRank);
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
        public ICombatUnit GetUnitByCombatId(int combatId)
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
            ProcessEventQueues();
            CheckDeaths();

            ExecuteRiposte(unit, target);
            RemoveConditions(unit, target);

            foreach (var entry in Solver.SkillResult.SkillEntries)
                logger.Log("[duel] " + unit.Character.Name + " used " + skill.Id + " -> " +
                    target.Character.Name + " (" + entry.Type + (entry.Amount != 0 ? " " + entry.Amount : "") + ")");
        }

        private void ExecuteRiposte(ICombatUnit attacker, ICombatUnit target)
        {
            if (target == null || ((FormationUnitInfo)target.CombatInfo).IsDead)
                return;
            if (!target.Character.GetStatusEffect(StatusType.Riposte).IsApplied)
                return;

            var riposteSkill = target.Character.RiposteSkill;
            if (riposteSkill == null)
                return;

            Solver.SkillResult.Reset();
            Solver.ExecuteSkill(target, attacker, riposteSkill, null);
            ProcessEventQueues();
            CheckDeaths();
        }

        private void RemoveConditions(ICombatUnit performer, ICombatUnit target)
        {
            if (Solver == null)
                return;
            Solver.RemoveConditions(performer);
            if (target != null)
                Solver.RemoveConditions(target);
        }

        private void ProcessEventQueues()
        {
            foreach (var unit in HeroParty.Units.Concat(MonsterParty.Units))
            {
                if (unit.EventQueue.Count == 0)
                    continue;

                var events = new List<IEffectEvent>(unit.EventQueue);
                unit.EventQueue.Clear();
                foreach (var effectEvent in events)
                    effectEvent.Execute();
            }
        }

        private void AddHero(DuelHeroPick pick, Team team, ref int combatId)
        {
            var heroClass = content.GetHeroClass(pick.ClassId);
            if (heroClass == null)
                return;
            var hero = HeroGeneration.GenerateHero(heroClass, pick.Seed);
            hero.SelectCombatSkills(pick.SelectedSkillIds);
            ApplyQuirks(hero, pick.QuirkIds);
            var unit = new FormationUnit(hero, team);
            unit.PrepareForBattle(combatId++);
            if (team == Team.Heroes)
                HeroParty.AddUnit(unit);
            else
                MonsterParty.AddUnit(unit);
        }

        private void ApplyQuirks(Hero hero, IReadOnlyList<string> quirkIds)
        {
            if (quirkIds == null)
                quirkIds = new List<string>();

            foreach (var quirkId in quirkIds)
            {
                hero.AddQuirk(quirkId);
                var quirk = content.GetQuirk(quirkId);
                if (quirk == null)
                    continue;
                foreach (var buffId in quirk.Buffs)
                {
                    var buff = content.GetBuff(buffId);
                    if (buff != null && hero.GetAttribute(buff.AttributeType) != null)
                        hero.AddBuff(new BuffInfo(buff, BuffDurationType.Permanent, BuffSourceType.Quirk));
                }
            }

            var hp = hero.GetPairedAttribute(AttributeType.HitPoints);
            hp.CurrentValue = hp.ModifiedValue;
        }

        private void NextRound()
        {
            if (BattleGround == null)
                return;

            BattleGround.Round.NextRound(BattleGround);
            BeginTurn();
        }

        private void CheckDeaths()
        {
            deathCheck.Check();
        }

        private static CombatSkill FindSkill(ICombatUnit unit, string skillId)
        {
            if (unit == null || unit.Character.CurrentCombatSkills == null)
                return null;
            return unit.Character.CurrentCombatSkills.FirstOrDefault(skill => skill.Id == skillId);
        }
    }
}