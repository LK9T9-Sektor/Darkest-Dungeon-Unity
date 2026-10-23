using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Ui;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>The skill tile view model: icon fallback initials and tone brush.</summary>
    [TestFixture]
    public class DuelSkillViewModelTests
    {
        /// <summary>Two words give two letters, a single word gives its first two letters.</summary>
        /// <param name="displayName">The skill display name.</param>
        /// <param name="expected">The expected initials.</param>
        [TestCase("Plague Grenade", "PG")]
        [TestCase("Bulwark of Faith", "BO")]
        [TestCase("Smite", "SM")]
        [TestCase("A", "A")]
        public void Initials_AreDerivedFromDisplayName(string displayName, string expected)
        {
            var skill = new DuelSkillViewModel("id", displayName);

            Assert.That(skill.Initials, Is.EqualTo(expected));
        }

        /// <summary>The fallback fill follows the skill tone.</summary>
        [Test]
        public void ToneBrush_MatchesTone()
        {
            var attack = new DuelSkillViewModel("id", "Smite", SkillTone.Attack);
            var heal = new DuelSkillViewModel("id", "Battle Heal", SkillTone.Heal);
            var buff = new DuelSkillViewModel("id", "Inspiring Cry", SkillTone.Buff);

            Assert.That(attack.ToneBrush, Is.SameAs(SkillToneClassifier.AttackBrush));
            Assert.That(heal.ToneBrush, Is.SameAs(SkillToneClassifier.HealBrush));
            Assert.That(buff.ToneBrush, Is.SameAs(SkillToneClassifier.BuffBrush));
        }
    }
}
