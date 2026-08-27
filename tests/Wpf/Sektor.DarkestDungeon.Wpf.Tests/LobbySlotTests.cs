namespace Sektor.DarkestDungeon.Wpf.Tests
{
    using System.Linq;

    using NUnit.Framework;

    using Sektor.DarkestDungeon.Wpf.Combat;
    using Sektor.DarkestDungeon.Wpf.ViewModels;

    [TestFixture]
    public class LobbySlotTests
    {
        [Test]
        public void Slot_LoadsActiveSkillsAndQuirks()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);

            Assert.That(slot.Skills.Count, Is.GreaterThan(0));
            Assert.That(slot.MaxActiveSkills, Is.GreaterThan(0));
            Assert.That(slot.Skills.Count(skill => skill.IsActive), Is.EqualTo(slot.MaxActiveSkills));
            Assert.That(slot.SelectedSkillIds.Count, Is.EqualTo(slot.MaxActiveSkills));
            Assert.That(slot.Quirks.Count, Is.EqualTo(2));
            Assert.That(slot.QuirkSummary, Is.Not.Empty);
            Assert.That(slot.Details, Does.Contain("HP"));
        }

        [Test]
        public void Slot_ToggleSkill_RespectsMaxActive()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);
            var active = slot.Skills.Where(skill => skill.IsActive).ToList();
            var inactive = slot.Skills.Where(skill => !skill.IsActive).FirstOrDefault();
            if (inactive == null)
            {
                Assert.Pass("All class skills are active; nothing to toggle.");
                return;
            }

            slot.ToggleSkillCommand.Execute(inactive);
            Assert.That(slot.Skills.Count(skill => skill.IsActive), Is.EqualTo(slot.MaxActiveSkills));

            slot.ToggleSkillCommand.Execute(active[0]);
            Assert.That(slot.Skills.Count(skill => skill.IsActive), Is.EqualTo(slot.MaxActiveSkills - 1));

            slot.ToggleSkillCommand.Execute(inactive);
            Assert.That(slot.Skills.Count(skill => skill.IsActive), Is.EqualTo(slot.MaxActiveSkills));
        }

        [Test]
        public void Slot_RerollQuirks_ReplacesQuirks()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);
            string before = slot.QuirkSummary;

            slot.RerollQuirksCommand.Execute(null);

            Assert.That(slot.Quirks.Count, Is.EqualTo(2));
            Assert.That(slot.QuirkSummary, Is.Not.Empty);
            Assert.That(slot.QuirkSummary, Is.EqualTo(before).Or.Not.EqualTo(before));
        }
    }
}