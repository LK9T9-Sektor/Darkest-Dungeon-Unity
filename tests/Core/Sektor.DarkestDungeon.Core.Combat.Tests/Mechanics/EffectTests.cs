namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics
{
    using System.Collections.Generic;

    using NSubstitute;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
    using Sektor.DarkestDungeon.Core.Combat.Raid.Events;

    [TestFixture]
    public class EffectTests
    {
        private static ICombatUnit MakeUnit()
        {
            var unit = Substitute.For<ICombatUnit>();
            unit.EventQueue.Returns(new List<IEffectEvent>());
            return unit;
        }

        [Test]
        public void Apply_OnMissFalse_WithMissResult_SkipsEffects()
        {
            var performer = MakeUnit();
            var target = MakeUnit();
            var battleContext = Substitute.For<IBattleContext>();
            var subEffect = new RecordingSubEffect();

            var effect = new Effect { TargetType = EffectTargetType.Target };
            effect.BooleanParams[EffectBoolParams.OnMiss] = false;
            effect.SubEffects.Add(subEffect);

            var skillResult = new SkillResult();
            skillResult.AddResultEntry(new SkillResultEntry(target, SkillResultType.Miss));

            effect.Apply(performer, target, skillResult, battleContext);

            Assert.That(subEffect.InstantCalls, Is.Zero);
            Assert.That(target.EventQueue, Is.Empty);
        }

        [Test]
        public void Apply_OnMissTrue_WithHitResult_AppliesEffect()
        {
            var performer = MakeUnit();
            var target = MakeUnit();
            var battleContext = Substitute.For<IBattleContext>();
            var subEffect = new RecordingSubEffect();

            var effect = new Effect { TargetType = EffectTargetType.Target };
            effect.BooleanParams[EffectBoolParams.OnMiss] = false;
            effect.SubEffects.Add(subEffect);

            var skillResult = new SkillResult();
            skillResult.AddResultEntry(new SkillResultEntry(target, 5, SkillResultType.Hit));

            effect.Apply(performer, target, skillResult, battleContext);

            Assert.That(target.EventQueue, Has.Count.EqualTo(1));
            Assert.That(skillResult.AppliedEffects, Contains.Item(effect));
        }

        [Test]
        public void Apply_ApplyOnce_SecondApplicationIsSkipped()
        {
            var performer = MakeUnit();
            var target = MakeUnit();
            var battleContext = Substitute.For<IBattleContext>();
            var subEffect = new RecordingSubEffect();

            var effect = new Effect { TargetType = EffectTargetType.Target };
            effect.BooleanParams[EffectBoolParams.ApplyOnce] = true;
            effect.SubEffects.Add(subEffect);

            var skillResult = new SkillResult();
            skillResult.AddResultEntry(new SkillResultEntry(target, 5, SkillResultType.Hit));

            effect.Apply(performer, target, skillResult, battleContext);
            effect.Apply(performer, target, skillResult, battleContext);

            Assert.That(target.EventQueue, Has.Count.EqualTo(1));
        }

        [Test]
        public void Apply_GlobalTarget_WithTorchParam_CallsTorchEvent()
        {
            var performer = MakeUnit();
            var target = MakeUnit();
            var events = Substitute.For<IBattleEvents>();
            var battleContext = Substitute.For<IBattleContext>();
            battleContext.Events.Returns(events);

            var effect = new Effect { TargetType = EffectTargetType.Global };
            effect.IntegerParams[EffectIntParams.Torch] = 5;
            effect.SubEffects.Add(new RecordingSubEffect());

            var skillResult = new SkillResult();
            skillResult.AddResultEntry(new SkillResultEntry(target, SkillResultType.Utility));

            effect.Apply(performer, target, skillResult, battleContext);

            events.Received(1).IncreaseTorch(5);
        }
    }
}