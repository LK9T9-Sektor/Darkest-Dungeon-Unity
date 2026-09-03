using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Wpf.Ui;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Tests for the buff name/description/duration text derivation.</summary>
    [TestFixture]
    public class BuffDetailsTests
    {
        [Test]
        public void FormatName_WithContentId_ReturnsTheIdTitle()
        {
            var buff = new Buff(BuffType.StatAdd, AttributeType.AttackRating, 0.06f) { Id = "bleed_debuff_1" };

            Assert.That(BuffDetails.FormatName(buff), Is.EqualTo("Bleed Debuff 1"));
        }

        [Test]
        public void FormatName_WithoutContentId_FallsBackToTheAttributeLabel()
        {
            var buff = new Buff(BuffType.StatAdd, AttributeType.AttackRating, 0.06f);

            Assert.That(BuffDetails.FormatName(buff), Is.EqualTo("Accuracy"));
        }

        [Test]
        public void FormatDescription_AdditivePercent_FormatsSignedPercentage()
        {
            var buff = new Buff(BuffType.StatAdd, AttributeType.AttackRating, 0.06f);

            Assert.That(BuffDetails.FormatDescription(buff), Is.EqualTo("+6% Accuracy"));
        }

        [Test]
        public void FormatDescription_AdditiveNegativePercent_KeepsTheMinusSign()
        {
            var buff = new Buff(BuffType.StatAdd, AttributeType.Stun, -0.15f);

            Assert.That(BuffDetails.FormatDescription(buff), Is.EqualTo("-15% Stun Resist"));
        }

        [Test]
        public void FormatDescription_AdditiveFlatValue_FormatsThePlainNumber()
        {
            var buff = new Buff(BuffType.StatAdd, AttributeType.HitPoints, 5f);

            Assert.That(BuffDetails.FormatDescription(buff), Is.EqualTo("+5 Max HP"));
        }

        [Test]
        public void FormatDescription_Multiply_ShowsTheMultiplier()
        {
            var buff = new Buff(BuffType.StatMultiply, AttributeType.DamageHigh, 1.12f);

            Assert.That(BuffDetails.FormatDescription(buff), Is.EqualTo("x1.12 Max Damage"));
        }

        [Test]
        public void FormatDuration_Round_ShowsTheRemainingRounds()
        {
            var buff = new Buff(BuffType.StatAdd, AttributeType.AttackRating, 0.06f);
            var info = new BuffInfo(buff, BuffDurationType.Round, BuffSourceType.Adventure, 3);

            Assert.That(BuffDetails.FormatDuration(info), Is.EqualTo("3 rounds"));
        }

        [Test]
        public void FormatDuration_RoundSingle_StaysSingular()
        {
            var buff = new Buff(BuffType.StatAdd, AttributeType.AttackRating, 0.06f);
            var info = new BuffInfo(buff, BuffDurationType.Round, BuffSourceType.Adventure, 1);

            Assert.That(BuffDetails.FormatDuration(info), Is.EqualTo("1 round"));
        }

        [Test]
        public void FormatDuration_CombatAndPermanent_ReturnTheirLabels()
        {
            var buff = new Buff(BuffType.StatAdd, AttributeType.AttackRating, 0.06f);
            var combat = new BuffInfo(buff, BuffDurationType.Combat, BuffSourceType.Adventure);
            var permanent = new BuffInfo(buff, BuffDurationType.Permanent, BuffSourceType.Adventure);

            Assert.That(BuffDetails.FormatDuration(combat), Is.EqualTo("Combat"));
            Assert.That(BuffDetails.FormatDuration(permanent), Is.EqualTo("Permanent"));
        }

        [Test]
        public void FormatTexts_NullBuff_ReturnEmptyStrings()
        {
            Assert.That(BuffDetails.FormatName(null), Is.EqualTo(string.Empty));
            Assert.That(BuffDetails.FormatDescription(null), Is.EqualTo(string.Empty));
            Assert.That(BuffDetails.FormatDuration(null), Is.EqualTo(string.Empty));
        }
    }
}