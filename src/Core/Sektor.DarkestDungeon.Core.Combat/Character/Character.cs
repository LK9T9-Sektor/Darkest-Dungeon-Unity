using System;
using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Character.Statuses;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>Base character model with attributes, statuses, buffs and combat state.</summary>
    public class Character : ICharacter
    {
        /// <inheritdoc/>
        public virtual string Name { get { return "Character"; } }

        /// <inheritdoc/>
        public virtual string Class { get { return "Class"; } }

        /// <inheritdoc/>
        public virtual int Size { get { return 1; } }

        /// <inheritdoc/>
        public virtual bool AtDeathsDoor { get { return false; } }

        /// <inheritdoc/>
        public virtual bool IsStressed { get { return false; } }

        /// <inheritdoc/>
        public virtual bool IsOverstressed { get { return false; } }

        /// <inheritdoc/>
        public virtual bool IsVirtued { get { return false; } }

        /// <inheritdoc/>
        public virtual bool IsAfflicted { get { return false; } }

        /// <inheritdoc/>
        public virtual bool IsMonster { get { return false; } }

        /// <inheritdoc/>
        public bool InMode { get { return CurrentMode != null; } }

        /// <inheritdoc/>
        public ICharacterMode CurrentMode { get; set; }

        /// <inheritdoc/>
        public virtual List<ICharacterMode> Modes { get { return null; } }

        /// <inheritdoc/>
        public virtual IBattleModifier BattleModifiers { get { return null; } }

        /// <inheritdoc/>
        public virtual List<SkillArtInfo> SkillArtInfo { get { return new List<SkillArtInfo>(); } }

        /// <inheritdoc/>
        public virtual List<MonsterType> MonsterTypes { get { return null; } }

        /// <inheritdoc/>
        public virtual List<CombatSkill> CombatSkills { get { return null; } }

        /// <inheritdoc/>
        public virtual MonsterBrain Brain { get { return null; } }

        /// <inheritdoc/>
        public float HealthRatio { get { return GetPairedAttribute(AttributeType.HitPoints).ValueRatio; } }

        /// <inheritdoc/>
        public float CurrentHealth { get { return GetPairedAttribute(AttributeType.HitPoints).CurrentValue; } }

        /// <inheritdoc/>
        public float MaxHealth { get { return GetPairedAttribute(AttributeType.HitPoints).ModifiedValue; } }

        /// <inheritdoc/>
        public bool HasZeroHealth { get { return GetPairedAttribute(AttributeType.HitPoints).CurrentValue <= 0; } }

        /// <inheritdoc/>
        public virtual int PreferableSkill { get { return -1; } }

        /// <inheritdoc/>
        public IEmptyCaptor EmptyCaptor { get { return null; } }

        /// <inheritdoc/>
        public object ControllerCaptor { get { return null; } }

        /// <inheritdoc/>
        public IPairedAttribute Stress { get { return GetPairedAttribute(AttributeType.Stress); } }

        /// <inheritdoc/>
        public virtual CombatSkill RiposteSkill { get { return null; } }

        /// <inheritdoc/>
        public virtual List<CombatSkill> CurrentCombatSkills { get { return null; } }

        /// <inheritdoc/>
        public virtual bool IsReligious { get { return false; } }

        /// <summary>Gets the health paired attribute.</summary>
        public IPairedAttribute Health { get { return GetPairedAttribute(AttributeType.HitPoints); } }

        /// <summary>Gets the list of applied buffs.</summary>
        protected readonly List<BuffInfo> BuffInfo;

        /// <summary>Gets the status effects dictionary.</summary>
        protected readonly Dictionary<StatusType, StatusEffect> StatusEffects;

        private readonly Dictionary<AttributeType, SingleAttribute> singleAttributes;
        private readonly Dictionary<AttributeType, PairedAttribute> pairedAttributes;

        private static readonly AttributeType[] SingleStats = new AttributeType[]
        {
            AttributeType.DefenseRating, AttributeType.ProtectionRating, AttributeType.SpeedRating,
            AttributeType.AttackRating, AttributeType.CritChance, AttributeType.DamageLow, AttributeType.DamageHigh,
        };

        private static readonly AttributeType[] Modifiers = new AttributeType[]
        {
            AttributeType.HpHealAmount, AttributeType.HpHealPercent, AttributeType.MoveChance, AttributeType.DebuffChance,
            AttributeType.StressHealPercent, AttributeType.DmgReceivedPercent, AttributeType.HpHealReceivedPercent,
            AttributeType.StressDmgReceivedPercent, AttributeType.StressHealReceivedPercent, AttributeType.StunChance,
            AttributeType.PoisonChance, AttributeType.BleedChance, AttributeType.ResolveCheckPercent, AttributeType.StressDmgPercent,
            AttributeType.ScoutingChance, AttributeType.PartySurpriseChance, AttributeType.MonsterSurpirseChance,
            AttributeType.RemoveQuirkChance, AttributeType.FoodConsumption, AttributeType.StarvingDamagePercent,
        };

        /// <summary>Initializes a new instance of the <see cref="Character"/> class.</summary>
        protected Character()
        {
            BuffInfo = new List<BuffInfo>();
            pairedAttributes = new Dictionary<AttributeType, PairedAttribute>();
            singleAttributes = new Dictionary<AttributeType, SingleAttribute>();
            StatusEffects = new Dictionary<StatusType, StatusEffect>();
            InitializeBasicStatuses(StatusEffects);

            AddPairedAttribute(AttributeType.HitPoints, new PairedAttribute());

            for (int i = 0; i < SingleStats.Length; i++)
                AddSingleAttribute(SingleStats[i], new SingleAttribute());

            for (int i = 0; i < Modifiers.Length; i++)
                AddSingleAttribute(Modifiers[i], new SingleAttribute());
        }

        /// <summary>Initializes the basic status effects dictionary.</summary>
        /// <param name="targetDictionary">The target dictionary.</param>
        public static void InitializeBasicStatuses(Dictionary<StatusType, StatusEffect> targetDictionary)
        {
            targetDictionary.Clear();

            targetDictionary.Add(StatusType.Stun, new StunStatusEffect());
            targetDictionary.Add(StatusType.Marked, new MarkStatusEffect());
            targetDictionary.Add(StatusType.Riposte, new RiposteStatusEffect());
            targetDictionary.Add(StatusType.Bleeding, new BleedingStatusEffect());
            targetDictionary.Add(StatusType.Poison, new PoisonStatusEffect());
            targetDictionary.Add(StatusType.Guard, new GuardStatusEffect());
            targetDictionary.Add(StatusType.Guarded, new GuardedStatusEffect());
            targetDictionary.Add(StatusType.DeathsDoor, new DeathsDoorStatusEffect());
            targetDictionary.Add(StatusType.DeathRecovery, new DeathRecoveryStatusEffect());
        }

        /// <summary>Updates round-scoped statuses and buff durations.</summary>
        public void UpdateRound()
        {
            foreach (var effect in StatusEffects)
                effect.Value.UpdateNextTurn();

            UpdateDurations(BuffDurationType.Round);
        }

        /// <summary>Updates the durations of buffs of the given type.</summary>
        /// <param name="durationType">The duration type.</param>
        public void UpdateDurations(BuffDurationType durationType)
        {
            foreach (var buffEntry in BuffInfo.FindAll(roundBuff => roundBuff.DurationType == durationType))
                if (--buffEntry.Duration <= 0)
                    RemoveBuff(buffEntry);
        }

        /// <inheritdoc/>
        public void RemoveConditionalBuffs()
        {
            for (int i = BuffInfo.Count - 1; i >= 0; i--)
            {
                if (BuffInfo[i].SourceType == BuffSourceType.Condition)
                    RemoveBuff(BuffInfo[i]);
            }
        }

        /// <inheritdoc/>
        public void AddBuff(BuffInfo newBuffInfo)
        {
            BuffInfo.Add(newBuffInfo);
            if (newBuffInfo.Buff.RuleType == BuffRule.Always)
                ApplyBuff(newBuffInfo);
        }

        /// <summary>Applies all buff rules using the given combat context.</summary>
        /// <param name="rules">The combat rules context.</param>
        public void ApplyAllBuffRules(BattleRulesContext rules)
        {
            for (int i = 0; i < BuffInfo.Count; i++)
                ApplyBuffRule(BuffInfo[i], rules);
        }

        /// <inheritdoc/>
        public virtual int Heal(float healAmount, bool includeModifier)
        {
            int heal = includeModifier
                ? CeilToInt(healAmount * (1 + this[AttributeType.HpHealReceivedPercent].ModifiedValue))
                : CeilToInt(healAmount);

            this[AttributeType.HitPoints, true].IncreaseValue(heal);
            return heal;
        }

        /// <summary>Heals a percentage of max health.</summary>
        /// <param name="healPercent">The percentage.</param>
        /// <param name="includeModifier">Whether the received modifier applies.</param>
        /// <returns>The actual health restored.</returns>
        public int HealPercent(float healPercent, bool includeModifier)
        {
            return Heal(this[AttributeType.HitPoints, true].ModifiedValue * healPercent, includeModifier);
        }

        /// <inheritdoc/>
        public int TakeDamage(float damageAmount)
        {
            int damage = RoundToInt(damageAmount);
            GetPairedAttribute(AttributeType.HitPoints).DecreaseValue(damage);
            return damage;
        }

        /// <inheritdoc/>
        public void TakeDamagePercent(float damagePercent)
        {
            TakeDamage(this[AttributeType.HitPoints, true].ModifiedValue * damagePercent);
        }

        /// <inheritdoc/>
        public SingleAttribute GetSingleAttribute(AttributeType stat)
        {
            if (singleAttributes.ContainsKey(stat))
                return singleAttributes[stat];
            return null;
        }

        /// <inheritdoc/>
        IAttribute ICharacter.GetSingleAttribute(AttributeType stat)
        {
            return GetSingleAttribute(stat);
        }

        /// <inheritdoc/>
        public IPairedAttribute GetPairedAttribute(AttributeType stat)
        {
            if (pairedAttributes.ContainsKey(stat))
                return pairedAttributes[stat];
            return null;
        }

        /// <inheritdoc/>
        IStress ICharacter.Stress
        {
            get { return (IStress)GetPairedAttribute(AttributeType.Stress); }
        }

        /// <summary>Gets an attribute as its base type.</summary>
        /// <param name="stat">The attribute type.</param>
        /// <returns>The base attribute or null.</returns>
        public BaseAttribute GetAttribute(AttributeType stat)
        {
            if (singleAttributes.ContainsKey(stat))
                return singleAttributes[stat];
            else if (pairedAttributes.ContainsKey(stat))
                return pairedAttributes[stat];
            return null;
        }

        /// <inheritdoc/>
        public IStatusEffect GetStatusEffect(StatusType type)
        {
            return StatusEffects[type];
        }

        /// <summary>Gets a single attribute by indexer.</summary>
        /// <param name="stat">The attribute type.</param>
        /// <returns>The single attribute or null.</returns>
        public SingleAttribute this[AttributeType stat]
        {
            get
            {
                if (singleAttributes.ContainsKey(stat))
                    return singleAttributes[stat];
                return null;
            }
        }

        /// <summary>Gets a paired attribute by indexer.</summary>
        /// <param name="stat">The attribute type.</param>
        /// <param name="paired">Marker for the paired indexer.</param>
        /// <returns>The paired attribute or null.</returns>
        public PairedAttribute this[AttributeType stat, bool paired]
        {
            get
            {
                if (pairedAttributes.ContainsKey(stat))
                    return pairedAttributes[stat];
                return null;
            }
        }

        /// <inheritdoc/>
        public virtual bool AddQuirk(IQuirk quirk)
        {
            return false;
        }

        /// <inheritdoc/>
        public virtual IQuirk AddRandomDisease()
        {
            return null;
        }

        /// <inheritdoc/>
        public virtual void RevertTrait()
        {
        }

        /// <inheritdoc/>
        public float Speed { get { return Clamp(GetSingleAttribute(AttributeType.SpeedRating).ModifiedValue, 0, float.MaxValue); } }

        /// <inheritdoc/>
        public float Crit { get { return Clamp(GetSingleAttribute(AttributeType.CritChance).ModifiedValue, 0, 1); } }

        /// <inheritdoc/>
        public float Accuracy { get { return Clamp(GetSingleAttribute(AttributeType.AttackRating).ModifiedValue, -1, 2); } }

        /// <inheritdoc/>
        public float Dodge { get { return Clamp(GetSingleAttribute(AttributeType.DefenseRating).ModifiedValue, 0, float.MaxValue); } }

        /// <inheritdoc/>
        public float Protection { get { return Clamp(GetSingleAttribute(AttributeType.ProtectionRating).ModifiedValue, 0, float.MaxValue); } }

        /// <inheritdoc/>
        public float MinDamage { get { return Clamp(GetSingleAttribute(AttributeType.DamageLow).ModifiedValue, 0, float.MaxValue); } }

        /// <inheritdoc/>
        public float MaxDamage { get { return Clamp(GetSingleAttribute(AttributeType.DamageHigh).ModifiedValue, MinDamage, float.MaxValue); } }

        /// <inheritdoc/>
        public float DamageMod { get { return GetSingleAttribute(AttributeType.DamageHigh).ModifiedValue; } }

        /// <summary>Applies a single buff rule.</summary>
        /// <param name="buffEntry">The buff entry.</param>
        /// <param name="rules">The combat rules context.</param>
        public void ApplySingleBuffRule(BattleRulesContext rules, BuffRule rule)
        {
            for (int i = 0; i < BuffInfo.Count; i++)
                if (BuffInfo[i].Buff.RuleType == rule)
                    ApplyBuffRule(BuffInfo[i], rules);
        }

        /// <summary>Adds a single attribute.</summary>
        /// <param name="stat">The attribute type.</param>
        /// <param name="attribute">The attribute.</param>
        protected void AddSingleAttribute(AttributeType stat, SingleAttribute attribute)
        {
            singleAttributes.Add(stat, attribute);
        }

        /// <summary>Adds a paired attribute.</summary>
        /// <param name="stat">The attribute type.</param>
        /// <param name="attribute">The attribute.</param>
        protected void AddPairedAttribute(AttributeType stat, PairedAttribute attribute)
        {
            pairedAttributes.Add(stat, attribute);
        }

        /// <summary>Applies the buff modifiers of a buff entry.</summary>
        /// <param name="buffEntry">The buff entry.</param>
        protected void ApplyBuff(BuffInfo buffEntry)
        {
            switch (buffEntry.Buff.Type)
            {
                case BuffType.StatAdd:
                    GetAttribute(buffEntry.Buff.AttributeType).FlatAddition += buffEntry.ModifierValue;
                    break;
                case BuffType.StatMultiply:
                    GetAttribute(buffEntry.Buff.AttributeType).Multiplier += buffEntry.ModifierValue;
                    break;
            }
        }

        /// <summary>Reverts the buff modifiers of a buff entry.</summary>
        /// <param name="buffEntry">The buff entry.</param>
        protected void RevertBuff(BuffInfo buffEntry)
        {
            switch (buffEntry.Buff.Type)
            {
                case BuffType.StatAdd:
                    GetAttribute(buffEntry.Buff.AttributeType).FlatAddition -= buffEntry.ModifierValue;
                    break;
                case BuffType.StatMultiply:
                    GetAttribute(buffEntry.Buff.AttributeType).Multiplier -= buffEntry.ModifierValue;
                    break;
            }
        }

        /// <summary>Removes a buff entry and reverts its modifiers.</summary>
        /// <param name="buffEntry">The buff entry.</param>
        protected void RemoveBuff(BuffInfo buffEntry)
        {
            BuffInfo.Remove(buffEntry);
            RevertBuff(buffEntry);
        }

        /// <summary>Applies a buff rule against the combat context.</summary>
        /// <param name="buffEntry">The buff entry.</param>
        /// <param name="rules">The combat rules context.</param>
        protected virtual void ApplyBuffRule(BuffInfo buffEntry, BattleRulesContext rules)
        {
            bool apply = false;
            switch (buffEntry.Buff.RuleType)
            {
                case BuffRule.Always:
                    apply = !buffEntry.Buff.IsFalseRule;
                    break;
                case BuffRule.Afflicted:
                    apply = RulesMatch(rules.Unit.Character.IsAfflicted, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.Virtued:
                    apply = RulesMatch(rules.Unit.Character.IsVirtued, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.DeathsDoor:
                    apply = RulesMatch(rules.Unit.Character.AtDeathsDoor, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.FirstRound:
                    apply = rules.BattleGround != null && rules.BattleGround.BattleStatus == BattleStatus.Fighting
                        && RulesMatch(rules.BattleGround.Round.RoundNumber == 0, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.HpAbove:
                    apply = RulesMatch(rules.Unit.Character.HealthRatio > buffEntry.Buff.SingleParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.HpBelow:
                    apply = RulesMatch(rules.Unit.Character.HealthRatio < buffEntry.Buff.SingleParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.StressAbove:
                    apply = RulesMatch(rules.Unit.Character.Stress.CurrentValue > buffEntry.Buff.SingleParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.StressBelow:
                    apply = RulesMatch(rules.Unit.Character.Stress.CurrentValue < buffEntry.Buff.SingleParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.InRank:
                    apply = RulesMatch(rules.Unit.Rank == buffEntry.Buff.SingleParam + 1, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.Size:
                    apply = rules.Target != null && RulesMatch(rules.Target.Size == buffEntry.Buff.SingleParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.LightAbove:
                    apply = RulesMatch(rules.TorchAmount > buffEntry.Buff.SingleParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.LightBelow:
                    apply = RulesMatch(rules.TorchAmount < buffEntry.Buff.SingleParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.Skill:
                    apply = rules.Skill != null && RulesMatch(rules.Skill.Id == buffEntry.Buff.StringParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.Melee:
                    apply = rules.Skill != null && RulesMatch(rules.Skill.Type == "melee", buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.Ranged:
                    apply = rules.Skill != null && RulesMatch(rules.Skill.Type == "ranged", buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.Status:
                    if (rules.Target != null)
                    {
                        StatusType targetStatus = StringToStatusType(buffEntry.Buff.StringParam);
                        if (targetStatus != StatusType.None)
                            apply = RulesMatch(rules.Target.Character.GetStatusEffect(targetStatus).IsApplied, buffEntry.Buff.IsFalseRule);
                    }
                    break;
                case BuffRule.EnemyType:
                    if (rules.Target != null && rules.Target.Character.IsMonster && rules.Target.Character.MonsterTypes != null)
                    {
                        MonsterType monsterType = StringToMonsterType(buffEntry.Buff.StringParam);
                        apply = RulesMatch(rules.Target.Character.MonsterTypes.Contains(monsterType), buffEntry.Buff.IsFalseRule);
                    }
                    break;
                case BuffRule.InMode:
                    apply = rules.Unit.Character.InMode
                        && RulesMatch(rules.Unit.Character.CurrentMode != null
                            && rules.Unit.Character.CurrentMode.Id == buffEntry.Buff.StringParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.Riposting:
                    apply = RulesMatch(rules.IsRiposting, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.InCamp:
                    apply = RulesMatch(rules.IsDoingCamping, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.InCorridor:
                    apply = RulesMatch(rules.IsInHall, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.InDungeon:
                    apply = rules.Dungeon != null && RulesMatch(rules.Dungeon == buffEntry.Buff.StringParam, buffEntry.Buff.IsFalseRule);
                    break;
                case BuffRule.InActivity:
                case BuffRule.WalkBack:
                    apply = false;
                    break;
            }

            if (apply)
                ApplyBuff(buffEntry);
            else
                RevertBuff(buffEntry);
        }

        private static bool RulesMatch(bool condition, bool isFalseRule)
        {
            return isFalseRule ? !condition : condition;
        }

        private static int CeilToInt(float value)
        {
            return (int)Math.Ceiling(value);
        }

        private static int RoundToInt(float value)
        {
            return (int)Math.Round(value);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private static StatusType StringToStatusType(string value)
        {
            switch (value)
            {
                case "stun": return StatusType.Stun;
                case "bleeding": return StatusType.Bleeding;
                case "poison": return StatusType.Poison;
                case "marked": return StatusType.Marked;
                case "riposte": return StatusType.Riposte;
                case "guard": return StatusType.Guard;
                case "guarded": return StatusType.Guarded;
                case "deaths_door": return StatusType.DeathsDoor;
                case "death_recovery": return StatusType.DeathRecovery;
                default: return StatusType.None;
            }
        }

        private static MonsterType StringToMonsterType(string value)
        {
            switch (value)
            {
                case "unholy": return MonsterType.Unholy;
                case "man": return MonsterType.Man;
                case "eldritch": return MonsterType.Eldritch;
                case "beast": return MonsterType.Beast;
                case "corpse": return MonsterType.Corpse;
                default: return MonsterType.None;
            }
        }
    }
}