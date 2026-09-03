using NSubstitute;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Battle;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects;

namespace Sektor.DarkestDungeon.Core.Combat.Tests.Mechanics
{
    [TestFixture]
    public class DiseaseEffectTests
    {
        [Test]
        public void ApplyQueued_SpecificDisease_ResolvesAndAppliesTheQuirk()
        {
            var attribute = Substitute.For<IAttribute>();
            attribute.ModifiedValue.Returns(0f);

            var quirk = Substitute.For<IQuirk>();
            quirk.Id.Returns("the_worries");

            var character = Substitute.For<ICharacter>();
            character.IsMonster.Returns(false);
            character.GetSingleAttribute(AttributeType.Disease).Returns(attribute);
            character.AddQuirk(quirk).Returns(true);

            var target = Substitute.For<ICombatUnit>();
            target.Character.Returns(character);

            var events = Substitute.For<IBattleEvents>();
            var context = Substitute.For<IBattleContext>();
            context.Events.Returns(events);
            context.GetQuirk("the_worries").Returns(quirk);

            var diseaseEffect = new DiseaseEffect("the_worries");

            bool applied = diseaseEffect.ApplyQueued(Substitute.For<ICombatUnit>(), target, MakeEffect(), context);

            Assert.That(applied, Is.True);
            character.Received(1).AddQuirk(quirk);
            events.Received(1).ShowPopup(target, PopupType.Disease, "the_worries");
            events.Received(1).SetHalo(target, "disease");
        }

        [Test]
        public void ApplyQueued_DiseaseResist_FailsAndShowsResistPopup()
        {
            var attribute = Substitute.For<IAttribute>();
            attribute.ModifiedValue.Returns(1f);

            var character = Substitute.For<ICharacter>();
            character.IsMonster.Returns(false);
            character.GetSingleAttribute(AttributeType.Disease).Returns(attribute);

            var target = Substitute.For<ICombatUnit>();
            target.Character.Returns(character);

            var events = Substitute.For<IBattleEvents>();
            var context = Substitute.For<IBattleContext>();
            context.Events.Returns(events);

            var diseaseEffect = new DiseaseEffect("the_worries");

            bool applied = diseaseEffect.ApplyQueued(Substitute.For<ICombatUnit>(), target, MakeEffect(), context);

            Assert.That(applied, Is.False);
            events.Received(1).ShowPopup(target, PopupType.DiseaseResist, Arg.Any<string>());
            character.DidNotReceive().AddQuirk(Arg.Any<IQuirk>());
        }

        [Test]
        public void ApplyQueued_RandomDisease_WithEmptyPool_DoesNotCrashAndAppliesNothing()
        {
            var attribute = Substitute.For<IAttribute>();
            attribute.ModifiedValue.Returns(0f);

            var character = Substitute.For<ICharacter>();
            character.IsMonster.Returns(false);
            character.GetSingleAttribute(AttributeType.Disease).Returns(attribute);
            character.AddRandomDisease().Returns((IQuirk)null);

            var target = Substitute.For<ICombatUnit>();
            target.Character.Returns(character);

            var events = Substitute.For<IBattleEvents>();
            var context = Substitute.For<IBattleContext>();
            context.Events.Returns(events);

            var diseaseEffect = new DiseaseEffect(null);

            bool applied = diseaseEffect.ApplyQueued(Substitute.For<ICombatUnit>(), target, MakeEffect(), context);

            Assert.That(applied, Is.False);
            character.DidNotReceive().AddQuirk(Arg.Any<IQuirk>());
            events.DidNotReceive().ShowPopup(Arg.Any<ICombatUnit>(), Arg.Any<PopupType>(), Arg.Any<string>());
        }

        [Test]
        public void ApplyQueued_RandomDisease_AppliesWhenTheCharacterReturnsOne()
        {
            var attribute = Substitute.For<IAttribute>();
            attribute.ModifiedValue.Returns(0f);

            var quirk = Substitute.For<IQuirk>();
            quirk.Id.Returns("rabies");

            var character = Substitute.For<ICharacter>();
            character.IsMonster.Returns(false);
            character.GetSingleAttribute(AttributeType.Disease).Returns(attribute);
            character.AddRandomDisease().Returns(quirk);
            character.AddQuirk(quirk).Returns(true);

            var target = Substitute.For<ICombatUnit>();
            target.Character.Returns(character);

            var events = Substitute.For<IBattleEvents>();
            var context = Substitute.For<IBattleContext>();
            context.Events.Returns(events);

            var diseaseEffect = new DiseaseEffect(null);

            bool applied = diseaseEffect.ApplyQueued(Substitute.For<ICombatUnit>(), target, MakeEffect(), context);

            Assert.That(applied, Is.True);
            character.Received(1).AddQuirk(quirk);
            events.Received(1).ShowPopup(target, PopupType.Disease, "rabies");
        }

        [Test]
        public void ApplyQueued_MonsterTarget_IsIgnored()
        {
            var character = Substitute.For<ICharacter>();
            character.IsMonster.Returns(true);

            var target = Substitute.For<ICombatUnit>();
            target.Character.Returns(character);

            var events = Substitute.For<IBattleEvents>();
            var context = Substitute.For<IBattleContext>();
            context.Events.Returns(events);

            var diseaseEffect = new DiseaseEffect("the_worries");

            bool applied = diseaseEffect.ApplyQueued(Substitute.For<ICombatUnit>(), target, MakeEffect(), context);

            Assert.That(applied, Is.False);
            events.DidNotReceive().ShowPopup(Arg.Any<ICombatUnit>(), Arg.Any<PopupType>(), Arg.Any<string>());
        }

        [Test]
        public void ApplyInstant_SpecificDisease_AppliesWithoutFeedback()
        {
            var attribute = Substitute.For<IAttribute>();
            attribute.ModifiedValue.Returns(0f);

            var quirk = Substitute.For<IQuirk>();
            quirk.Id.Returns("the_worries");

            var character = Substitute.For<ICharacter>();
            character.IsMonster.Returns(false);
            character.GetSingleAttribute(AttributeType.Disease).Returns(attribute);
            character.AddQuirk(quirk).Returns(true);

            var target = Substitute.For<ICombatUnit>();
            target.Character.Returns(character);

            var events = Substitute.For<IBattleEvents>();
            var context = Substitute.For<IBattleContext>();
            context.Events.Returns(events);
            context.GetQuirk("the_worries").Returns(quirk);

            var diseaseEffect = new DiseaseEffect("the_worries");

            bool applied = diseaseEffect.ApplyInstant(Substitute.For<ICombatUnit>(), target, MakeEffect(), context);

            Assert.That(applied, Is.True);
            character.Received(1).AddQuirk(quirk);
            events.DidNotReceive().ShowPopup(Arg.Any<ICombatUnit>(), Arg.Any<PopupType>(), Arg.Any<string>());
        }

        private static Effect MakeEffect()
        {
            var effect = new Effect { TargetType = EffectTargetType.Target };
            effect.IntegerParams[EffectIntParams.Chance] = 100;
            return effect;
        }
    }
}