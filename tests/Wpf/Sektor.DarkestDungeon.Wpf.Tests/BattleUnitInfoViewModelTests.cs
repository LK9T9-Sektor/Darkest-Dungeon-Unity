using System.Collections.Generic;

using NUnit.Framework;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Tests
{
    /// <summary>Battle unit-info adapter: live stats, polymorphic capabilities and fallback data.</summary>
    [TestFixture]
    public class BattleUnitInfoViewModelTests
    {
        /// <summary>Heroes keep name/skin editing and receive live stats.</summary>
        [Test]
        public void HeroSheet_CopiesLiveStatsAndKeepsNameAndSkin()
        {
            var unit = new DuelUnitViewModel(1, 2, 1, "Reynauld", "Crusader")
            {
                IsHero = true,
                ClassId = "crusader",
                HpCurrent = 42,
                HpMax = 78,
                Stress = 15,
                Speed = 5,
                Damage = "8 - 15",
                Accuracy = 85,
                Crit = 7,
                Dodge = 12,
                Protection = 10,
                ResistStun = 40,
                ResistDeathBlow = 67,
            };
            var stats = new HeroStatsViewModel();
            stats.Apply(unit, new List<DuelSkillViewModel>());

            var sheet = BattleUnitInfoViewModel.Create(unit, stats, null, null, null);

            Assert.That(sheet, Is.TypeOf<BattleHeroInfoViewModel>());
            Assert.That(sheet.Stats.HitPoints, Is.EqualTo("42 / 78"));
            Assert.That(sheet.Stats.Stress, Is.EqualTo("15 / 100"));
            Assert.That(sheet.Stats.ResistStun, Is.EqualTo(40));
            Assert.That(sheet.Stats.ResistDeathBlow, Is.EqualTo(67));
            Assert.That(sheet.CanEditName, Is.True);
            Assert.That(sheet.CanRerollName, Is.True);
            Assert.That(sheet.CanChangeSkin, Is.True);
            Assert.That(sheet.CanEditSkills, Is.False);
            Assert.That(sheet.CanEditCampingSkills, Is.False);
            Assert.That(sheet.ShowPortrait, Is.False);
            Assert.That(sheet.ShowClassSwitcher, Is.False);
            Assert.That(sheet.ShowPosition, Is.False);
            Assert.That(sheet.ShowCharacterControls, Is.True);
            Assert.That(sheet.IsSkillSelectionReadOnly, Is.True);
            Assert.That(sheet.Diseases, Has.Count.EqualTo(3));
            Assert.That(sheet.PositiveQuirks, Has.Count.EqualTo(5));
        }

        /// <summary>Monster sheets are fully read-only and hide the bottom controls.</summary>
        [Test]
        public void MonsterSheet_IsReadOnly()
        {
            var unit = new DuelUnitViewModel(2, 1, 1, "Bone Soldier", "Bone Soldier")
            {
                IsHero = false,
                ClassId = "bone_soldier",
            };
            var stats = new HeroStatsViewModel();
            stats.Apply(unit, new List<DuelSkillViewModel>());

            var sheet = BattleUnitInfoViewModel.Create(unit, stats, null, null, null);

            Assert.That(sheet, Is.TypeOf<BattleMonsterInfoViewModel>());
            Assert.That(sheet.CanEditName, Is.False);
            Assert.That(sheet.CanRerollName, Is.False);
            Assert.That(sheet.CanChangeSkin, Is.False);
            Assert.That(sheet.ShowCharacterControls, Is.False);
        }

        /// <summary>Name rerolls stay local to the unit card.</summary>
        [Test]
        public void HeroSheet_RenameUpdatesUnitName()
        {
            var unit = new DuelUnitViewModel(1, 1, 1, "Reynauld", "Crusader") { IsHero = true, ClassId = "crusader" };
            var stats = new HeroStatsViewModel();
            stats.Apply(unit, new List<DuelSkillViewModel>());
            var sheet = BattleUnitInfoViewModel.Create(unit, stats, null, null, null);

            sheet.HeroName = "Dismas";

            Assert.That(sheet.Stats.HeroName, Is.EqualTo("Dismas"));
            Assert.That(unit.Name, Is.EqualTo("Dismas"));
        }
    }
}
