using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Tests for the skill tooltip effect lines (buffs/debuffs a skill applies).</summary>
    [TestFixture]
    public class SkillDetailsTests
    {
        [Test]
        public void BuildEffects_StunSkill_ShowsTheStunStatus()
        {
            var skill = SkillWith(EffectTargetType.Target, new StunEffect());

            var lines = SkillDetails.BuildEffects(skill);

            Assert.That(lines, Does.Contain("Stun"));
        }

        [Test]
        public void BuildEffects_BleedSkill_ShowsAmountAndRounds()
        {
            var effect = new Effect { TargetType = EffectTargetType.Target };
            effect.IntegerParams[EffectIntParams.Duration] = 2;
            effect.SubEffects.Add(new BleedEffect(3));

            var skill = new CombatSkill();
            skill.Effects.Add(effect);

            var lines = SkillDetails.BuildEffects(skill);

            Assert.That(lines, Does.Contain("Bleed 3 (2 rounds)"));
        }

        [Test]
        public void BuildEffects_StatBuffSkill_ShowsBuffAndDebuffLines()
        {
            var effect = new Effect { TargetType = EffectTargetType.Target };
            var statBuff = new CombatStatBuffEffect();
            statBuff.StatAddBuffs[AttributeType.AttackRating] = 0.06f;
            statBuff.StatAddBuffs[AttributeType.Stun] = -0.15f;
            effect.SubEffects.Add(statBuff);

            var skill = new CombatSkill();
            skill.Effects.Add(effect);

            var lines = SkillDetails.BuildEffects(skill);

            Assert.That(lines, Does.Contain("Buff: +6% Accuracy"));
            Assert.That(lines, Does.Contain("Debuff: -15% Stun Resist"));
        }

        [Test]
        public void BuildEffects_ContentBuffId_ResolvesTheBuffDescription()
        {
            var effect = new Effect { TargetType = EffectTargetType.Target };
            var buffEffect = new BuffEffect();
            buffEffect.BuffIds.Add("bleed_debuff_1");
            effect.SubEffects.Add(buffEffect);

            var skill = new CombatSkill();
            skill.Effects.Add(effect);

            var lines = SkillDetails.BuildEffects(skill);

            Assert.That(lines, Does.Contain("Debuff: -20% Bleed Resist"));
        }

        [Test]
        public void BuildEffects_SelfTargetedEffect_IsAnnotated()
        {
            var effect = new Effect { TargetType = EffectTargetType.Performer };
            effect.SubEffects.Add(new StunEffect());

            var skill = new CombatSkill();
            skill.Effects.Add(effect);

            var lines = SkillDetails.BuildEffects(skill);

            Assert.That(lines, Does.Contain("(self) Stun"));
        }

        [Test]
        public void BuildEffectRows_StunSkill_ShowsTheStunRowAsDebuff()
        {
            var skill = SkillWith(EffectTargetType.Target, new StunEffect());

            var rows = SkillDetails.BuildEffectRows(skill);

            Assert.That(rows, Has.Count.EqualTo(1));
            Assert.That(rows[0].Name, Is.EqualTo("Stun"));
            Assert.That(rows[0].Tone, Is.EqualTo("Debuff"));
        }

        [Test]
        public void BuildEffectRows_StatBuffSkill_ShowsBuffAndDebuffRows()
        {
            var effect = new Effect { TargetType = EffectTargetType.Target };
            var statBuff = new CombatStatBuffEffect();
            statBuff.StatAddBuffs[AttributeType.AttackRating] = 0.06f;
            statBuff.StatAddBuffs[AttributeType.Stun] = -0.15f;
            effect.SubEffects.Add(statBuff);

            var skill = new CombatSkill();
            skill.Effects.Add(effect);

            var rows = SkillDetails.BuildEffectRows(skill);

            Assert.That(rows.Any(row => row.Name == "Buff" && row.Description == "+6% Accuracy"), Is.True);
            Assert.That(rows.Any(row => row.Name == "Debuff" && row.Description == "-15% Stun Resist"), Is.True);
        }

        [Test]
        public void BuildBaseInfo_DamageSkill_ContainsStatsButNoEffects()
        {
            var skill = new CombatSkill { Category = SkillCategory.Damage, Accuracy = 0.85f, DamageMin = 4, DamageMax = 9 };
            skill.LaunchRanks = new FormationSet("12");
            skill.TargetRanks = new FormationSet("12");

            string baseInfo = SkillDetails.BuildBaseInfo(skill);

            Assert.That(baseInfo, Does.Contain("Damage 4-9"));
            Assert.That(baseInfo, Does.Contain("ACC 85%"));
            Assert.That(baseInfo, Does.Not.Contain("Effects:"));
        }

        private static CombatSkill SkillWith(EffectTargetType targetType, params SubEffect[] subEffects)
        {
            var effect = new Effect { TargetType = targetType };
            foreach (var subEffect in subEffects)
                effect.SubEffects.Add(subEffect);

            var skill = new CombatSkill();
            skill.Effects.Add(effect);
            return skill;
        }
    }
}