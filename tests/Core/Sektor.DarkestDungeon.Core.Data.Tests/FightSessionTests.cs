using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Duel;
using Sektor.DarkestDungeon.Core.Duel.Fight;
using Sektor.DarkestDungeon.Core.Data.Catalogs;
using Sektor.DarkestDungeon.Core.Data.Content;
using Sektor.DarkestDungeon.Core.Data.Dto;
using Sektor.DarkestDungeon.Core.Data.Readers;

namespace Sektor.DarkestDungeon.Core.Data.Tests
{
    /// <summary>Tests the automated campaign fight runner against the real campaign content.</summary>
    public class FightSessionTests
    {
        /// <summary>A hero party dueled against real campaign monsters under the campaign AI finishes deterministically.</summary>
        [Test]
        public void RunFightStartsWithHeroesAndMonstersAndFinishes()
        {
            var content = BuildContent();
            var player = new List<FightUnitSpec>
            {
                new HeroFightUnitSpec("crusader", 101, new List<string> { "smite", "zealous_accusation", "stunning_blow", "battle_heal" }, null),
                new HeroFightUnitSpec("vestal", 102, new List<string> { "judgement", "mace_bash", "divine_grace", "dazzling_light" }, null),
                new HeroFightUnitSpec("highwayman", 103, new List<string> { "wicked_slice", "pistol_shot", "point_blank_shot", "opened_vein" }, null),
                new HeroFightUnitSpec("plague_doctor", 104, new List<string> { "noxious_blast", "plague_grenade", "incision", "battlefield_medicine" }, null)
            };
            var ai = new List<FightUnitSpec>
            {
                new MonsterFightUnitSpec("swine_slasher_A"),
                new MonsterFightUnitSpec("swine_slasher_B"),
                new MonsterFightUnitSpec("swine_drummer_A"),
                new MonsterFightUnitSpec("swine_piglet_A")
            };

            var session = new FightSession(content, 42);
            session.Start(player, ai);

            Assert.That(session.IsStarted, Is.True);
            Assert.That(session.Duel.HeroParty.Units.Count, Is.EqualTo(4));
            Assert.That(session.Duel.MonsterParty.Units.Count, Is.EqualTo(4));

            int steps;
            for (steps = 0; steps < 20000 && !session.IsFinished; steps++)
                session.Tick();

            Assert.That(session.IsFinished, Is.True, "Fight did not finish within the step budget.");
            Assert.That(session.Duel.Phase, Is.EqualTo(DuelPhase.Finished));
            Assert.That(session.Duel.BattleGround.Round.RoundNumber, Is.GreaterThan(0));
        }

        /// <summary>The fight runner is deterministic for a fixed seed.</summary>
        [Test]
        public void RunFightIsDeterministicForFixedSeed()
        {
            var player = new List<FightUnitSpec>
            {
                new HeroFightUnitSpec("crusader", 1, new List<string> { "smite", "zealous_accusation", "stunning_blow", "battle_heal" }, null),
                new HeroFightUnitSpec("vestal", 2, new List<string> { "judgement", "mace_bash", "divine_grace", "dazzling_light" }, null),
                new HeroFightUnitSpec("highwayman", 3, new List<string> { "wicked_slice", "pistol_shot", "point_blank_shot", "opened_vein" }, null),
                new HeroFightUnitSpec("plague_doctor", 4, new List<string> { "noxious_blast", "plague_grenade", "incision", "battlefield_medicine" }, null)
            };
            var ai = new List<FightUnitSpec>
            {
                new MonsterFightUnitSpec("swine_slasher_A"),
                new MonsterFightUnitSpec("swine_slasher_B"),
                new MonsterFightUnitSpec("swine_drummer_A"),
                new MonsterFightUnitSpec("swine_piglet_A")
            };

            var first = Run(player, ai, 7);
            var second = Run(player, ai, 7);

            Assert.That(first.Winner, Is.EqualTo(second.Winner));
            Assert.That(first.Steps, Is.EqualTo(second.Steps));
            Assert.That(first.Winner, Is.Not.Null, "A fight must produce a winner.");
        }

        private static FightOutcome Run(IReadOnlyList<FightUnitSpec> player, IReadOnlyList<FightUnitSpec> ai, int seed)
        {
            var session = new FightSession(BuildContent(), seed);
            session.Start(player, ai);

            int steps = 0;
            while (!session.IsFinished && steps < 20000)
            {
                session.Tick();
                steps++;
            }

            bool heroesAlive = session.Duel.HeroParty.Units.Any(u => !u.CombatInfo.IsDead);
            bool monstersAlive = session.Duel.MonsterParty.Units.Any(u => !u.CombatInfo.IsDead);
            string winner = heroesAlive ? (monstersAlive ? "draw" : "heroes") : "monsters";
            return new FightOutcome(winner, steps, session.IsFinished);
        }

