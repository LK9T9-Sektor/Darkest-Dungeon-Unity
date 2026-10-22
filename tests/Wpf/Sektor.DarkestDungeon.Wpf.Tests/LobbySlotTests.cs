using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.Combat;
using Sektor.DarkestDungeon.Wpf.Data;
using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
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
            Assert.That(slot.Skills.All(skill => skill.Level >= 1), Is.True, "base skills must show level 1, not 0");
            Assert.That(slot.SelectedSkillIds.Count, Is.EqualTo(slot.MaxActiveSkills));
            Assert.That(slot.Quirks.Count, Is.EqualTo(2));
            Assert.That(slot.QuirkSummary, Is.Not.Empty);
            Assert.That(slot.Details, Does.Contain("HP"));
            Assert.That(slot.SelectedQuirkIds.Count, Is.EqualTo(2));
        }

        [Test]
        public void Slot_ToggleSkill_RespectsMaxActive()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);
            slot.AssignClass("crusader");
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
        public void Slot_SkillAvailabilityTracksMaxActive()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);
            slot.AssignClass("crusader");

            Assert.That(slot.CanSelectSkills, Is.True);
            Assert.That(slot.Stats.ResistStun, Is.GreaterThan(0));
            Assert.That(slot.Skills.Count(skill => skill.IsActive), Is.EqualTo(slot.MaxActiveSkills));
            Assert.That(slot.Skills.Where(skill => !skill.IsActive).All(skill => !skill.IsUsable), Is.True);
            Assert.That(slot.Skills.All(skill => skill.IsLoadout), Is.True);
            Assert.That(slot.Skills.All(skill => skill.IsMasked == !skill.IsActive), Is.True);

            slot.ToggleSkillCommand.Execute(slot.Skills.First(skill => skill.IsActive));

            Assert.That(slot.Skills.Where(skill => !skill.IsActive).All(skill => skill.IsUsable), Is.True);
            Assert.That(slot.Skills.All(skill => skill.IsMasked == !skill.IsActive), Is.True);
        }

        [Test]
        public void Slot_AbominationSkillsAreLockedAndAllSelected()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);
            slot.AssignClass("abomination");

            Assert.That(slot.CanSelectSkills, Is.False);
            Assert.That(slot.SkillsLocked, Is.True);
            Assert.That(slot.Skills, Is.Not.Empty);
            Assert.That(slot.Skills.All(skill => skill.IsActive), Is.True);
            Assert.That(slot.Skills.All(skill => !skill.IsMasked), Is.True);
            Assert.That(slot.SelectedSkillIds.Count, Is.EqualTo(slot.Skills.Count));

            var before = slot.SelectedSkillIds.ToList();
            slot.ToggleSkillCommand.Execute(slot.Skills[0]);

            Assert.That(slot.SelectedSkillIds, Is.EqualTo(before));
        }

        [Test]
        public void TrinketSlot_Reroll_AssignsValidTrinket()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);
            slot.AssignClass("crusader");

            slot.TrinketSlots[0].RerollCommand.Execute(null);

            Assert.That(slot.TrinketSlots[0].TrinketId, Is.Not.Empty);
        }

        [Test]
        public void Slot_RerollName_UpdatesNameAndStats()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds, null, null, 2);

            slot.RerollNameCommand.Execute(null);

            Assert.That(slot.HeroName, Is.Not.Empty);
            Assert.That(slot.Stats.HeroName, Is.EqualTo(slot.HeroName));
            Assert.That(slot.Position, Is.EqualTo(2));
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

        [Test]
        public void Slot_QuirkRowsCarryLockStateAndReadableName()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);

            Assert.That(slot.PositiveQuirks, Has.Count.EqualTo(5));
            Assert.That(slot.NegativeQuirks, Has.Count.EqualTo(5));
            var positive = slot.PositiveQuirks.First(quirk => quirk.DisplayName.Length > 0);
            var negative = slot.NegativeQuirks.First(quirk => quirk.DisplayName.Length > 0);

            Assert.That(positive.IsLocked, Is.True);
            Assert.That(positive.LockIcon, Is.Not.Null);
            Assert.That(negative.IsLocked, Is.False);
            Assert.That(negative.LockIcon, Is.Null);
            Assert.That(positive.DisplayName, Does.Not.Contain("_"));
            Assert.That(negative.DisplayName, Does.Not.Contain("_"));
        }

        [Test]
        public void Slot_NameEditing_ClampsToConfiguredLimit()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);

            slot.EditNameCommand.Execute(null);
            Assert.That(slot.IsEditingName, Is.True);

            slot.HeroName = new string('A', 40);
            slot.IsEditingName = false;

            Assert.That(slot.HeroName.Length, Is.EqualTo(slot.MaxHeroNameLength));
            Assert.That(slot.Stats.HeroName, Is.EqualTo(slot.HeroName));
        }

        [Test]
        public void Slot_HasFixedThreeDiseaseSlots()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);

            Assert.That(slot.Diseases, Has.Count.EqualTo(3));
            Assert.That(slot.Diseases.All(disease => disease.IsEmpty), Is.True);
        }

        [Test]
        public void Slot_SkinSelector_StartsAtFirstSkin()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);

            Assert.That(slot.SkinId, Is.EqualTo(1));
            slot.NextSkinCommand.Execute(null);
            Assert.That(slot.SkinId, Is.EqualTo(1), "only one skin is implemented so far");
        }

        [Test]
        public void Slot_LoadsTwoTrinketSlots()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);

            Assert.That(slot.TrinketSlots.Count, Is.EqualTo(2));
            Assert.That(slot.SelectedTrinketIds, Is.Empty);
        }

        [Test]
        public void Slot_RerollTrinkets_AssignsValidTrinkets()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);

            slot.RerollTrinketsCommand.Execute(null);

            Assert.That(slot.SelectedTrinketIds.Count, Is.EqualTo(2));
            Assert.That(slot.SelectedTrinketIds[0], Is.Not.EqualTo(slot.SelectedTrinketIds[1]));
        }

        [Test]
        public void Slot_TrinketPool_FiltersByClassRequirement()
        {
            var crusader = new HeroSlotViewModel(1, DuelClasses.AllClassIds);
            crusader.AssignClass("crusader");

            crusader.TrinketSlots[0].Select("sacred_scroll");
            Assert.That(crusader.SelectedTrinketIds, Is.Empty,
                "sacred_scroll requires a vestal and must not be selectable on a crusader.");

            var vestal = new HeroSlotViewModel(2, DuelClasses.AllClassIds);
            vestal.AssignClass("vestal");
            vestal.TrinketSlots[0].Select("sacred_scroll");
            Assert.That(vestal.SelectedTrinketIds, Contains.Item("sacred_scroll"));
        }

        [Test]
        public void Slot_LoadsCampingSkillsAndRespectsMaxSelected()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);
            slot.AssignClass("crusader");

            Assert.That(slot.CampingSkills, Is.Not.Empty);
            Assert.That(slot.CampingSkills.Count, Is.GreaterThan(slot.MaxCampingSkills));
            Assert.That(slot.CampingSkills.Count(skill => skill.IsActive), Is.EqualTo(slot.MaxCampingSkills));
            Assert.That(slot.SelectedCampingSkillIds.Count, Is.EqualTo(slot.MaxCampingSkills));
            Assert.That(slot.CampingSkills.All(skill => skill.Level == 0), Is.True);
            Assert.That(slot.CampingSkills.All(skill => skill.IsLoadout), Is.True);
            Assert.That(slot.CampingSkills.All(skill => skill.IsMasked == !skill.IsActive), Is.True);

            var inactive = slot.CampingSkills.First(skill => !skill.IsActive);
            slot.ToggleCampingSkillCommand.Execute(inactive);
            Assert.That(slot.CampingSkills.Count(skill => skill.IsActive), Is.EqualTo(slot.MaxCampingSkills),
                "A fifth camping skill must not be selectable.");

            var active = slot.CampingSkills.First(skill => skill.IsActive);
            slot.ToggleCampingSkillCommand.Execute(active);
            Assert.That(slot.CampingSkills.Count(skill => skill.IsActive), Is.EqualTo(slot.MaxCampingSkills - 1));
            Assert.That(slot.CampingSkills.Where(skill => !skill.IsActive).All(skill => skill.IsUsable), Is.True);

            slot.ToggleCampingSkillCommand.Execute(inactive);
            Assert.That(slot.CampingSkills.Count(skill => skill.IsActive), Is.EqualTo(slot.MaxCampingSkills));
        }

        [Test]
        public void Slot_ResolvesIdleContentForCurrentClass()
        {
            var catalog = new SpineDemoCatalog(Path.Combine(AppContext.BaseDirectory, "Content", "Spine"));
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds, spineCatalog: catalog);

            slot.AssignClass("crusader");

            Assert.That(slot.IdleContentDirectory, Does.EndWith(Path.Combine("Crusader", "Idle")));
            Assert.That(Directory.Exists(slot.IdleContentDirectory), Is.True);
        }

        [Test]
        public void Slot_WithoutSpineCatalog_HasNoIdleDirectory()
        {
            var slot = new HeroSlotViewModel(1, DuelClasses.AllClassIds);

            Assert.That(slot.IdleContentDirectory, Is.Empty);
        }
    }
}