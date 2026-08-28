using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Combat.Character.Components;
using Sektor.DarkestDungeon.Core.Combat.Character.Statuses;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;

namespace Sektor.DarkestDungeon.Core.Combat.Character
{
    /// <summary>A hero character with class stats, stress, trait and combat skills.</summary>
    public class Hero : Character
    {
        private static readonly AttributeType[] HeroResistances = new AttributeType[]
        {
            AttributeType.Stun, AttributeType.Poison, AttributeType.Disease,
            AttributeType.DeathBlow, AttributeType.Move, AttributeType.Bleed,
            AttributeType.Debuff, AttributeType.Trap,
        };

        /// <summary>Gets the hero class.</summary>
        public HeroClass HeroClass { get; set; }

        /// <summary>Gets the hero's generated name.</summary>
        public string HeroName { get; private set; }

        /// <summary>Gets the class string id.</summary>
        public string ClassStringId { get; private set; }

        /// <summary>Gets or sets the hero's trait (affliction/virtue).</summary>
        public Trait Trait { get; set; }

        /// <summary>Gets or sets the resolve.</summary>
        public Resolve Resolve { get; set; }

        /// <inheritdoc/>
        public override string Name { get { return HeroName; } }

        /// <inheritdoc/>
        public override string Class { get { return ClassStringId; } }

        /// <inheritdoc/>
        public override bool AtDeathsDoor { get { return GetStatusEffect(StatusType.DeathsDoor).IsApplied; } }

        /// <inheritdoc/>
        public override bool IsStressed { get { return Stress.CurrentValue >= 50; } }

        /// <inheritdoc/>
        public override bool IsOverstressed { get { return Stress.CurrentValue >= 100; } }

        /// <inheritdoc/>
        public override bool IsVirtued { get { return Trait != null && Trait.IsVirtue; } }

        /// <inheritdoc/>
        public override bool IsAfflicted { get { return Trait != null && Trait.IsAffliction; } }

        /// <inheritdoc/>
        public override bool IsReligious { get { return HeroClass.IsReligious; } }

        /// <inheritdoc/>
        public override List<ICharacterMode> Modes
        {
            get { return HeroClass.Modes.Cast<ICharacterMode>().ToList(); }
        }

        /// <inheritdoc/>
        public override List<SkillArtInfo> SkillArtInfo { get { return HeroClass.SkillArtInfo; } }

        /// <inheritdoc/>
        public override CombatSkill RiposteSkill { get { return HeroClass.RiposteSkill; } }

        /// <inheritdoc/>
        public override List<CombatSkill> CurrentCombatSkills
        {
            get
            {
                return SelectedCombatSkills.Count > 0 ? SelectedCombatSkills : HeroClass.CombatSkills;
            }
        }

        /// <summary>Gets the active combat skills selected by the player (empty means all class skills).</summary>
        public List<CombatSkill> SelectedCombatSkills { get; } = new List<CombatSkill>();

        /// <summary>Gets the quirk ids applied to the hero.</summary>
        public List<string> Quirks { get; } = new List<string>();

        /// <summary>Records a quirk id on the hero.</summary>
        /// <param name="quirkId">The quirk id.</param>
        public void AddQuirk(string quirkId)
        {
            if (!Quirks.Contains(quirkId))
                Quirks.Add(quirkId);
        }

        /// <summary>Sets the active combat skills from the given ids (only skills known to the class).</summary>
        /// <param name="skillIds">The selected skill ids.</param>
        public void SelectCombatSkills(IEnumerable<string> skillIds)
        {
            SelectedCombatSkills.Clear();
            if (skillIds == null)
                return;

            foreach (var skillId in skillIds)
            {
                var skill = HeroClass.CombatSkills.FirstOrDefault(candidate => candidate.Id == skillId);
                if (skill != null && !SelectedCombatSkills.Contains(skill))
                    SelectedCombatSkills.Add(skill);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="Hero"/> class.</summary>
        /// <param name="heroClass">The hero class.</param>
        /// <param name="level">The resolve level.</param>
        /// <param name="name">The generated hero name.</param>
        public Hero(HeroClass heroClass, int level, string name)
        {
            HeroClass = heroClass;
            HeroName = name;
            ClassStringId = heroClass.StringId;
            Resolve = new Resolve(level, 0);

            this[AttributeType.HitPoints, true].RawValue = heroClass.Attributes[AttributeType.HitPoints];
            this[AttributeType.HitPoints, true].CurrentValue = this[AttributeType.HitPoints, true].ModifiedValue;

            foreach (var stat in new[]
            {
                AttributeType.DefenseRating, AttributeType.ProtectionRating, AttributeType.SpeedRating,
                AttributeType.AttackRating, AttributeType.CritChance, AttributeType.DamageLow, AttributeType.DamageHigh,
            })
                if (heroClass.Attributes.ContainsKey(stat))
                    this[stat].RawValue = heroClass.Attributes[stat];

            foreach (var resistance in HeroResistances)
            {
                float value = heroClass.Resistances.ContainsKey(resistance) ? heroClass.Resistances[resistance] : 0;
                if (resistance == AttributeType.DeathBlow)
                    AddSingleAttribute(resistance, new SingleAttribute(value));
                else
                    AddSingleAttribute(resistance, new SingleAttribute(value + level * 0.1f));
            }

            AddPairedAttribute(AttributeType.Stress, new PairedAttribute(0, 100, true));
        }

        /// <inheritdoc/>
        public override void RevertTrait()
        {
            Trait = null;
            for (int i = BuffInfo.Count - 1; i >= 0; i--)
            {
                if (BuffInfo[i].SourceType == BuffSourceType.Trait)
                    RemoveBuff(BuffInfo[i]);
            }
        }

        /// <summary>Applies an overstress trait (affliction or virtue) and its permanent buffs.</summary>
        /// <param name="trait">The trait to apply.</param>
        /// <param name="buffs">The trait's buffs (resolved from content).</param>
        public void ApplyTrait(Trait trait, IReadOnlyList<Buff> buffs)
        {
            if (Trait != null)
                RevertTrait();
            Trait = trait;
            foreach (var buff in buffs)
                AddBuff(new BuffInfo(buff, BuffDurationType.Permanent, BuffSourceType.Trait));
        }
    }
}