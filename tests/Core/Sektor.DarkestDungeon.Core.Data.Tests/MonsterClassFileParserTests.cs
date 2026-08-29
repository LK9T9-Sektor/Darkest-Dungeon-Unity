using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;

namespace Sektor.DarkestDungeon.Core.Data.Tests
{
    /// <summary>Tests for the MonsterClassFileParser on real campaign monster files.</summary>
    public class MonsterClassFileParserTests
    {
        /// <summary>The real swine_slasher_A monster file parses into the expected model.</summary>
        [Test]
        public void ParseRealMonsterFileMapsFields()
        {
            string path = FindUnityDataFile("Data", "Monsters", "swine_slasher_A.txt");
            Assert.That(path, Is.Not.Null, "unity Assets/Resources/Data/Monsters must be available.");

            MonsterClass monsterClass = MonsterClassFileParser.Parse(File.ReadAllText(path));

            Assert.That(monsterClass.StringId, Is.EqualTo("swine_slasher_A"));
            Assert.That(monsterClass.TypeId, Is.EqualTo("swine_slasher"));
            Assert.That(monsterClass.Size, Is.EqualTo(1));
            Assert.That(monsterClass.EnemyTypes, Does.Contain(MonsterType.Man));
            Assert.That(monsterClass.EnemyTypes, Does.Contain(MonsterType.Beast));
            Assert.That(monsterClass.Attributes[AttributeType.HitPoints], Is.EqualTo(8f));
            Assert.That(monsterClass.Attributes[AttributeType.DefenseRating], Is.EqualTo(0.075f));
            Assert.That(monsterClass.Attributes[AttributeType.ProtectionRating], Is.EqualTo(0.25f));
            Assert.That(monsterClass.Attributes[AttributeType.SpeedRating], Is.EqualTo(5f));
            Assert.That(monsterClass.Attributes[AttributeType.Stun], Is.EqualTo(0.10f));
            Assert.That(monsterClass.Attributes[AttributeType.Debuff], Is.EqualTo(0.15f));
            Assert.That(monsterClass.PreferableSkill, Is.EqualTo(1));
            Assert.That(monsterClass.InitiativeTurns, Is.EqualTo(1));
            Assert.That(monsterClass.MonsterBrainId, Is.EqualTo("swine_slasher_A"));
            Assert.That(monsterClass.Modifiers.CanSurprise, Is.True);
            Assert.That(monsterClass.Modifiers.CanBeSurprised, Is.True);
            Assert.That(monsterClass.Modifiers.AlwaysSurprise, Is.False);
        }

        /// <summary>Combat skills of a real monster map damage, accuracy and formations correctly.</summary>
        [Test]
        public void ParseRealMonsterSkillMapsCombatSkill()
        {
            string path = FindUnityDataFile("Data", "Monsters", "swine_slasher_A.txt");
            Assert.That(path, Is.Not.Null);

            MonsterClass monsterClass = MonsterClassFileParser.Parse(File.ReadAllText(path));

            Assert.That(monsterClass.CombatSkills, Has.Count.EqualTo(1));
            var skill = monsterClass.CombatSkills[0];
            Assert.That(skill.Id, Is.EqualTo("hook_where_it_hurts"));
            Assert.That(skill.DamageMin, Is.EqualTo(3f));
            Assert.That(skill.DamageMax, Is.EqualTo(7f));
            Assert.That(skill.Accuracy, Is.EqualTo(0.825f));
            Assert.That(skill.CritMod, Is.EqualTo(0.16f));
            Assert.That(skill.LaunchRanks.Ranks, Is.EqualTo(new[] { 1, 2, 3, 4 }));
            Assert.That(skill.TargetRanks.Ranks, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(skill.Move.Pushback, Is.EqualTo(1));
            Assert.That(skill.Move.Pullforward, Is.EqualTo(0));
        }

        /// <summary>The Monster model built from a class exposes combat-ready attributes.</summary>
        [Test]
        public void MonsterInstanceExposesCharacterState()
        {
            string path = FindUnityDataFile("Data", "Monsters", "swine_slasher_A.txt");
            Assert.That(path, Is.Not.Null);

            MonsterClass monsterClass = MonsterClassFileParser.Parse(File.ReadAllText(path));
            var monster = new Monster(monsterClass);

            Assert.That(monster.IsMonster, Is.True);
            Assert.That(monster.MaxHealth, Is.EqualTo(8f));
            Assert.That(monster.CurrentHealth, Is.EqualTo(8f));
            Assert.That(monster.Protection, Is.EqualTo(0.25f));
            Assert.That(monster.Speed, Is.EqualTo(5f));
            Assert.That(monster.DamageMod, Is.EqualTo(1f));
            Assert.That(monster.CurrentCombatSkills, Has.Count.EqualTo(1));
            Assert.That(monster.PreferableSkill, Is.EqualTo(1));
            Assert.That(monster.Brain, Is.Null);

            monster.AssignBrain(null);
            Assert.That(monster.Brain, Is.Null);
        }

        private static string FindUnityDataFile(params string[] parts)
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                string candidate = Path.Combine(
                    new[] { current.FullName, "unity", "Assets", "Resources" }.Concat(parts).ToArray());
                if (File.Exists(candidate))
                    return candidate;
                current = current.Parent;
            }
            return null;
        }
    }
}