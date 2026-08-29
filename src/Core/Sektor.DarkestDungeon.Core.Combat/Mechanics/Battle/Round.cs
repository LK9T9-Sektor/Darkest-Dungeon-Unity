using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>Round/turn state machine for combat.</summary>
    public class Round
    {
        /// <summary>Gets or sets the round number.</summary>
        public int RoundNumber { get; set; }

        /// <summary>Gets or sets the round status.</summary>
        public RoundStatus RoundStatus { get; set; }

        /// <summary>Gets or sets the current turn type.</summary>
        public TurnType TurnType { get; set; }

        /// <summary>Gets or sets the current turn status.</summary>
        public TurnStatus TurnStatus { get; set; }

        /// <summary>Gets or sets the hero's current action.</summary>
        public HeroTurnAction HeroAction { get; set; }

        /// <summary>Gets or sets the selected unit.</summary>
        public ICombatUnit SelectedUnit { get; set; }

        /// <summary>Gets or sets the selected target.</summary>
        public ICombatUnit SelectedTarget { get; set; }

        /// <summary>Gets the ordered units for initiative.</summary>
        public List<ICombatUnit> OrderedUnits { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="Round"/> class.</summary>
        public Round()
        {
            OrderedUnits = new List<ICombatUnit>();
        }

        /// <summary>Prepares for a hero turn.</summary>
        /// <param name="heroUnit">The hero unit taking the turn.</param>
        /// <param name="battleGround">The battlefield.</param>
        public void PreHeroTurn(ICombatUnit heroUnit, IBattleGround battleGround)
        {
            battleGround.LastSkillUsed = null;
            heroUnit.CombatInfo.UpdateNextTurn();
            TurnType = TurnType.HeroTurn;
            TurnStatus = TurnStatus.PreTurn;

            HeroAction = HeroTurnAction.Waiting;
            SelectedUnit = heroUnit;
            SelectedTarget = null;

            OrderedUnits.Remove(heroUnit);
        }

        /// <summary>Called when the hero turn starts.</summary>
        public void OnHeroTurn()
        {
            TurnStatus = TurnStatus.Progress;
        }

        /// <summary>Called when the hero turn ends.</summary>
        public void PostHeroTurn()
        {
            TurnStatus = TurnStatus.PostTurn;

            HeroAction = HeroTurnAction.Waiting;
            SelectedUnit = null;
            SelectedTarget = null;
        }

        /// <summary>Prepares for a monster turn.</summary>
        /// <param name="monsterUnit">The monster unit taking the turn.</param>
        /// <param name="battleGround">The battlefield.</param>
        public void PreMonsterTurn(ICombatUnit monsterUnit, IBattleGround battleGround)
        {
            battleGround.LastSkillUsed = null;
            monsterUnit.CombatInfo.UpdateNextTurn();
            TurnType = TurnType.MonsterTurn;
            TurnStatus = TurnStatus.PreTurn;

            HeroAction = HeroTurnAction.Waiting;
            SelectedUnit = monsterUnit;
            SelectedTarget = null;

            OrderedUnits.Remove(monsterUnit);
        }

        /// <summary>Called when the monster turn starts.</summary>
        public void OnMonsterTurn()
        {
            TurnStatus = TurnStatus.Progress;
        }

        /// <summary>Called when the monster turn ends.</summary>
        public void PostMonsterTurn()
        {
            TurnStatus = TurnStatus.PostTurn;

            HeroAction = HeroTurnAction.Waiting;
            SelectedUnit = null;
            SelectedTarget = null;
        }

        /// <summary>Selects a hero action.</summary>
        /// <param name="actionType">The action type.</param>
        /// <param name="selectedTarget">The selected target.</param>
        public void HeroActionSelected(HeroTurnAction actionType, ICombatUnit selectedTarget)
        {
            HeroAction = actionType;
            SelectedTarget = selectedTarget;
        }

        /// <summary>Starts the battle: resets the round and computes the first round order.</summary>
        /// <param name="battleGround">The battlefield.</param>
        public void StartBattle(IBattleGround battleGround)
        {
            RoundNumber = 0;
            RoundNumber = NextRound(battleGround);
        }

        /// <summary>Computes the next round's unit order by speed (Unity-compatible).</summary>
        /// <param name="battleGround">The battlefield.</param>
        /// <returns>The new round number.</returns>
        public int NextRound(IBattleGround battleGround)
        {
            RoundStatus = RoundStatus.Start;
            OrderedUnits.Clear();

            foreach (var unit in battleGround.HeroParty.Units)
            {
                unit.CombatInfo.UpdateNextRound();
                unit.CombatInfo.InitiativeRoll = unit.Character.Speed + RandomSolver.Next(0, 10) + RandomSolver.NextDouble();
                OrderedUnits.Add(unit);
            }

            foreach (var unit in battleGround.MonsterParty.Units)
            {
                unit.CombatInfo.UpdateNextRound();
                unit.CombatInfo.InitiativeRoll = unit.Character.Speed + RandomSolver.Next(0, 10) + RandomSolver.NextDouble();
                int turns = unit.Character.NumberOfTurns > 1 ? unit.Character.NumberOfTurns : 1;
                for (int i = 0; i < turns; i++)
                    OrderedUnits.Add(unit);
            }

            OrderedUnits = new List<ICombatUnit>(OrderedUnits.OrderByDescending(unit => unit.CombatInfo.InitiativeRoll));

            if (RoundNumber == 0)
            {
                if (battleGround.SurpriseStatus == SurpriseStatus.HeroesSurprised)
                    foreach (var unit in battleGround.HeroParty.Units)
                        unit.CombatInfo.InitiativeRoll -= 100;
                else if (battleGround.SurpriseStatus == SurpriseStatus.MonstersSurprised)
                    foreach (var unit in battleGround.MonsterParty.Units)
                        unit.CombatInfo.InitiativeRoll -= 100;

                OrderedUnits = new List<ICombatUnit>(OrderedUnits.OrderByDescending(unit => unit.CombatInfo.InitiativeRoll));
            }

            return ++RoundNumber;
        }

        /// <summary>Inserts a unit into the order by speed (for bonus initiative).</summary>
        /// <param name="unit">The unit to insert.</param>
        public void InsertInitiativeRoll(ICombatUnit unit)
        {
            for (int i = 0; i < OrderedUnits.Count; i++)
            {
                if (OrderedUnits[i].Character.Speed < unit.Character.Speed - 2)
                {
                    OrderedUnits.Insert(i, unit);
                    return;
                }
            }
            OrderedUnits.Add(unit);
        }
    }
}
