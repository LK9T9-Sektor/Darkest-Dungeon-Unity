using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Common;
using Sektor.DarkestDungeon.Core.Raid;
using CoreGen = Sektor.DarkestDungeon.Core.Raid.Generation;

public static class DungeonGenerator
{
    public static Dungeon GenerateDungeon(Quest quest, int seed = 0)
    {
        string[] lengthes = { "", "short", "medium", "long" };

        DungeonGenerationData genData = DarkestDungeonManager.Data.DungeonGenerationData.Find(item =>
            item.Dungeon == quest.Dungeon && item.Length == lengthes[quest.Length] && item.QuestType == quest.Type);
        DungeonEnviromentData envData = DarkestDungeonManager.Data.DungeonEnviromentData[quest.Dungeon];

        var core = CoreGen.DungeonGenerator.Generate(ConvertGenData(genData), ConvertEnvData(envData),
            quest.Dungeon, quest.Difficulty, new SystemRandomRng(seed));

        Dungeon dungeon = MapDungeon(core);
        PopulateQuestGoals(dungeon, quest, core, envData);

        dungeon.DungeonMash = DarkestDungeonManager.Data.DungeonEnviromentData[quest.Dungeon].
            BattleMashes.Find(mash => mash.MashId == quest.Difficulty);
        dungeon.SharedMash = DarkestDungeonManager.Data.DungeonEnviromentData["shared"].
            BattleMashes.Find(mash => mash.MashId == quest.Difficulty);

        return dungeon;
    }

    private static CoreGen.DungeonGenerationData ConvertGenData(DungeonGenerationData data)
    {
        return new CoreGen.DungeonGenerationData
        {
            Length = data.Length,
            QuestType = data.QuestType,
            Dungeon = data.Dungeon,
            BaseRoomNumber = data.BaseRoomNumber,
            BaseCorridorNumber = data.BaseCorridorNumber,
            GridSizeX = data.GridSizeX,
            GridSizeY = data.GridSizeY,
            Spacing = data.Spacing,
            GoalRoomNumber = data.GoalRoomNumber,
            MinFinalDistance = data.MinFinalDistance,
            HallwayBattleMin = data.HallwayBattleMin,
            HallwayBattleMax = data.HallwayBattleMax,
            HallwayTrapMin = data.HallwayTrapMin,
            HallwayTrapMax = data.HallwayTrapMax,
            HallwayObstacleMin = data.HallwayObstacleMin,
            HallwayObstacleMax = data.HallwayObstacleMax,
            HallwayCurioMin = data.HallwayCurioMin,
            HallwayCurioMax = data.HallwayCurioMax,
            HallwayHungerMin = data.HallwayHungerMin,
            HallwayHungerMax = data.HallwayHungerMax,
            TotalRoomBattleMin = data.TotalRoomBattleMin,
            TotalRoomBattleMax = data.TotalRoomBattleMax,
            RoomGuardedCurioMin = data.RoomGuardedCurioMin,
            RoomGuardedCurioMax = data.RoomGuardedCurioMax,
            RoomGuardedTresureMin = data.RoomGuardedTresureMin,
            RoomGuardedTresureMax = data.RoomGuardedTresureMax,
        };
    }

    private static CoreGen.DungeonEnviromentData ConvertEnvData(DungeonEnviromentData data)
    {
        var result = new CoreGen.DungeonEnviromentData
        {
            HallVariations = data.HallVariations,
            RoomVariations = data.RoomVariations,
        };
        foreach (var mash in data.BattleMashes)
        {
            result.BattleMashes.Add(new CoreGen.DungeonBattleMash
            {
                MashId = mash.MashId,
                HallEncounters = ConvertEncounters(mash.HallEncounters),
                RoomEncounters = ConvertEncounters(mash.RoomEncounters),
                BossEncounters = ConvertEncounters(mash.BossEncounters),
                StallEncounters = ConvertEncounters(mash.StallEncounters),
            });
        }
        result.HallCurios = ConvertProps(data.HallCurios);
        result.RoomCurios = ConvertProps(data.RoomCurios);
        result.RoomTresures = ConvertProps(data.RoomTresures);
        result.Traps = ConvertProps(data.Traps);
        result.Obstacles = ConvertProps(data.Obstacles);
        result.SecretTresures = ConvertProps(data.SecretTresures);
        return result;
    }

