using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle
{
    /// <summary>Central combat solver: skill usability, damage/heal resolution, monster brain, buff conditions.</summary>
    public class BattleSolver
    {
        /// <summary>Gets the shared skill result of the current resolution.</summary>
        public SkillResult SkillResult { get { return skillExecutionResult; } }

        /// <summary>Gets the hero action preview of the current resolution.</summary>
        public HeroActionInfo HeroActionInfo { get { return heroSkillExecutionInfo; } }

        private readonly IBattleContext BattleContext;
        private readonly SkillResult skillExecutionResult = new SkillResult();
        private readonly HeroActionInfo heroSkillExecutionInfo = new HeroActionInfo();

        /// <summary>Initializes a new instance of the <see cref="BattleSolver"/> class.</summary>
        /// <param name="battleContext">The battle context.</param>
        public BattleSolver(IBattleContext battleContext)
        {
            BattleContext = battleContext;
        }

        /// <summary>Checks whether a skill can be used by the given unit.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="skill">The skill to check.</param>
        /// <returns>True if the skill is usable.</returns>
        public bool IsSkillUsable(ICombatUnit performer, CombatSkill skill)
        {
            IFormationParty friends;
            IFormationParty enemies;
            if (performer.Team == Team.Heroes)
            {
                friends = BattleContext.BattleGround.HeroParty;
                enemies = BattleContext.BattleGround.MonsterParty;
            }
            else
            {
                friends = BattleContext.BattleGround.MonsterParty;
                enemies = BattleContext.BattleGround.HeroParty;
            }

            return skill.LaunchRanks.IsLaunchableFrom(performer.Rank, performer.Size) &&
                skill.HasAvailableTargets(performer, friends, enemies) &&
                IsValidInCurrentMode(performer, skill) &&
                !ExceedsLimit(performer.CombatInfo.SkillsUsedThisTurn, skill.LimitPerTurn, skill.Id) &&
                !ExceedsLimit(performer.CombatInfo.SkillsUsedInBattle, skill.LimitPerBattle, skill.Id);
        }

        private static bool IsValidInCurrentMode(ICombatUnit performer, CombatSkill skill)
        {
            if (skill.ValidModes.Count == 0)
                return true;

            var currentMode = performer.Character.CurrentMode;
            return currentMode != null && skill.ValidModes.Contains(currentMode.Id);
        }

        private static bool ExceedsLimit(IReadOnlyList<string> usedSkills, int? limit, string skillId)
        {
            if (!limit.HasValue)
                return false;

            int count = 0;
            foreach (string usedId in usedSkills)
                if (usedId == skillId)
                    count++;

            return count >= limit.Value;
        }

        /// <summary>Checks whether a camping skill can be used by the given unit.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="skill">The camping skill to check.</param>
        /// <returns>True if the camping skill is usable.</returns>
        public bool IsCampingSkillUsable(ICombatUnit performer, CampingSkill skill)
        {
            int skillUsageCount = 0;
            for (int i = 0; i < performer.CombatInfo.SkillsUsedThisTurn.Count; i++)
                if (performer.CombatInfo.SkillsUsedThisTurn[i] == skill.Id)
                    skillUsageCount++;

            if (skillUsageCount >= skill.Limit)
                return false;

            if (skill.TimeCost > BattleContext.CampingTimeLeft)
                return false;

            for (int i = 0; i < skill.Effects.Count; i++)
            {
                switch (skill.Effects[i].Selection)
                {
                    case CampTargetType.Self:
                        return true;
                    case CampTargetType.Individual:
                    case CampTargetType.PartyOther:
                        if (BattleContext.BattleGround.HeroParty.Units.Count > 1)
                            return true;
                        break;
                }
            }

            return false;
        }

        /// <summary>Checks whether a camp effect requirement is fulfilled by the target.</summary>
        /// <param name="target">The target unit.</param>
        /// <param name="requirement">The requirement.</param>
        /// <returns>True if the requirement is fulfilled.</returns>
        public bool IsRequirementFulfilled(ICombatUnit target, CampEffectRequirement requirement)
        {
            switch (requirement)
            {
                case CampEffectRequirement.Afflicted:
                    return target.Character.IsAfflicted;
                case CampEffectRequirement.DeathRecovery:
                    return target.Character.GetStatusEffect(StatusType.DeathRecovery).IsApplied;
                case CampEffectRequirement.Nonreligious:
                    return target.Character.IsMonster == false && target.Character.IsReligious == false;
                case CampEffectRequirement.Religious:
                    return target.Character.IsMonster == false && target.Character.IsReligious;
                default:
                    return true;
            }
        }

        /// <summary>Checks whether a skill has a targetable performer/ally/enemy.</summary>
        /// <param name="skill">The skill.</param>
        /// <param name="allies">The allies formation.</param>
        /// <param name="enemies">The enemies formation.</param>
        /// <param name="performer">The performing unit.</param>
        /// <returns>True if the skill can target something.</returns>
        public bool IsPerformerSkillTargetable(CombatSkill skill, IFormationParty allies, IFormationParty enemies, ICombatUnit performer)
        {
            if (skill.TargetRanks.IsSelfTarget)
            {
                if (skill.Heal != null && performer.CombatInfo.BlockedHealUnitIds.Contains(performer.CombatInfo.CombatId))
                    return false;
                if (skill.IsBuffSkill && performer.CombatInfo.BlockedBuffUnitIds.Contains(performer.CombatInfo.CombatId))
                    return false;
                return true;
            }

            if (skill.TargetRanks.IsSelfFormation)
            {
                if (skill.IsSelfValid)
                {
                    if (skill.Heal != null)
                    {
                        if (performer.CombatInfo.BlockedHealUnitIds.Contains(performer.CombatInfo.CombatId) == false)
                            return true;
                    }
                    else if (skill.IsBuffSkill)
                    {
                        if (performer.CombatInfo.BlockedBuffUnitIds.Contains(performer.CombatInfo.CombatId) == false)
                            return true;
                    }
                    else
                        return true;
                }

                for (int i = 0; i < allies.Units.Count; i++)
                {
                    if (skill.Heal != null && performer.CombatInfo.BlockedHealUnitIds.
                        Contains(allies.Units[i].CombatInfo.CombatId))
                        continue;
                    if (skill.IsBuffSkill && performer.CombatInfo.BlockedBuffUnitIds.
                        Contains(allies.Units[i].CombatInfo.CombatId))
                        continue;

                    if (allies.Units[i] != performer && skill.TargetRanks.IsTargetableUnit(allies.Units[i].Rank, allies.Units[i].Size))
                        return true;
                }
            }
            else
            {
                for (int i = 0; i < enemies.Units.Count; i++)
                    if (skill.TargetRanks.IsTargetableUnit(enemies.Units[i].Rank, enemies.Units[i].Size))
                        return true;
            }

            return false;
        }

        /// <summary>Finds the final targets of a camping effect.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="primaryTarget">The primary target.</param>
        /// <param name="effect">The camp effect.</param>
        /// <param name="finalTargets">The list receiving the final targets.</param>
        public void FindTargets(ICombatUnit performer, ICombatUnit primaryTarget, CampEffect effect, List<ICombatUnit> finalTargets)
        {
            finalTargets.Clear();

            switch (effect.Selection)
            {
                case CampTargetType.Individual:
                    if (primaryTarget != null)
                        finalTargets.Add(primaryTarget);
                    break;
                case CampTargetType.PartyOther:
                    for (int j = 0; j < BattleContext.BattleGround.HeroParty.Units.Count; j++)
                        if (BattleContext.BattleGround.HeroParty.Units[j] != performer)
                            finalTargets.Add(BattleContext.BattleGround.HeroParty.Units[j]);
                    break;
                case CampTargetType.Self:
                    finalTargets.Add(performer);
                    break;
            }
        }

        /// <summary>Gets the available targets of a skill for a performer.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="skill">The skill.</param>
        /// <returns>The list of available target units.</returns>
        public List<ICombatUnit> GetSkillAvailableTargets(ICombatUnit performer, CombatSkill skill)
        {
            if (performer.Team == Team.Heroes)
                return skill.GetAvailableTargets(performer, BattleContext.BattleGround.HeroParty,
                    BattleContext.BattleGround.MonsterParty);
            else
                return skill.GetAvailableTargets(performer, BattleContext.BattleGround.MonsterParty,
                    BattleContext.BattleGround.HeroParty);
        }

        /// <summary>Runs the monster brain (or hero auto-pick) to select a skill and targets.</summary>
        /// <param name="performer">The acting unit.</param>
        /// <param name="combatSkillOverride">Optional forced skill identifier.</param>
        /// <returns>The brain decision.</returns>
        public MonsterBrainDecision UseMonsterBrain(ICombatUnit performer, string combatSkillOverride = null)
        {
            if (performer.Character.IsMonster)
            {
                var monsterBrain = performer.Character.Brain;

                if (string.IsNullOrEmpty(combatSkillOverride))
                {
                    var skillDesires = new List<SkillSelectionDesire>(monsterBrain.SkillDesireSet);
                    var monsterBrainDecision = new MonsterBrainDecision(BrainDecisionType.Pass);

                    while (skillDesires.Count != 0)
                    {
                        SkillSelectionDesire desire = RandomSolver.ChooseByRandom(skillDesires);
                        if (desire != null && desire.SelectSkill(performer, monsterBrainDecision, BattleContext))
                        {
                            var cooldown = monsterBrain.SkillCooldowns.Find(cd => cd.SkillId == monsterBrainDecision.SelectedSkill.Id);
                            if (cooldown != null) performer.CombatInfo.SkillCooldowns.Add(cooldown.Copy());
                            BattleContext.BattleGround.LastSkillUsed = monsterBrainDecision.SelectedSkill.Id;
                            return monsterBrainDecision;
                        }
                        else
                            skillDesires.Remove(desire);
                    }
                    return new MonsterBrainDecision(BrainDecisionType.Pass);
                }
                else
                {
                    var availableSkill = performer.Character.CombatSkills.Find(skill => skill.Id == combatSkillOverride);

                    if (availableSkill != null && IsSkillUsable(performer, availableSkill))
                    {
                        var monsterBrainDecision = new MonsterBrainDecision(BrainDecisionType.Pass);
                        monsterBrainDecision.Decision = BrainDecisionType.Perform;
                        monsterBrainDecision.SelectedSkill = availableSkill;
                        monsterBrainDecision.TargetInfo.Targets = GetSkillAvailableTargets(performer, monsterBrainDecision.SelectedSkill);
                        monsterBrainDecision.TargetInfo.Type = monsterBrainDecision.SelectedSkill.TargetRanks.IsSelfTarget ?
                            SkillTargetType.Self : monsterBrainDecision.SelectedSkill.TargetRanks.IsSelfFormation ?
                            SkillTargetType.Party : SkillTargetType.Enemy;

                        var availableTargetDesires = new List<TargetSelectionDesire>(monsterBrain.TargetDesireSet);

                        while (availableTargetDesires.Count > 0)
                        {
                            TargetSelectionDesire desire = RandomSolver.ChooseByRandom(availableTargetDesires);
                            if (desire.SelectTarget(performer, monsterBrainDecision))
                                return monsterBrainDecision;
                            else
                                availableTargetDesires.Remove(desire);
                        }
                        return new MonsterBrainDecision(BrainDecisionType.Pass);
                    }
                    return new MonsterBrainDecision(BrainDecisionType.Pass);
                }
            }
            else
            {
                var availableSkills = performer.Character.CurrentMode == null ? new List<CombatSkill>(performer.Character.CurrentCombatSkills).FindAll(skill =>
                    skill != null && IsSkillUsable(performer, skill)) : new List<CombatSkill>(performer.Character.CurrentCombatSkills).FindAll(skill =>
                    skill.ValidModes.Contains(performer.Character.CurrentMode.Id) && IsSkillUsable(performer, skill));

                if (availableSkills.Count != 0)
                {
                    var monsterBrainDecision = new MonsterBrainDecision(BrainDecisionType.Pass);
                    monsterBrainDecision.Decision = BrainDecisionType.Perform;
                    monsterBrainDecision.SelectedSkill = availableSkills[RandomSolver.Next(availableSkills.Count)];
                    monsterBrainDecision.TargetInfo.Targets = GetSkillAvailableTargets(performer, monsterBrainDecision.SelectedSkill);
                    monsterBrainDecision.TargetInfo.Type = monsterBrainDecision.SelectedSkill.TargetRanks.IsSelfTarget ?
                        SkillTargetType.Self : monsterBrainDecision.SelectedSkill.TargetRanks.IsSelfFormation ?
                        SkillTargetType.Party : SkillTargetType.Enemy;

                    var availableTargets = new List<ICombatUnit>(monsterBrainDecision.TargetInfo.Targets);
                    if (availableTargets.Count > 0)
                    {
                        monsterBrainDecision.TargetInfo.Targets.Clear();

                        if (monsterBrainDecision.SelectedSkill.TargetRanks.IsMultitarget)
                        {
                            monsterBrainDecision.TargetInfo.Targets.AddRange(availableTargets);
                            return monsterBrainDecision;
                        }
                        else
                        {
                            int index = RandomSolver.Next(availableTargets.Count);
                            monsterBrainDecision.TargetInfo.Targets.Add(availableTargets[index]);
                            availableTargets.RemoveAt(index);
                            return monsterBrainDecision;
                        }
                    }
                    return new MonsterBrainDecision(BrainDecisionType.Pass);
                }
                return new MonsterBrainDecision(BrainDecisionType.Pass);
            }
        }

        /// <summary>Selects the concrete targets for a skill.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="primaryTarget">The primary target.</param>
        /// <param name="skill">The skill.</param>
        /// <returns>The target info.</returns>
        public SkillTargetInfo SelectSkillTargets(ICombatUnit performer, ICombatUnit primaryTarget, CombatSkill skill)
        {
            if (skill.TargetRanks.IsSelfTarget)
                return new SkillTargetInfo(performer, SkillTargetType.Self);

            if (skill.TargetRanks.IsSelfFormation)
            {
                if (skill.TargetRanks.IsMultitarget)
                {
                    var targets = performer.Team == Team.Heroes ?
                        new List<ICombatUnit>(BattleContext.BattleGround.HeroParty.Units) :
                        new List<ICombatUnit>(BattleContext.BattleGround.MonsterParty.Units);

                    if (!skill.IsSelfValid)
                        targets.Remove(performer);

                    for (int i = targets.Count - 1; i >= 0; i--)
                        if (!skill.TargetRanks.IsTargetableUnit(targets[i].Rank, targets[i].Size))
                            targets.Remove(targets[i]);

                    return new SkillTargetInfo(targets, SkillTargetType.Party);
                }
                else
                    return new SkillTargetInfo(primaryTarget, SkillTargetType.Party);
            }
            else
            {
                if (skill.TargetRanks.IsMultitarget)
                {
                    var targets = performer.Team == Team.Heroes ?
                        new List<ICombatUnit>(BattleContext.BattleGround.MonsterParty.Units) :
                        new List<ICombatUnit>(BattleContext.BattleGround.HeroParty.Units);

                    for (int i = targets.Count - 1; i >= 0; i--)
                        if (!skill.TargetRanks.IsTargetableUnit(targets[i].Rank, targets[i].Size))
                            targets.Remove(targets[i]);

                    return new SkillTargetInfo(targets, SkillTargetType.Enemy);
                }
                else
                    return new SkillTargetInfo(primaryTarget, SkillTargetType.Enemy);
            }
        }

        /// <summary>Executes a skill against a target, resolving heal or damage.</summary>
        /// <param name="performerUnit">The performing unit.</param>
        /// <param name="targetUnit">The target unit.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="artInfo">The skill art info.</param>
        public void ExecuteSkill(ICombatUnit performerUnit, ICombatUnit targetUnit, CombatSkill skill, SkillArtInfo artInfo)
        {
            SkillResult.Skill = skill;
            SkillResult.ArtInfo = artInfo;

            performerUnit.CombatInfo.SkillsUsedThisTurn.Add(skill.Id);
            performerUnit.CombatInfo.SkillsUsedInBattle.Add(skill.Id);

            var target = targetUnit.Character;
            var performer = performerUnit.Character;

            ApplyConditions(performerUnit, targetUnit, skill);

            if (skill.Move != null && !performerUnit.CombatInfo.IsImmobilized)
            {
                if (skill.Move.Pullforward > 0)
                    BattleContext.Events.Pull(performerUnit, skill.Move.Pullforward, false);
                else if (skill.Move.Pushback > 0)
                    BattleContext.Events.Push(performerUnit, skill.Move.Pushback, false);
            }

            if (skill.Category == SkillCategory.Heal || skill.Category == SkillCategory.Support)
            {
                if (skill.Heal != null)
                {
                    float initialHeal = RandomSolver.Next(skill.Heal.MinAmount, skill.Heal.MaxAmount + 1) *
                        (1 + performer.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

                    if (skill.IsCritValid)
                    {
                        float critChance = performer.GetSingleAttribute(AttributeType.CritChance).ModifiedValue + skill.CritMod / 100;
                        if (RandomSolver.CheckSuccess(critChance))
                        {
                            int critHeal = target.Heal(initialHeal * 1.5f, true);
                            BattleContext.Events.UpdateOverlay(targetUnit);
                            SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, critHeal, SkillResultType.CritHeal));

                            ApplyEffects(performerUnit, targetUnit, skill);
                            if (targetUnit.Character.IsMonster == false)
                                BattleContext.ApplyEffectById("crit_heal_stress_heal", targetUnit, true);
                            return;
                        }
                    }

                    int heal = target.Heal(initialHeal, true);
                    BattleContext.Events.UpdateOverlay(targetUnit);

                    SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, heal, SkillResultType.Heal));
                    ApplyEffects(performerUnit, targetUnit, skill);
                }
                else
                {
                    SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, SkillResultType.Utility));
                    ApplyEffects(performerUnit, targetUnit, skill);
                }
            }
            else
            {
                float accuracy = skill.Accuracy + performer.Accuracy;
                float hitChance = Clamp(accuracy - target.Dodge, 0, 0.95f);
                float roll = (float)RandomSolver.NextDouble();
                if (target.BattleModifiers != null && target.BattleModifiers.CanBeHit == false)
                    roll = float.MaxValue;

                if (roll > hitChance)
                {
                    if (!(skill.CanMiss == false || (target.BattleModifiers != null && target.BattleModifiers.CanBeMissed == false)))
                    {
                        if (roll > Math.Min(accuracy, 0.95f))
                            SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, SkillResultType.Miss));
                        else
                            SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, SkillResultType.Dodge));

                        ApplyEffects(performerUnit, targetUnit, skill);
                        return;
                    }
                }

                float initialDamage = !performer.IsMonster ?
                    Lerp(performer.MinDamage, performer.MaxDamage, (float)RandomSolver.NextDouble()) * (1 + skill.DamageMod) :
                    Lerp(skill.DamageMin, skill.DamageMax, (float)RandomSolver.NextDouble()) * performer.DamageMod;

                int damage = CeilToInt(initialDamage * (1 - target.Protection));
                if (damage < 0)
                    damage = 0;

                if (target.BattleModifiers != null && target.BattleModifiers.CanBeDamagedDirectly == false)
                    damage = 0;

                if (skill.IsCritValid)
                {
                    float critChance = performer.GetSingleAttribute(AttributeType.CritChance).ModifiedValue + skill.CritMod;
                    if (RandomSolver.CheckSuccess(critChance))
                    {
                        int critDamage = target.TakeDamage(damage * 1.5f);
                        BattleContext.Events.UpdateOverlay(targetUnit);

                        if (target.HasZeroHealth)
                            SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, critDamage, true, SkillResultType.Crit));
                        else
                            SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, critDamage, SkillResultType.Crit));

                        ApplyEffects(performerUnit, targetUnit, skill);
                        if (targetUnit.Character.IsMonster == false)
                            BattleContext.ApplyEffectById("Stress 2", targetUnit, true);
                        return;
                    }
                }
                damage = target.TakeDamage(damage);
                BattleContext.Events.UpdateOverlay(targetUnit);
                if (target.HasZeroHealth)
                    SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, damage, true, SkillResultType.Hit));
                else
                    SkillResult.AddResultEntry(new SkillResultEntry(targetUnit, damage, SkillResultType.Hit));

                ApplyEffects(performerUnit, targetUnit, skill);
            }
        }

        /// <summary>Calculates the potential hit/crit/damage preview of a skill.</summary>
        /// <param name="performerUnit">The performing unit.</param>
        /// <param name="targetUnit">The target unit.</param>
        /// <param name="skill">The skill.</param>
        public void CalculateSkillPotential(ICombatUnit performerUnit, ICombatUnit targetUnit, CombatSkill skill)
        {
            var target = targetUnit.Character;
            var performer = performerUnit.Character;

            if (skill.Category == SkillCategory.Heal || skill.Category == SkillCategory.Support)
                HeroActionInfo.UpdateInfo(false, 0, 0, 0, 0);
            else
            {
                ApplyConditions(performerUnit, targetUnit, skill);
                float accuracy = skill.Accuracy + performer.Accuracy;
                float hitChance = Clamp(accuracy - target.Dodge, 0, 0.95f);
                if (skill.CanMiss == false)
                    hitChance = 1;
                else if (target.BattleModifiers != null && target.BattleModifiers.CanBeMissed == false)
                    hitChance = 1;

                float initialMinDamage = !performer.IsMonster ?
                    performer.MinDamage * (1 + skill.DamageMod) :
                    skill.DamageMin * performer.DamageMod;
                float initialMaxDamage = !performer.IsMonster ?
                    performer.MaxDamage * (1 + skill.DamageMod) :
                    skill.DamageMax * performer.DamageMod;

                int minDamage = CeilToInt(initialMinDamage * (1 - target.Protection));
                if (minDamage < 0)
                    minDamage = 0;
                int maxDamage = CeilToInt(initialMaxDamage * (1 - target.Protection));
                if (maxDamage < 0)
                    maxDamage = 0;

                if (target.BattleModifiers != null && target.BattleModifiers.CanBeDamagedDirectly == false)
                {
                    minDamage = 0;
                    maxDamage = 0;
                }

                float critChance = 0;
                if (skill.IsCritValid)
                    critChance = performer.GetSingleAttribute(AttributeType.CritChance).ModifiedValue + skill.CritMod;

                RemoveConditions(performerUnit);
                RemoveConditions(targetUnit);
                HeroActionInfo.UpdateInfo(true, hitChance, critChance, minDamage, maxDamage);
            }
        }

        /// <summary>Applies all effects of a skill to the target.</summary>
        /// <param name="performerUnit">The performing unit.</param>
        /// <param name="targetUnit">The target unit.</param>
        /// <param name="skill">The skill.</param>
        public void ApplyEffects(ICombatUnit performerUnit, ICombatUnit targetUnit, CombatSkill skill)
        {
            if (skill.ValidModes.Count > 1 && performerUnit.Character.CurrentMode != null)
                foreach (var effect in skill.ModeEffects[performerUnit.Character.CurrentMode.Id])
                    effect.Apply(performerUnit, targetUnit, SkillResult, BattleContext);

            foreach (var effect in skill.Effects)
                effect.Apply(performerUnit, targetUnit, SkillResult, BattleContext);
        }

        /// <summary>Applies the combat buff conditions of a skill to both units.</summary>
        /// <param name="performerUnit">The performing unit.</param>
        /// <param name="targetUnit">The target unit.</param>
        /// <param name="skill">The skill.</param>
        public void ApplyConditions(ICombatUnit performerUnit, ICombatUnit targetUnit, CombatSkill skill)
        {
            BattleContext.ApplyCombatUnitRules(performerUnit, targetUnit, skill, performerUnit.Character.RiposteSkill == skill);
            BattleContext.ApplyCombatUnitRules(targetUnit, performerUnit, skill, false);

            foreach (var effect in skill.Effects)
                effect.ApplyTargetConditions(performerUnit, targetUnit, BattleContext);
        }

        /// <summary>Removes the combat conditions from a unit.</summary>
        /// <param name="targetUnit">The unit.</param>
        public void RemoveConditions(ICombatUnit targetUnit)
        {
            BattleContext.ApplyIdleUnitRules(targetUnit);
            targetUnit.Character.RemoveConditionalBuffs();
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private static int CeilToInt(float value)
        {
            return (int)Math.Ceiling(value);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}