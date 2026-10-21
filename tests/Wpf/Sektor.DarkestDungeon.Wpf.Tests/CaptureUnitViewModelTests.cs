using NUnit.Framework;

using Sektor.DarkestDungeon.Core.GameEvents;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Verifies the captured unit projection used by the replay formation.</summary>
    [TestFixture]
    public class CaptureUnitViewModelTests
    {
        /// <summary>A zero-HP hero at Death's Door is alive, even when an older capture marked it dead.</summary>
        [Test]
        public void IsDead_ZeroHpAtDeathsDoor_IsFalse()
        {
            var state = new CombatantState(
                "hero_1", "crusader", 0, 33, false, "crusader_portrait", "crusader",
                Rank: 1, AtDeathsDoor: true, IsDead: true);

            var unit = new CaptureUnitViewModel(state);

            Assert.That(unit.IsDead, Is.False);
            Assert.That(unit.ConditionText, Is.EqualTo("СМЕРТНЫЙ ПОРОГ"));
        }

        /// <summary>A unit recorded dead outside Death's Door is still dead.</summary>
        [Test]
        public void IsDead_CapturedDeadWithoutDeathsDoor_IsTrue()
        {
            var state = new CombatantState(
                "monster_3", "brigand_cutthroat", 0, 12, true, "brigand_portrait", "brigand_cutthroat",
                Rank: 1, IsDead: true);

            var unit = new CaptureUnitViewModel(state);

            Assert.That(unit.IsDead, Is.True);
        }
    }
}
