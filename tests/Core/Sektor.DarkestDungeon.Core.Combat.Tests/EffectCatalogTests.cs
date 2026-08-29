using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills.Effects;

namespace Sektor.DarkestDungeon.Core.Combat.Tests
{
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
        public void Load_ParsesStatBuffsAndRiposteStatMods()
        {
            var catalog = EffectCatalog.Load(
                "effect: .name \"Highwayman Buff 1\" .target \"performer\" .chance 100% .combat_stat_buff 1 .attack_rating_add 6% .crit_chance_add 5% .damage_low_multiply 12% .damage_high_multiply 12% .duration 3\n" +
                "effect: .name \"Vestal Curse 1\" .target \"target\" .chance 100% .combat_stat_buff 1 .attack_rating_add -7% .damage_low_multiply -20% .damage_high_multiply -20%\n" +
                "effect: .name \"Hwy Riposte 1\" .target \"performer\" .riposte 1 .duration 2 .damage_low_multiply -40% .damage_high_multiply -40%");

            var buff = catalog.Get("Highwayman Buff 1");
            Assert.That(buff, Is.Not.Null);
            var statBuff = buff.SubEffects.OfType<CombatStatBuffEffect>().Single();
            Assert.That(statBuff.StatAddBuffs[AttributeType.AttackRating], Is.EqualTo(0.06f).Within(0.0001f));
            Assert.That(statBuff.StatAddBuffs[AttributeType.CritChance], Is.EqualTo(0.05f).Within(0.0001f));
            Assert.That(statBuff.StatMultBuffs[AttributeType.DamageLow], Is.EqualTo(0.12f).Within(0.0001f));
            Assert.That(statBuff.StatMultBuffs[AttributeType.DamageHigh], Is.EqualTo(0.12f).Within(0.0001f));
            Assert.That(buff.IntegerParams[EffectIntParams.Duration], Is.EqualTo(3));

            var curse = catalog.Get("Vestal Curse 1");
            var curseBuff = curse.SubEffects.OfType<CombatStatBuffEffect>().Single();
            Assert.That(curseBuff.StatAddBuffs[AttributeType.AttackRating], Is.EqualTo(-0.07f).Within(0.0001f));
            Assert.That(curseBuff.StatMultBuffs[AttributeType.DamageLow], Is.EqualTo(-0.2f).Within(0.0001f));

            var riposte = catalog.Get("Hwy Riposte 1");
            var riposteEffect = riposte.SubEffects.OfType<RiposteEffect>().Single();
            Assert.That(riposteEffect.StatMultBuffs[AttributeType.DamageLow], Is.EqualTo(-0.4f).Within(0.0001f));
        }

        [Test]
        public void Load_ParsesBuffIdKeys()
        {
            var catalog = EffectCatalog.Load(
                "effect: .name \"Bleed Resist Buff\" .target \"performer\" .chance 100% .buff_ids \"buff_bleed_resist_1\" \"buff_bleed_resist_2\" .duration 3");

            var effect = catalog.Get("Bleed Resist Buff");
            Assert.That(effect, Is.Not.Null);
            var buffEffect = effect.SubEffects.OfType<BuffEffect>().Single();
            CollectionAssert.AreEquivalent(new[] { "buff_bleed_resist_1", "buff_bleed_resist_2" }, buffEffect.BuffIds);
        }

        [Test]
        public void Load_ParsesTorchKeys()
        {
            var catalog = EffectCatalog.Load(
                "effect: .name \"Darkness 1\" .target \"global\" .chance 100% .torch_decrease 5\n" +
                "effect: .name \"Light 1\" .target \"global\" .chance 100% .torch_increase 6\n" +
                "effect: .name \"Net Change\" .target \"global\" .torch_decrease 2 .torch_increase 7");

            var darkness = catalog.Get("Darkness 1");
            Assert.That(darkness, Is.Not.Null);
            Assert.That(darkness.TargetType, Is.EqualTo(EffectTargetType.Global));
            Assert.That(darkness.IntegerParams[EffectIntParams.Torch], Is.EqualTo(-5));

            var light = catalog.Get("Light 1");
            Assert.That(light.IntegerParams[EffectIntParams.Torch], Is.EqualTo(6));

            var net = catalog.Get("Net Change");
            Assert.That(net.IntegerParams[EffectIntParams.Torch], Is.EqualTo(5));
        }

        [Test]
        public void Load_ParsesSetMode()
        {
            var catalog = EffectCatalog.Load(
                "effect: .name \"Switch Beast\" .target \"performer\" .set_mode beast");

            var effect = catalog.Get("Switch Beast");
            Assert.That(effect, Is.Not.Null);
            var setMode = effect.SubEffects.OfType<SetModeEffect>().Single();
            Assert.That(setMode.Mode, Is.EqualTo("beast"));
        }

        private static void AssertSubEffect<T>(Effect effect)
            where T : SubEffect
        {
            Assert.That(effect, Is.Not.Null);
            Assert.That(effect.SubEffects.Any(sub => sub is T), Is.True);
        }
    }
}