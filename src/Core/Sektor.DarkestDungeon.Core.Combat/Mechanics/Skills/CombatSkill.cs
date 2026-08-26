using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
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

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>Combat skill definition.</summary>
    public class CombatSkill : Skill
    {
        /// <summary>Gets or sets the skill level.</summary>
        public int Level { get; set; }

        /// <summary>Gets or sets the skill type string.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets the skill category.</summary>
        public SkillCategory Category { get; set; }

        /// <summary>Gets or sets the accuracy modifier.</summary>
        public float Accuracy { get; set; }

        /// <summary>Gets or sets the minimum damage.</summary>
        public float DamageMin { get; set; }

        /// <summary>Gets or sets the maximum damage.</summary>
        public float DamageMax { get; set; }

        /// <summary>Gets or sets the damage modifier.</summary>
        public float DamageMod { get; set; }

        /// <summary>Gets or sets the crit modifier.</summary>
        public float CritMod { get; set; }

        /// <summary>Gets or sets a value indicating whether crit is valid.</summary>
        public bool IsCritValid { get; set; }

        /// <summary>Gets or sets a value indicating whether self-targeting is valid.</summary>
        public bool IsSelfValid { get; set; }

        /// <summary>Gets or sets a value indicating whether generation is guaranteed.</summary>
        public bool IsGenerationGuaranteed { get; set; }

        /// <summary>Gets or sets a value indicating whether the skill is knowledgeable.</summary>
        public bool? IsKnowledgeable { get; set; }

        /// <summary>Gets or sets a value indicating whether the skill can miss.</summary>
        public bool? CanMiss { get; set; }

        /// <summary>Gets or sets the extra targets chance.</summary>
        public float ExtraTargetsChance { get; set; }

        /// <summary>Gets or sets the heal component.</summary>
        public HealComponent Heal { get; set; }

        /// <summary>Gets or sets the move component.</summary>
        public MoveComponent Move { get; set; }

        /// <summary>Gets the list of effects.</summary>
        public List<Effect> Effects { get; }

        /// <summary>Gets or sets the launch ranks.</summary>
        public FormationSet LaunchRanks { get; set; }

        /// <summary>Gets or sets the target ranks.</summary>
        public FormationSet TargetRanks { get; set; }

        /// <summary>Gets the list of valid modes.</summary>
        public List<string> ValidModes { get; }

        /// <summary>Gets the mode effects dictionary.</summary>
        public Dictionary<string, List<Effect>> ModeEffects { get; }

        /// <summary>Gets or sets a value indicating whether this skill continues the turn.</summary>
        public bool IsContinueTurn { get; set; }

        /// <summary>Gets or sets the per-turn usage limit.</summary>
        public int? LimitPerTurn { get; set; }

        /// <summary>Gets or sets the per-battle usage limit.</summary>
        public int? LimitPerBattle { get; set; }

        /// <summary>Gets a value indicating whether this is a buff skill.</summary>
        public bool IsBuffSkill
        {
            get
            {
                return Effects.Find(effect => effect.SubEffects.Find(subEffect =>
                    subEffect.Type == EffectSubType.Buff || subEffect.Type == EffectSubType.StatBuff) != null) != null;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="CombatSkill"/> class.</summary>
        public CombatSkill()
        {
            Level = 0;
            IsCritValid = true;
            IsSelfValid = true;
            Category = SkillCategory.Damage;
            ValidModes = new List<string>();
            Effects = new List<Effect>();
            ModeEffects = new Dictionary<string, List<Effect>>();
        }

        /// <summary>Gets the available targets for this skill.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="friends">The friendly party.</param>
        /// <param name="enemies">The enemy party.</param>
        /// <returns>The list of available targets.</returns>
        public List<ICombatUnit> GetAvailableTargets(ICombatUnit performer, IFormationParty friends, IFormationParty enemies)
        {
            if (TargetRanks.IsSelfTarget)
                return new List<ICombatUnit> { performer };

            if (TargetRanks.IsSelfFormation)
                return new List<ICombatUnit>(friends.Units.FindAll(unit =>
                    ((unit == performer && TargetRanks.IsTargetableUnit(unit.Rank, unit.Size) && IsSelfValid) ||
                    (unit != performer && TargetRanks.IsTargetableUnit(unit.Rank, unit.Size))) && (unit.Character.BattleModifiers == null ||
                    unit.Character.BattleModifiers.IsValidFriendlyTarget)));

            return new List<ICombatUnit>(enemies.Units.FindAll(unit => unit != performer && TargetRanks.IsTargetableUnit(unit.Rank, unit.Size)));
        }

        /// <summary>Checks if this skill has available targets.</summary>
        /// <param name="performer">The performing unit.</param>
        /// <param name="friends">The friendly party.</param>
        /// <param name="enemies">The enemy party.</param>
        /// <returns>True if targets are available.</returns>
        public bool HasAvailableTargets(ICombatUnit performer, IFormationParty friends, IFormationParty enemies)
        {
            if (TargetRanks.IsSelfTarget)
                return true;

            if (TargetRanks.IsSelfFormation)
                for (int i = 0; i < friends.Units.Count; i++)
                {
                    if (performer == friends.Units[i])
                    {
                        if (IsSelfValid && TargetRanks.IsTargetableUnit(friends.Units[i].Rank, friends.Units[i].Size))
                            return true;
                    }
                    else if (TargetRanks.IsTargetableUnit(friends.Units[i].Rank, friends.Units[i].Size))
                        return true;
                }
            else
                for (int i = 0; i < enemies.Units.Count; i++)
                    if (TargetRanks.IsTargetableUnit(enemies.Units[i].Rank, enemies.Units[i].Size))
                        return true;

            return false;
        }
    }
}