    private static List<CoreGen.DungeonBattleEncounter> ConvertEncounters(List<DungeonBattleEncounter> encounters)
    {
        var result = new List<CoreGen.DungeonBattleEncounter>();
        if (encounters == null)
            return result;
        foreach (var encounter in encounters)
            result.Add(new CoreGen.DungeonBattleEncounter(encounter.Chance, encounter.MonsterSet));
        return result;
    }

    private static List<CoreGen.DungeonPropsEncounter> ConvertProps(List<DungeonPropsEncounter> props)
    {
        var result = new List<CoreGen.DungeonPropsEncounter>();
        if (props == null)
            return result;
        foreach (var prop in props)
            result.Add(new CoreGen.DungeonPropsEncounter(prop.Chance, prop.PropName));
        return result;
    }

    private static Dungeon MapDungeon(CoreGen.Dungeon core)
    {
        var dungeon = new Dungeon
        {
            Name = core.Name,
            GridSizeX = core.GridSizeX,
            GridSizeY = core.GridSizeY,
            StartingRoomId = core.StartingRoomId,
            TotalRoomBattles = core.TotalRoomBattles,
            RoomGuardedCurio = core.RoomGuardedCurio,
            RoomGuardedTresure = core.RoomGuardedTresure,
            HallwayBattles = core.HallwayBattles,
            HallwayTraps = core.HallwayTraps,
            HallwayObstacles = core.HallwayObstacles,
            HallwayCurios = core.HallwayCurios,
            HallwayHunger = core.HallwayHunger,
        };

        foreach (var pair in core.Rooms)
        {
            var coreRoom = pair.Value;
            var room = new DungeonRoom(coreRoom.Id, coreRoom.GridX, coreRoom.GridY)
            {
                Type = coreRoom.Type,
                TextureId = coreRoom.TextureId,
                BattleEncounter = MapEncounter(coreRoom.BattleEncounter),
                Prop = MapProp(coreRoom.Prop),
            };
            foreach (var coreDoor in coreRoom.Doors)
                room.Doors.Add(new Door(coreRoom.Id, coreDoor.TargetArea, ToUnityDirection(coreDoor.Direction)));
            dungeon.Rooms.Add(room.Id, room);
        }

        foreach (var pair in core.Hallways)
        {
            var coreHall = pair.Value;
            var hallway = new Hallway(coreHall.Id)
            {
                RoomA = dungeon.Rooms[coreHall.RoomA.Id],
                RoomB = dungeon.Rooms[coreHall.RoomB.Id],
            };
            foreach (var coreSector in coreHall.Halls)
            {
                if (coreSector.Type == AreaType.Door && coreSector.Prop is CoreGen.Door)
                {
                    var door = (CoreGen.Door)coreSector.Prop;
                    hallway.Halls.Add(new HallSector(coreSector.Id, coreSector.GridX, coreSector.GridY, hallway,
                        new Door(coreSector.Id, door.TargetArea, ToUnityDirection(door.Direction))));
                }
                else
                {
                    hallway.Halls.Add(new HallSector(coreSector.Id, coreSector.GridX, coreSector.GridY, hallway)
                    {
                        Type = coreSector.Type,
                        TextureId = coreSector.TextureId,
                        BattleEncounter = MapEncounter(coreSector.BattleEncounter),
                        Prop = MapProp(coreSector.Prop),
                    });
                }
            }
            dungeon.Hallways.Add(hallway.Id, hallway);
        }

        return dungeon;
    }

    private static BattleEncounter MapEncounter(CoreGen.BattleEncounter core)
    {
        return core == null ? null : new BattleEncounter(core.Monsters);
    }

    private static Prop MapProp(Prop coreProp)
    {
        if (coreProp == null || coreProp.StringId == null)
            return null;

        if (coreProp is CoreGen.Trap)
            return new Trap(coreProp.StringId);
        if (coreProp is CoreGen.Obstacle)
            return new Obstacle(coreProp.StringId);

        Curio curio;
        if (DarkestDungeonManager.Data.Curios.TryGetValue(coreProp.StringId, out curio))
            return curio;
        return new Curio(coreProp.StringId);
    }

