namespace Sektor.DarkestDungeon.Core.Combat.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Core.Combat.Mechanics;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
    using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects;

    [TestFixture]
    public class EffectCatalogTests
    {
        [Test]
        public void Load_ParsesCommonNonBuffEffectKeys()
        {
            var catalog = EffectCatalog.Load(
                "effect: .name \"Stun 1\" .target \"target\" .chance 100% .stun 1\n" +
                "effect: .name \"Bleed 2\" .target \"target\" .chance 110% .dotBleed 2 .duration 3\n" +
                "effect: .name \"PD Blight 1\" .target \"target\" .chance 100% .dotPoison 4 .duration 3\n" +
                "effect: .name \"HealSelf 1\" .target \"performer\" .chance 100% .heal 3\n" +
                "effect: .name \"Pull 2A\" .target \"target\" .pull 2 .chance 100%\n" +
                "effect: .name \"Push 1A\" .target \"target\" .push 1\n" +
                "effect: .name \"Cure 1\" .target \"performer\" .cure 1\n" +
                "effect: .name \"Riposte 1\" .target \"performer\" .riposte 1 .duration 2\n" +
                "effect: .name \"Shuffle Party\" .target \"global\" .shuffleparty 1\n" +
                "effect: .name \"Mark 1\" .target \"target\" .tag 1 .duration 3\n" +
                "effect: .name \"Stress 2\" .target \"target\" .chance 100% .stress 15\n" +
                "effect: .name \"Calm 1\" .target \"performer\" .healstress 4");

            Assert.That(catalog.Count, Is.EqualTo(12));
            AssertSubEffect<StunEffect>(catalog.Get("Stun 1"));
            AssertSubEffect<BleedEffect>(catalog.Get("bleed 2"));
            AssertSubEffect<PoisonEffect>(catalog.Get("PD Blight 1"));
            AssertSubEffect<HealEffect>(catalog.Get("HealSelf 1"));
            AssertSubEffect<PullEffect>(catalog.Get("Pull 2A"));
            AssertSubEffect<PushEffect>(catalog.Get("Push 1A"));
            AssertSubEffect<CureEffect>(catalog.Get("Cure 1"));
            AssertSubEffect<RiposteEffect>(catalog.Get("Riposte 1"));
            AssertSubEffect<ShuffleTargetEffect>(catalog.Get("Shuffle Party"));
            AssertSubEffect<TagEffect>(catalog.Get("Mark 1"));
            AssertSubEffect<StressEffect>(catalog.Get("Stress 2"));
            AssertSubEffect<StressHealEffect>(catalog.Get("Calm 1"));

            var bleed = catalog.Get("Bleed 2");
            Assert.That(bleed.IntegerParams[EffectIntParams.Duration], Is.EqualTo(3));
        }

        [Test]
        public void Load_IgnoresUnsupportedBuffKeys_UntilStatBuffStorageLands()
        {
            var catalog = EffectCatalog.Load(
                "effect: .name \"Highwayman Buff 1\" .target \"performer\" .chance 100% .combat_stat_buff 1 .attack_rating_add 6% .crit_chance_add 5% .damage_low_multiply 12% .damage_high_multiply 12%");

            Assert.That(catalog.Count, Is.EqualTo(0));
        }

        private static void AssertSubEffect<T>(Effect effect)
            where T : SubEffect
        {
            Assert.That(effect, Is.Not.Null);
            Assert.That(effect.SubEffects.Any(sub => sub is T), Is.True);
        }
    }
}