        /// <summary>
        /// A manual player action executes the chosen hero skill against the chosen target instead of
        /// an AI choice; the fight parks on player-controlled heroes and proceeds automatically elsewhere.
        /// </summary>
        [Test]
        public void ManualPlayerAction_ExecutesTheChosenSkillOnTheChosenTarget()
        {
            var content = BuildContent();
            var player = new List<FightUnitSpec>
            {
                new HeroFightUnitSpec("crusader", 11, new List<string> { "smite", "zealous_accusation", "stunning_blow", "battle_heal" }, null),
                new HeroFightUnitSpec("vestal", 12, new List<string> { "judgement", "mace_bash", "divine_grace", "dazzling_light" }, null),
                new HeroFightUnitSpec("highwayman", 13, new List<string> { "wicked_slice", "pistol_shot", "point_blank_shot", "opened_vein" }, null),
                new HeroFightUnitSpec("plague_doctor", 14, new List<string> { "noxious_blast", "plague_grenade", "incision", "battlefield_medicine" }, null)
            };
            var ai = new List<FightUnitSpec>
            {
                new MonsterFightUnitSpec("swine_slasher_A"),
                new MonsterFightUnitSpec("swine_slasher_B"),
                new MonsterFightUnitSpec("swine_drummer_A"),
                new MonsterFightUnitSpec("swine_piglet_A")
            };

            var session = new FightSession(content, 21);
            session.Start(player, ai);

            Assert.That(session.IsStarted, Is.True);

            int guard = 0;
            while (!session.IsFinished && !session.IsWaitingForPlayerAction && guard++ < 5000)
            {
                if (!session.Tick())
                    break;
            }

            Assert.That(session.IsWaitingForPlayerAction, Is.True, "The fight must park on a player hero.");

            var actor = session.Duel.CurrentUnit;
            var target = session.Duel.MonsterParty.Units.First(unit => unit.IsTargetable);
            float healthBefore = target.Character.HealthRatio;

            bool running = session.Tick(new FightPlayerAction("smite", target.CombatInfo.CombatId));

            Assert.That(running, Is.True, "The fight continues after a manual action.");
            Assert.That(
                target.Character.HealthRatio,
                Is.LessThanOrEqualTo(healthBefore),
                "The manual smite must reduce the target's health.");

            while (!session.IsFinished && guard++ < 5000)
                session.Tick();

            Assert.That(session.IsFinished, Is.True, "The fight must finish under automatic control after the manual action.");
        }

        private static TextFightContent BuildContent()
        {
            string resourcesDir = FindUnityResourcesDir();
            Assert.That(resourcesDir, Is.Not.Null, "unity Assets/Resources must be available.");

            string effectsText = File.ReadAllText(Path.Combine(resourcesDir, "Data", "Mechanics", "Effects.txt"));
            EffectCatalog effects = GameDataReader.ReadEffects(effectsText);

            string[] heroFiles = Directory.GetFiles(Path.Combine(resourcesDir, "Data", "Heroes", "Info"), "*.bytes");
            HeroCatalog heroes = GameDataReader.ReadHeroes(heroFiles.Select(File.ReadAllText), effects);

            string[] monsterFiles = Directory.GetFiles(Path.Combine(resourcesDir, "Data", "Monsters"), "*.txt");
            MonsterCatalog monsters = GameDataReader.ReadMonsters(monsterFiles.Select(File.ReadAllText), effects);

            MonsterBrainCatalog brains = GameDataReader.ReadBrains(File.ReadAllText(Path.Combine(resourcesDir, "Data", "JsonAI.json")));
            BuffCatalog buffs = GameDataReader.ReadBuffs(File.ReadAllText(Path.Combine(resourcesDir, "Data", "JsonBuffs.json")));
            QuirkCatalog quirks = GameDataReader.ReadQuirks(File.ReadAllText(Path.Combine(resourcesDir, "Data", "JsonQuirks.json")));

            var traits = GameDataReader.ReadTraits(File.ReadAllText(Path.Combine(resourcesDir, "Data", "JsonTraits.json")));
            return new TextFightContent(
                heroes,
                monsters,
                brains,
                buffs,
                quirks,
                effects,
                traits.Where(trait => trait.IsAffliction).ToList(),
                traits.Where(trait => trait.IsVirtue).ToList());
        }

        private static string FindUnityResourcesDir()
        {
            var current = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory;
            while (current != null)
            {
                string candidate = Path.Combine(current.FullName, "unity", "Assets", "Resources");
                if (Directory.Exists(candidate))
                    return candidate;
                current = current.Parent;
            }
            return null;
        }

        private sealed class FightOutcome
        {
            public FightOutcome(string winner, int steps, bool finished)
            {
                Winner = winner;
                Steps = steps;
                Finished = finished;
            }

            public string Winner { get; }

            public int Steps { get; }

            public bool Finished { get; }
        }
    }
}