    private static void PopulateQuestGoals(Dungeon dungeon, Quest quest, CoreGen.Dungeon core,
        DungeonEnviromentData envData)
    {
        switch (quest.Goal.Type)
        {
            case "kill_monster":
                var killData = quest.Goal.QuestData as QuestKillMonsterData;
                if (killData != null)
                {
                    var bossCoreRoom = FindLongestPathRoom(core);
                    var bossRoom = dungeon.Rooms[bossCoreRoom.Id];
                    bossRoom.Type = AreaType.Boss;
                    var bossEncounter = envData.BattleMashes.Find(mash => mash.MashId == quest.Difficulty).
                        BossEncounters.Find(encounter => encounter.MonsterSet.Contains(killData.MonsterNameIds[0]));
                    if (bossEncounter != null)
                        bossRoom.BattleEncounter = new BattleEncounter(bossEncounter.MonsterSet);
                }
                break;
            case "activate":
                var activateData = quest.Goal.QuestData as QuestActivateData;
                if (activateData != null)
                {
                    var lastPath = FindLongestPath(core);
                    for (int i = 0; i < activateData.Amount; i++)
                    {
                        var availableRooms = core.Rooms.Values.Where(room =>
                            room.MinPath >= (float)i / activateData.Amount * lastPath &&
                            room.MinPath <= (float)(i + 1) / activateData.Amount * lastPath).ToList();
                        if (availableRooms.Count == 0)
                            break;
                        int randomRoom = UnityEngine.Random.Range(0, availableRooms.Count - 1);
                        var questRoom = dungeon.Rooms[availableRooms[randomRoom].Id];
                        if (questRoom.Type == AreaType.Empty)
                        {
                            questRoom.Type = AreaType.Curio;
                            questRoom.Prop = new Curio(activateData.CurioName) { IsQuestCurio = true };
                        }
                        else
                            i--;
                    }
                }
                break;
            case "gather":
                var gatherData = quest.Goal.QuestData as QuestGatherData;
                if (gatherData != null)
                {
                    var lastPath = FindLongestPath(core);
                    for (int i = 0; i < gatherData.Item.Amount; i++)
                    {
                        var availableRooms = core.Rooms.Values.Where(room =>
                            room.MinPath >= (float)i / gatherData.Item.Amount * lastPath &&
                            room.MinPath <= (float)(i + 1) / gatherData.Item.Amount * lastPath).ToList();
                        if (availableRooms.Count == 0)
                            break;
                        int randomRoom = UnityEngine.Random.Range(0, availableRooms.Count - 1);
                        var questRoom = dungeon.Rooms[availableRooms[randomRoom].Id];
                        if (questRoom.Type == AreaType.Empty)
                        {
                            questRoom.Type = AreaType.Curio;
                            questRoom.Prop = new Curio(gatherData.CurioName) { IsQuestCurio = true };
                        }
                        else
                            i--;
                    }
                }
                break;
        }
    }

    private static int FindLongestPath(CoreGen.Dungeon core)
    {
        return core.Rooms.Values.Max(room => room.MinPath);
    }

    private static CoreGen.DungeonRoom FindLongestPathRoom(CoreGen.Dungeon core)
    {
        int maxPath = FindLongestPath(core);
        var maxRooms = core.Rooms.Values.Where(room => room.MinPath == maxPath).ToList();
        return maxRooms.Count > 0 ? maxRooms[UnityEngine.Random.Range(0, maxRooms.Count)] : core.Rooms[core.StartingRoomId];
    }

    private static Direction ToUnityDirection(CoreGen.Direction direction)
    {
        switch (direction)
        {
            case CoreGen.Direction.Top:
                return Direction.Top;
            case CoreGen.Direction.Bot:
                return Direction.Bot;
            case CoreGen.Direction.Left:
                return Direction.Left;
            default:
                return Direction.Right;
        }
    }
}