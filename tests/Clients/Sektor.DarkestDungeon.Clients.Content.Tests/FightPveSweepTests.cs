using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Sektor.DarkestDungeon.Clients.Content;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Content.Trinket;
using Sektor.DarkestDungeon.Core.Duel.Fight;

namespace Sektor.DarkestDungeon.Clients.Content.Tests
{
    /// <summary>
    /// Sweeps every campaign monster against a standard hero party through the pure core fight runner.
    /// The goal is to prove the core campaign battle path (monsters + campaign brains) executes every
    /// monster class without exceptions, stalls or parser gaps, and stays deterministic for a seed.
    /// </summary>
    public class FightPveSweepTests
    {
        private const int StepBudget = 20000;
        private const int DeterminismSampleSize = 20;

        private static TextFightContent _content;
        private static MonsterCatalog _monsters;

        /// <summary>Builds the campaign content once for the whole fixture.</summary>
        [OneTimeSetUp]
        public void BuildSharedContent()
        {
            BuildContent();
        }

        /// <summary>
        /// Scripted boss/prop entities that are not normal combat units and are unwinnable in a generic
        /// 1v4 sweep fight by design (invulnerable or scripted transform stages). Excluded from the sweep.
        /// </summary>
        private static readonly HashSet<string> SpecialMonstersSkipped = new HashSet<string>
        {
            "cauldron_empty_A", "cauldron_empty_B", "cauldron_empty_C", // Hag captor vessel: prot 1 (100%), initiative 0, boss tag.
            "ancestor_nebula_D", // Ancestor transform stage: hp 999, can_be_hit False, initiative 0.
            "ancestor_small_D",  // Ancestor mirror stage: can_be_damaged_directly false, can_die_from_damage False.
        };

        /// <summary>
        /// Every campaign monster, fought alone against a standard hero party, finishes the battle within
        /// the step budget without throwing. Failures are aggregated per monster id.
        /// </summary>
        [Test]
        public void EveryMonsterFinishesAgainstTheStandardHeroParty()
        {
            var failures = new List<string>();
            var passes = new List<string>();

            foreach (string monsterId in _monsters.Ids)
            {
                if (SpecialMonstersSkipped.Contains(monsterId))
                    continue;

                string result = TryRunStandardParty(monsterId, out string detail);
                if (result == "pass")
                    passes.Add(monsterId);
                else
                    failures.Add(monsterId + " -> " + detail);
            }

            Assert.That(failures.Count, Is.Zero,
                "{0} of {1} monsters failed:\n{2}",
                failures.Count, _monsters.Count, string.Join("\n", failures));
            Assert.That(passes.Count, Is.GreaterThan(0), "The sweep must cover at least one monster.");
        }

        /// <summary>
        /// A sample of monsters is deterministic for a fixed seed: two runs of the same fight produce the
        /// same winner and the same number of acting steps.
        /// </summary>
        [Test]
        public void SampleMonstersAreDeterministicForAFixedSeed()
        {
            var sample = _monsters.Ids
                .Where(id => !SpecialMonstersSkipped.Contains(id))
                .Where((id, index) => index % Math.Max(1, _monsters.Count / DeterminismSampleSize) == 0)
                .Take(DeterminismSampleSize)
                .ToList();

            var mismatches = new List<string>();
            foreach (string monsterId in sample)
            {
                FightOutcome first = RunStandardParty(monsterId, 7);
                FightOutcome second = RunStandardParty(monsterId, 7);
                if (first.Winner != second.Winner || first.Steps != second.Steps)
                    mismatches.Add(monsterId + string.Format(
                        " (winner {0}/{1}, steps {2}/{3})", first.Winner, second.Winner, first.Steps, second.Steps));
            }

            Assert.That(mismatches, Is.Empty,
                "Non-deterministic monsters:\n" + string.Join("\n", mismatches));
        }

