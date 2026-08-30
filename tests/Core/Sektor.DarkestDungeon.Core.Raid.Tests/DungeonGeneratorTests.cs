using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.Raid;
using Sektor.DarkestDungeon.Core.Raid.Generation;

namespace Sektor.DarkestDungeon.Core.Raid.Tests
{
    [TestFixture]
    public class DungeonGeneratorTests
    {
        [Test]
        public void ParseMapGenerator_ReadsShortExploreEntry()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Data", "Mechanics", "MapGenerator.txt");
            Assert.That(File.Exists(path), Is.True, "The MapGenerator.txt must be linked to the tests.");

            string text = File.ReadAllText(path);
            var entries = DungeonGenerationDataParser.Parse(text);

            TestContext.WriteLine("entries=" + entries.Count + " textLen=" + text.Length +
                " first=" + (entries.Count > 0 ? entries[0].Length + "|" + entries[0].Dungeon : "none"));

            Assert.That(entries.Count, Is.GreaterThan(0));
            var shortCrypts = entries.First(e => e.Length == "short" && e.Dungeon == "crypts");
            Assert.That(shortCrypts.QuestType, Is.EqualTo("explore"));
            Assert.That(shortCrypts.BaseRoomNumber, Is.EqualTo(9));
            Assert.That(shortCrypts.GridSizeX, Is.EqualTo(4));
            Assert.That(shortCrypts.GridSizeY, Is.EqualTo(3));
            Assert.That(shortCrypts.HallwayBattleMin, Is.EqualTo(2));
            Assert.That(shortCrypts.HallwayBattleMax, Is.EqualTo(4));
        }

        [Test]
        public void ParseDungeonEnviroment_ReadsCryptsMashAndProps()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Data", "Dungeons", "Crypts.bytes");
            Assert.That(File.Exists(path), Is.True, "The Crypts.bytes must be linked to the tests.");

            var env = DungeonEnviromentDataParser.Parse(File.ReadAllText(path));

            Assert.That(env.HallVariations, Is.GreaterThan(0));
            Assert.That(env.RoomVariations, Has.Count.GreaterThan(0));
            Assert.That(env.BattleMashes, Has.Count.GreaterThan(0));
            Assert.That(env.BattleMashes[0].RoomEncounters, Has.Count.GreaterThan(0));
            Assert.That(env.BattleMashes[0].HallEncounters, Has.Count.GreaterThan(0));
            Assert.That(env.HallCurios, Has.Count.GreaterThan(0));
            Assert.That(env.Traps, Has.Count.GreaterThan(0));
            Assert.That(env.Obstacles, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Generate_IsDeterministicForTheSameSeed()
        {
            var (genData, envData) = LoadRealData();
            var first = DungeonGenerator.Generate(genData, envData, "crypts", 1, new SystemRandomRng(12345));
            var second = DungeonGenerator.Generate(genData, envData, "crypts", 1, new SystemRandomRng(12345));

            Assert.That(second.Rooms.Count, Is.EqualTo(first.Rooms.Count));
            Assert.That(second.Hallways.Count, Is.EqualTo(first.Hallways.Count));
            CollectionAssert.AreEquivalent(first.Rooms.Keys, second.Rooms.Keys);
            CollectionAssert.AreEquivalent(first.Hallways.Keys, second.Hallways.Keys);
            Assert.That(second.StartingRoomId, Is.EqualTo(first.StartingRoomId));
        }

        [Test]
        public void Generate_DifferentSeed_MayDiffer()
        {
            var (genData, envData) = LoadRealData();
            var first = DungeonGenerator.Generate(genData, envData, "crypts", 1, new SystemRandomRng(1));
            var second = DungeonGenerator.Generate(genData, envData, "crypts", 1, new SystemRandomRng(2));

            bool identicalRooms = first.Rooms.Keys.SequenceEqual(second.Rooms.Keys)
                && first.Rooms.Keys.Count == second.Rooms.Keys.Count;
            Assert.That(identicalRooms, Is.False,
                "Different seeds should generally produce different layouts.");
        }

        [Test]
        public void Generate_ProducesExpectedRoomAndHallwayCounts()
        {
            var (genData, envData) = LoadRealData();

            var dungeon = DungeonGenerator.Generate(genData, envData, "crypts", 1, new SystemRandomRng(7));

            Assert.That(dungeon.Rooms.Count, Is.EqualTo(genData.BaseRoomNumber),
                "The generated room count must match base_room_number.");
            Assert.That(dungeon.Hallways.Count, Is.GreaterThan(0));
            Assert.That(dungeon.StartingRoomId, Is.Not.Null.And.Not.Empty);
            Assert.That(dungeon.Rooms[dungeon.StartingRoomId].Type, Is.EqualTo(AreaType.Entrance));

            var entrance = dungeon.Rooms[dungeon.StartingRoomId];
            Assert.That(entrance.Doors, Is.Not.Empty,
                "The entrance room must connect to at least one hallway.");
        }

        [Test]
        public void Generate_PopulatesRoomAndHallFeatureCounts()
        {
            var (genData, envData) = LoadRealData();

            var dungeon = DungeonGenerator.Generate(genData, envData, "crypts", 1, new SystemRandomRng(99));

            int battleRooms = dungeon.Rooms.Values.Count(r => r.Type == AreaType.Battle
                || r.Type == AreaType.BattleCurio || r.Type == AreaType.BattleTresure);
            Assert.That(battleRooms, Is.EqualTo(dungeon.TotalRoomBattles),
                "The populated room battle count must match TotalRoomBattles.");

            Assert.That(dungeon.HallwayBattles, Is.InRange(genData.HallwayBattleMin, genData.HallwayBattleMax));
            Assert.That(dungeon.HallwayTraps, Is.InRange(genData.HallwayTrapMin, genData.HallwayTrapMax));
        }

        [Test]
        public void Generate_AssignsFinalGridSizeWithStep7()
        {
            var (genData, envData) = LoadRealData();

            var dungeon = DungeonGenerator.Generate(genData, envData, "crypts", 1, new SystemRandomRng(5));

            Assert.That(dungeon.GridSizeX, Is.EqualTo(1 + (genData.GridSizeX - 1) * 7));
            Assert.That(dungeon.GridSizeY, Is.EqualTo(1 + (genData.GridSizeY - 1) * 7));
        }

        private static (DungeonGenerationData, DungeonEnviromentData) LoadRealData()
        {
            string mapPath = Path.Combine(AppContext.BaseDirectory, "Data", "Mechanics", "MapGenerator.txt");
            string envPath = Path.Combine(AppContext.BaseDirectory, "Data", "Dungeons", "Crypts.bytes");
            Assert.That(File.Exists(mapPath), Is.True);
            Assert.That(File.Exists(envPath), Is.True);

            var genData = DungeonGenerationDataParser.Parse(File.ReadAllText(mapPath))
                .First(e => e.Length == "short" && e.Dungeon == "crypts");
            var envData = DungeonEnviromentDataParser.Parse(File.ReadAllText(envPath));
            return (genData, envData);
        }
    }
}