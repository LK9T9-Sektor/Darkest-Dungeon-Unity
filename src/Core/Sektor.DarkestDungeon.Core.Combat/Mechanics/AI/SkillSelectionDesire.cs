using System;
using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;
using Sektor.DarkestDungeon.Core.Combat.Raid.Events;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Raid;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.AI
{
    /// <summary>Abstract base for skill selection desires that evaluate weighted skill choices.</summary>
    public abstract class SkillSelectionDesire : IProportionValue
    {
        /// <summary>Gets or sets the proportional chance weight.</summary>
        public virtual int Chance { get; set; }

        private readonly Dictionary<SkillSelectRestriction, int?> restrictions;

        /// <summary>Initializes a new instance of the <see cref="SkillSelectionDesire"/> class.</summary>
        protected SkillSelectionDesire()
        {
            restrictions = new Dictionary<SkillSelectRestriction, int?>();
            foreach (SkillSelectRestriction attribute in Enum.GetValues(typeof(SkillSelectRestriction)))
                restrictions.Add(attribute, null);
        }

        /// <summary>Attempts to select a skill and target for the given performer.</summary>
        /// <param name="performer">The combat unit performing the action.</param>
        /// <param name="decision">The brain decision to populate.</param>
        /// <param name="battleContext">The current battle context.</param>
        /// <returns>True if a valid skill and target were selected.</returns>
        public bool SelectSkill(ICombatUnit performer, MonsterBrainDecision decision, IBattleContext battleContext)
        {
            if (IsRestricted(performer, battleContext))
                return false;

            var availableSkills = GetMonsterCombatSkills(performer)
                .FindAll(skill => IsValidSkill(performer, skill, battleContext));

            if (availableSkills.Count > 0)
            {
                decision.Decision = BrainDecisionType.Perform;
                decision.SelectedSkill = availableSkills[RandomSolver.Next(availableSkills.Count)];
                decision.TargetInfo.Targets = battleContext.GetSkillAvailableTargets(performer, decision.SelectedSkill);
                decision.TargetInfo.Targets.RemoveAll(target => !IsValidTarget(target));
                decision.TargetInfo.Type = decision.SelectedSkill.TargetRanks.SkillTargetType;

                if (decision.TargetInfo.Targets.Count == 0)
                    return false;

                var availableTargetDesires = GetMonsterBrain(performer).TargetDesireSet.FindAll(IsValidTargetDesire);
                while (availableTargetDesires.Count > 0)
                {
                    TargetSelectionDesire desire = RandomSolver.ChooseByRandom(availableTargetDesires);
                    if (desire.SelectTarget(performer, decision))
                        return true;

                    availableTargetDesires.Remove(desire);
                }
                return false;
            }
            return false;
        }

        /// <summary>Checks whether the skill selection is restricted by battle conditions.</summary>
        /// <param name="performer">The combat unit.</param>
        /// <param name="battleContext">The battle context.</param>
        /// <returns>True if restricted.</returns>
        protected virtual bool IsRestricted(ICombatUnit performer, IBattleContext battleContext)
        {
            if (restrictions[SkillSelectRestriction.MonstersMin] != null)
                if (restrictions[SkillSelectRestriction.MonstersMin].Value > battleContext.BattleGround.MonsterNumber)
                    return true;
            if (restrictions[SkillSelectRestriction.MonstersMax] != null)
                if (restrictions[SkillSelectRestriction.MonstersMax].Value < battleContext.BattleGround.MonsterNumber)
                    return true;
            if (restrictions[SkillSelectRestriction.MonstersSizeMin] != null)
                if (restrictions[SkillSelectRestriction.MonstersSizeMin].Value > battleContext.BattleGround.MonsterSize)
                    return true;
            if (restrictions[SkillSelectRestriction.MonstersSizeMax] != null)
                if (restrictions[SkillSelectRestriction.MonstersSizeMax].Value < battleContext.BattleGround.MonsterSize)
                    return true;
            if (restrictions[SkillSelectRestriction.MarkedHeroesMin] != null)
                if (restrictions[SkillSelectRestriction.MarkedHeroesMin].Value > battleContext.BattleGround.MarkedHeroes)
                    return true;
            if (restrictions[SkillSelectRestriction.MarkedHeroesMax] != null)
                if (restrictions[SkillSelectRestriction.MarkedHeroesMax].Value < battleContext.BattleGround.MarkedHeroes)
                    return true;
            if (restrictions[SkillSelectRestriction.NonVirtuedHeroesMin] != null)
                if (restrictions[SkillSelectRestriction.NonVirtuedHeroesMin].Value > battleContext.BattleGround.NonVirtuedHeroes)
                    return true;
            if (restrictions[SkillSelectRestriction.ControlCountMin] != null)
                if (restrictions[SkillSelectRestriction.ControlCountMin].Value > battleContext.BattleGround.ControlCount)
                    return true;
            if (restrictions[SkillSelectRestriction.ControlCountMax] != null)
                if (restrictions[SkillSelectRestriction.ControlCountMax].Value < battleContext.BattleGround.ControlCount)
                    return true;
            if (restrictions[SkillSelectRestriction.HeroesMin] != null)
                if (restrictions[SkillSelectRestriction.HeroesMin].Value > battleContext.BattleGround.HeroNumber)
                    return true;
            if (restrictions[SkillSelectRestriction.VirtuedHeroesMax] != null)
                if (restrictions[SkillSelectRestriction.VirtuedHeroesMax].Value < battleContext.BattleGround.VirtuedHeroes)
                    return true;
            if (restrictions[SkillSelectRestriction.GuardedMonstersMin] != null)
                if (restrictions[SkillSelectRestriction.GuardedMonstersMin].Value > battleContext.BattleGround.GuardedMonsters)
                    return true;
            if (restrictions[SkillSelectRestriction.GuardedMonstersMax] != null)
                if (restrictions[SkillSelectRestriction.GuardedMonstersMax].Value < battleContext.BattleGround.GuardedMonsters)
                    return true;

            return false;
        }

        /// <summary>Determines whether the given skill is usable by the performer.</summary>
        /// <param name="performer">The combat unit.</param>
        /// <param name="skill">The skill to check.</param>
        /// <param name="battleContext">The battle context.</param>
        /// <returns>True if the skill is usable and not on cooldown.</returns>
        protected virtual bool IsValidSkill(ICombatUnit performer, CombatSkill skill, IBattleContext battleContext)
        {
            if (!battleContext.IsSkillUsable(performer, skill))
                return false;

            if (performer.CombatInfo.SkillCooldowns.Any(cooldown => cooldown.SkillId == skill.Id))
                return false;

            return true;
        }

        /// <summary>Determines whether the given target is valid.</summary>
        /// <param name="target">The potential target unit.</param>
        /// <returns>True if the target is valid.</returns>
        protected virtual bool IsValidTarget(ICombatUnit target)
        {
            return true;
        }

        /// <summary>Determines whether the given target desire is applicable.</summary>
        /// <param name="desire">The target selection desire.</param>
        /// <returns>True if the desire is valid.</returns>
        protected virtual bool IsValidTargetDesire(TargetSelectionDesire desire)
        {
            return true;
        }

        /// <summary>Populates restrictions from a data set dictionary.</summary>
        /// <param name="dataSet">The data set to process.</param>
        protected virtual void GenerateFromDataSet(Dictionary<string, object> dataSet)
        {
            foreach (var token in dataSet)
                ProcessBaseDataToken(token);
        }

        /// <summary>Processes a single key-value token from the data set.</summary>
        /// <param name="token">The key-value pair to process.</param>
        protected void ProcessBaseDataToken(KeyValuePair<string, object> token)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)((double)token.Value * 100);
                    break;
                case "marked_heroes_min":
                    restrictions[SkillSelectRestriction.MarkedHeroesMin] = (int)(long)token.Value;
                    break;
                case "marked_heroes_max":
                    restrictions[SkillSelectRestriction.MarkedHeroesMax] = (int)(long)token.Value;
                    break;
                case "monsters_size_min":
                    restrictions[SkillSelectRestriction.MonstersSizeMin] = (int)(long)token.Value;
                    break;
                case "monsters_size_max":
                    restrictions[SkillSelectRestriction.MonstersSizeMax] = (int)(long)token.Value;
                    break;
                case "non_virtued_heroes_min":
                    restrictions[SkillSelectRestriction.NonVirtuedHeroesMin] = (int)(long)token.Value;
                    break;
                case "virtued_heroes_max":
                    restrictions[SkillSelectRestriction.VirtuedHeroesMax] = (int)(long)token.Value;
                    break;
                case "non_deaths_door_heroes_min":
                    restrictions[SkillSelectRestriction.NonDeathsDorrHeroesMin] = (int)(long)token.Value;
                    break;
                case "control_count_min":
                    restrictions[SkillSelectRestriction.ControlCountMin] = (int)(long)token.Value;
                    break;
                case "control_count_max":
                    restrictions[SkillSelectRestriction.ControlCountMax] = (int)(long)token.Value;
                    break;
                case "heroes_min":
                    restrictions[SkillSelectRestriction.HeroesMin] = (int)(long)token.Value;
                    break;
                case "monsters_min":
                    restrictions[SkillSelectRestriction.MonstersMin] = (int)(long)token.Value;
                    break;
                case "monsters_max":
                    restrictions[SkillSelectRestriction.MonstersMax] = (int)(long)token.Value;
                    break;
                case "guarded_monsters_min":
                    restrictions[SkillSelectRestriction.GuardedMonstersMin] = (int)(long)token.Value;
                    break;
                case "guarded_monsters_max":
                    restrictions[SkillSelectRestriction.GuardedMonstersMax] = (int)(long)token.Value;
                    break;
            }
        }

        /// <summary>Retrieves the combat skills of the performer's monster data.</summary>
        /// <param name="performer">The combat unit.</param>
        /// <returns>The list of combat skills.</returns>
        protected virtual List<CombatSkill> GetMonsterCombatSkills(ICombatUnit performer)
        {
            return performer.Character.CombatSkills;
        }

        /// <summary>Retrieves the monster brain of the performer.</summary>
        /// <param name="performer">The combat unit.</param>
        /// <returns>The monster brain.</returns>
        protected virtual MonsterBrain GetMonsterBrain(ICombatUnit performer)
        {
            return performer.Character.Brain;
        }
    }
}