        private static string TryRunStandardParty(string monsterId, out string detail)
        {
            try
            {
                FightOutcome outcome = RunStandardParty(monsterId, 42);
                if (!outcome.Finished)
                {
                    detail = "did not finish within " + StepBudget + " steps (winner=" + outcome.Winner + ")";
                    return "fail";
                }

                detail = string.Empty;
                return "pass";
            }
            catch (Exception exception)
            {
                detail = exception.GetType().Name + ": " + exception.Message +
                    "\n" + FirstStackTraceLines(exception);
                return "fail";
            }
        }

        private static string FirstStackTraceLines(Exception exception)
        {
            string[] lines = (exception.StackTrace ?? string.Empty).Split('\n');
            return string.Join("\n", lines.Take(6));
        }

        private static FightOutcome RunStandardParty(string monsterId, int seed)
        {
            var player = StandardHeroParty();
            var ai = new List<FightUnitSpec> { new MonsterFightUnitSpec(monsterId) };

            var session = new FightSession(_content, seed);
            session.Start(player, ai);

            int steps = 0;
            while (!session.IsFinished && steps < StepBudget)
            {
                session.Tick();
                steps++;
            }

            bool heroesAlive = session.Duel.HeroParty.Units.Any(unit => !unit.CombatInfo.IsDead);
            bool monstersAlive = session.Duel.MonsterParty.Units.Any(unit => !unit.CombatInfo.IsDead);
            string winner = heroesAlive ? (monstersAlive ? "draw" : "heroes") : "monsters";
            return new FightOutcome(winner, steps, session.IsFinished);
        }

        private static List<FightUnitSpec> StandardHeroParty()
        {
            return new List<FightUnitSpec>
            {
                new HeroFightUnitSpec("crusader", 101, new List<string> { "smite", "zealous_accusation", "stunning_blow", "battle_heal" }, null),
                new HeroFightUnitSpec("vestal", 102, new List<string> { "judgement", "mace_bash", "divine_grace", "dazzling_light" }, null),
                new HeroFightUnitSpec("highwayman", 103, new List<string> { "wicked_slice", "pistol_shot", "point_blank_shot", "opened_vein" }, null),
                new HeroFightUnitSpec("plague_doctor", 104, new List<string> { "noxious_blast", "plague_grenade", "incision", "battlefield_medicine" }, null)
            };
        }

        private static void BuildContent()
        {
            string resourcesDir = FindUnityResourcesDir();
            Assert.That(resourcesDir, Is.Not.Null, "unity Assets/Resources must be available.");

            string effectsText = File.ReadAllText(Path.Combine(resourcesDir, "Data", "Mechanics", "Effects.txt"));
            EffectCatalog effects = GameDataReader.ReadEffects(effectsText);

            string[] heroFiles = Directory.GetFiles(Path.Combine(resourcesDir, "Data", "Heroes", "Info"), "*.bytes");
            HeroCatalog heroes = GameDataReader.ReadHeroes(heroFiles.Select(File.ReadAllText), effects);

            string[] monsterFiles = Directory.GetFiles(Path.Combine(resourcesDir, "Data", "Monsters"), "*.txt");
            MonsterCatalog monsters = GameDataReader.ReadMonsters(monsterFiles.Select(File.ReadAllText), effects);
            _monsters = monsters;

            MonsterBrainCatalog brains = GameDataReader.ReadBrains(File.ReadAllText(Path.Combine(resourcesDir, "Data", "JsonAI.json")));
            BuffCatalog buffs = GameDataReader.ReadBuffs(File.ReadAllText(Path.Combine(resourcesDir, "Data", "JsonBuffs.json")));
            QuirkCatalog quirks = GameDataReader.ReadQuirks(File.ReadAllText(Path.Combine(resourcesDir, "Data", "JsonQuirks.json")));
            TrinketCatalog trinkets = GameDataReader.ReadTrinketCatalog(File.ReadAllText(Path.Combine(resourcesDir, "Data", "JsonTrinkets.json")));

            var traits = GameDataReader.ReadTraits(File.ReadAllText(Path.Combine(resourcesDir, "Data", "JsonTraits.json")));
            _content = new TextFightContent(
                heroes,
                monsters,
                brains,
                buffs,
                quirks,
                effects,
                traits.Where(trait => trait.IsAffliction).ToList(),
                traits.Where(trait => trait.IsVirtue).ToList(),
                trinkets);
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