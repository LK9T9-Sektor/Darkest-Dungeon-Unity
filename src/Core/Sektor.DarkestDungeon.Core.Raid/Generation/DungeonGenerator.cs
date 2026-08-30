using System.Collections.Generic;
using System.Linq;
using Sektor.DarkestDungeon.Core.Common;

namespace Sektor.DarkestDungeon.Core.Raid.Generation
{
    /// <summary>
    /// Pure procedural dungeon generator. Produces a <see cref="Dungeon"/> (rooms, hallways, area
    /// types and environment props) from the generation data, the environment data and a seeded
    /// random source. Quest-goal placement is intentionally left to the caller.
    /// </summary>
    public static class DungeonGenerator
    {
        private const int GridStep = 7;

        /// <summary>Generates a dungeon from the given data.</summary>
        /// <param name="genData">The generation parameters.</param>
        /// <param name="envData">The region environment data.</param>
        /// <param name="dungeonName">The dungeon region id.</param>
        /// <param name="difficulty">The difficulty mash id (1-based).</param>
        /// <param name="rng">The seeded random source.</param>
        /// <returns>The generated dungeon.</returns>
        public static Dungeon Generate(DungeonGenerationData genData, DungeonEnviromentData envData,
            string dungeonName, int difficulty, IRng rng)
        {
            var dungeon = new Dungeon { Name = dungeonName };
            int roomsLeft = genData.BaseRoomNumber;
            int hallsLeft = genData.BaseCorridorNumber;
            int xSize = genData.GridSizeX;
            int ySize = genData.GridSizeY;
            dungeon.GridSizeX = xSize;
            dungeon.GridSizeY = ySize;

            var areas = new List<GenRoom>();
            var areaGrid = new GenRoom[xSize, ySize];

            GenerateRooms(areas, areaGrid, roomsLeft, xSize, ySize, rng);
            var hub = FindMaxConnectivityRoom(areas);

            var existingRooms = ForceBorderRooms(areas, hub, roomsLeft, rng);
            var existingHalls = ForceHallConnection(existingRooms, hallsLeft);

            foreach (var pair in CreateFinalRooms(existingRooms))
                dungeon.Rooms[pair.Key] = pair.Value;
            foreach (var pair in CreateFinalHallways(dungeon, existingHalls, genData))
                dungeon.Hallways[pair.Key] = pair.Value;

            MarkEntrance(dungeon, rng);
            RecomputeMinPaths(dungeon);
            PopulateRooms(dungeon, genData, rng);
            LoadRoomEnviroment(dungeon, envData, difficulty, rng);
            PopulateHalls(dungeon, genData, rng);
            LoadHallEnviroment(dungeon, envData, difficulty, rng);

            dungeon.GridSizeX = 1 + (xSize - 1) * GridStep;
            dungeon.GridSizeY = 1 + (ySize - 1) * GridStep;
            return dungeon;
        }

        private static void GenerateRooms(List<GenRoom> areas, GenRoom[,] areaGrid, int roomsLeft, int xSize, int ySize, IRng rng)
        {
            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    areaGrid[i, j] = new GenRoom(i, j) { Id = string.Format("room{0}_{1}", i + 1, j + 1) };
                    areas.Add(areaGrid[i, j]);
                }

            var emptyAreas = new List<GenRoom>(areas);
            for (int i = 0; i < roomsLeft; i++)
            {
                int index = rng.Next(emptyAreas.Count);
                emptyAreas[index].Exists = true;
                emptyAreas.RemoveAt(index);
            }

            for (int j = 0; j < ySize; j++)
            {
                for (int i = 1; i < xSize; i++)
                {
                    var hall = new GenHall
                    {
                        RoomA = areaGrid[i, j],
                        RoomB = areaGrid[i - 1, j],
                        Id = string.Format("hall{0}_{1}", areaGrid[i, j].Id, areaGrid[i - 1, j].Id),
                    };
                    areaGrid[i, j].Left = hall;
                    areaGrid[i - 1, j].Right = hall;
                }
            }

            for (int i = 0; i < xSize; i++)
            {
                for (int j = 1; j < ySize; j++)
                {
                    var hall = new GenHall
                    {
                        RoomA = areaGrid[i, j],
                        RoomB = areaGrid[i, j - 1],
                        Id = string.Format("hall{0}_{1}", areaGrid[i, j].Id, areaGrid[i, j - 1].Id),
                    };
                    areaGrid[i, j].Bot = hall;
                    areaGrid[i, j - 1].Top = hall;
                }
            }
        }

        private static GenRoom FindMaxConnectivityRoom(List<GenRoom> areas)
        {
            GenRoom room = null;
            int maxConnectivity = -1;
            foreach (var generatedRoom in areas)
            {
                if (!generatedRoom.Exists)
                    continue;
                if (generatedRoom.BorderingRooms <= maxConnectivity)
                    continue;

                room = generatedRoom;
                maxConnectivity = generatedRoom.BorderingRooms;
            }
            return room;
        }

        private static GenRoom FindLongestPathRoom(GenRoom entrance, List<GenRoom> areas, IRng rng)
        {
            CalculateMinPath(entrance, 0);

            int maxPath = areas.Max(area => area.MinPath);
            var maxRooms = areas.FindAll(area => area.MinPath == maxPath);
            return maxRooms.Count > 0 ? maxRooms[rng.Next(maxRooms.Count)] : null;
        }

        private static List<GenRoom> FindBorderingRooms(GenRoom fromRoom, List<GenRoom> visited = null)
        {
            if (visited == null)
                visited = new List<GenRoom>();

            if (fromRoom == null || !fromRoom.Exists)
                return visited;

            visited.Add(fromRoom);
            if (!visited.Contains(fromRoom.LeftRoom))
                FindBorderingRooms(fromRoom.LeftRoom, visited);
            if (!visited.Contains(fromRoom.RightRoom))
                FindBorderingRooms(fromRoom.RightRoom, visited);
            if (!visited.Contains(fromRoom.TopRoom))
                FindBorderingRooms(fromRoom.TopRoom, visited);
            if (!visited.Contains(fromRoom.BotRoom))
                FindBorderingRooms(fromRoom.BotRoom, visited);

            return visited;
        }

        private static List<GenRoom> ForceBorderRooms(List<GenRoom> areas, GenRoom hub, int roomNumber, IRng rng)
        {
            var visitedAreas = FindBorderingRooms(hub);

            while (visitedAreas.Count != roomNumber)
            {
                foreach (var area in areas)
                {
                    if (area.Exists && !visitedAreas.Contains(area))
                    {
                        area.Exists = false;
                        var availableRooms = areas.FindAll(item => !item.Exists && item.BorderingRooms > 0);
                        var newRandomArea = availableRooms[rng.Next(availableRooms.Count)];
                        newRandomArea.Exists = true;
                        break;
                    }
                }
                visitedAreas = FindBorderingRooms(hub);
            }
            return visitedAreas;
        }

        private static List<GenHall> ForceHallConnection(List<GenRoom> existingRooms, int hallNumber)
        {
            var existingHalls = new List<GenHall>();
            foreach (var room in existingRooms)
            {
                AddConnectingHall(existingHalls, room.Left);
                AddConnectingHall(existingHalls, room.Right);
                AddConnectingHall(existingHalls, room.Top);
                AddConnectingHall(existingHalls, room.Bot);
            }
            return existingHalls;
        }

        private static void AddConnectingHall(List<GenHall> existingHalls, GenHall hall)
        {
            if (hall != null && hall.RoomsExist && !existingHalls.Contains(hall))
            {
                hall.Exists = true;
                existingHalls.Add(hall);
            }
        }

        private static Dictionary<string, DungeonRoom> CreateFinalRooms(List<GenRoom> rooms)
        {
            var finalAreas = new Dictionary<string, DungeonRoom>();
            foreach (var genRoom in rooms)
            {
                var room = new DungeonRoom(genRoom.Id, 1 + (genRoom.GridX - 1) * GridStep, 1 + (genRoom.GridY - 1) * GridStep)
                {
                    MinPath = genRoom.MinPath,
                };

                if (genRoom.Left != null && genRoom.Left.Exists)
                    room.Doors.Add(new Door(genRoom.Id, genRoom.Left.Id, Direction.Left));
                if (genRoom.Right != null && genRoom.Right.Exists)
                    room.Doors.Add(new Door(genRoom.Id, genRoom.Right.Id, Direction.Right));
                if (genRoom.Top != null && genRoom.Top.Exists)
                    room.Doors.Add(new Door(genRoom.Id, genRoom.Top.Id, Direction.Top));
                if (genRoom.Bot != null && genRoom.Bot.Exists)
                    room.Doors.Add(new Door(genRoom.Id, genRoom.Bot.Id, Direction.Bot));

                finalAreas.Add(room.Id, room);
            }

            return finalAreas;
        }

        private static Dictionary<string, Hallway> CreateFinalHallways(Dungeon dungeon, List<GenHall> halls, DungeonGenerationData genData)
        {
            var finalHallways = new Dictionary<string, Hallway>();

            foreach (var genHall in halls)
            {
                var hallway = new Hallway(genHall.Id);

                hallway.RoomA = dungeon.Rooms[genHall.RoomA.Id];
                hallway.RoomB = dungeon.Rooms[genHall.RoomB.Id];
                int hallIncrementX = 0, hallIncrementY = 0;
                int hallGridX = hallway.RoomA.GridX, hallGridY = hallway.RoomA.GridY;

                if (hallway.RoomA.GridX < hallway.RoomB.GridX)
                    hallIncrementX = 1;
                else if (hallway.RoomA.GridX > hallway.RoomB.GridX)
                    hallIncrementX = -1;

                if (hallway.RoomA.GridY < hallway.RoomB.GridY)
                    hallIncrementY = 1;
                else if (hallway.RoomA.GridY > hallway.RoomB.GridY)
                    hallIncrementY = -1;

                hallGridX += hallIncrementX;
                hallGridY += hallIncrementY;

                hallway.Halls.Add(new HallSector(hallway.Id + "_0", hallGridX, hallGridY, hallway,
                    new Door(hallway.Id, genHall.RoomA.Id, Direction.Left)));

                for (int i = 1; i <= genData.Spacing; i++)
                {
                    hallGridX += hallIncrementX;
                    hallGridY += hallIncrementY;
                    hallway.Halls.Add(new HallSector(hallway.Id + "_" + i, hallGridX, hallGridY, hallway));
                }

                hallGridX += hallIncrementX;
                hallGridY += hallIncrementY;
                hallway.Halls.Add(new HallSector(hallway.Id + "_" + (genData.Spacing + 1), hallGridX, hallGridY,
                    hallway, new Door(hallway.Id, genHall.RoomB.Id, Direction.Right)));

                finalHallways.Add(hallway.Id, hallway);
            }

            return finalHallways;
        }

        private static void CalculateMinPath(GenRoom room, int currentPath)
        {
            if (room == null || room.MinPath <= currentPath)
                return;

            room.MinPath = currentPath;

            if (room.Left != null && room.Left.Exists)
                CalculateMinPath(room.LeftRoom, currentPath + 1);
            if (room.Right != null && room.Right.Exists)
                CalculateMinPath(room.RightRoom, currentPath + 1);
            if (room.Top != null && room.Top.Exists)
                CalculateMinPath(room.TopRoom, currentPath + 1);
            if (room.Bot != null && room.Bot.Exists)
                CalculateMinPath(room.BotRoom, currentPath + 1);
        }

        private static void RecomputeMinPaths(Dungeon dungeon)
        {
            if (string.IsNullOrEmpty(dungeon.StartingRoomId) || !dungeon.Rooms.ContainsKey(dungeon.StartingRoomId))
                return;

            foreach (var room in dungeon.Rooms.Values)
                room.MinPath = int.MaxValue;

            var start = dungeon.Rooms[dungeon.StartingRoomId];
            start.MinPath = 0;

            var queue = new Queue<DungeonRoom>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var hallway in dungeon.Hallways.Values)
                {
                    if (hallway.RoomA != current && hallway.RoomB != current)
                        continue;

                    var neighbor = hallway.RoomA == current ? hallway.RoomB : hallway.RoomA;
                    if (neighbor.MinPath <= current.MinPath + 1)
                        continue;
                    neighbor.MinPath = current.MinPath + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        private static void MarkEntrance(Dungeon dungeon, IRng rng)
        {
            int minConnections = 5;
            DungeonRoom entranceRoom = null;
            foreach (var room in dungeon.Rooms.Values)
            {
                if (room.Connections < minConnections)
                {
                    minConnections = room.Connections;
                    entranceRoom = room;
                }
                else if (room.Connections == minConnections)
                {
                    if (rng.Next(2) == 1)
                    {
                        minConnections = room.Connections;
                        entranceRoom = room;
                    }
                }
            }
            if (entranceRoom == null)
            {
                var rooms = dungeon.Rooms.Values.ToList();
                entranceRoom = rooms[rng.Next(rooms.Count)];
            }

            entranceRoom.Type = AreaType.Entrance;
            dungeon.StartingRoomId = entranceRoom.Id;
        }

        private static void PopulateRooms(Dungeon dungeon, DungeonGenerationData genData, IRng rng)
        {
            var rooms = dungeon.Rooms.Values.Where(item => item.Type == AreaType.Empty).ToList();

            dungeon.TotalRoomBattles = rng.Next(genData.TotalRoomBattleMin, genData.TotalRoomBattleMax + 1);
            int currentBattles = 0;
            int maxBattles = dungeon.TotalRoomBattles;

            int guardedTresures = rng.Next(genData.RoomGuardedTresureMin, genData.RoomGuardedTresureMax + 1);
            dungeon.RoomGuardedTresure = Clamp(guardedTresures, 0, maxBattles - currentBattles);
            currentBattles += dungeon.RoomGuardedTresure;

            for (int i = 0; i < dungeon.RoomGuardedTresure; i++)
            {
                if (rooms.Count == 0)
                    return;

                int index = rng.Next(rooms.Count);
                rooms[index].Type = AreaType.BattleTresure;
                rooms.RemoveAt(index);
            }

            int guardedCurios = rng.Next(genData.RoomGuardedCurioMin, genData.RoomGuardedCurioMax + 1);
            dungeon.RoomGuardedCurio = Clamp(guardedCurios, 0, maxBattles - currentBattles);
            currentBattles += dungeon.RoomGuardedCurio;

            for (int i = 0; i < dungeon.RoomGuardedCurio; i++)
            {
                if (rooms.Count == 0)
                    return;

                int index = rng.Next(rooms.Count);
                rooms[index].Type = AreaType.BattleCurio;
                rooms.RemoveAt(index);
            }

            for (int i = currentBattles; i < dungeon.TotalRoomBattles; i++)
            {
                if (rooms.Count == 0)
                    return;

                int index = rng.Next(rooms.Count);
                rooms[index].Type = AreaType.Battle;
                rooms.RemoveAt(index);
            }
        }

        private static void PopulateHalls(Dungeon dungeon, DungeonGenerationData genData, IRng rng)
        {
            var hallSectors = new List<HallSector>();
            foreach (var hallway in dungeon.Hallways.Values)
                foreach (var hallSector in hallway.Halls)
                    if (hallSector.Type != AreaType.Door)
                        hallSectors.Add(hallSector);

            dungeon.HallwayBattles = rng.Next(genData.HallwayBattleMin, genData.HallwayBattleMax + 1);
            for (int i = 0; i < dungeon.HallwayBattles; i++)
            {
                if (hallSectors.Count == 0)
                    return;

                int index = rng.Next(hallSectors.Count);
                hallSectors[index].Type = AreaType.Battle;
                hallSectors.RemoveAt(index);
            }

            dungeon.HallwayTraps = rng.Next(genData.HallwayTrapMin, genData.HallwayTrapMax + 1);
            for (int i = 0; i < dungeon.HallwayTraps; i++)
            {
                if (hallSectors.Count == 0)
                    return;

                int index = rng.Next(hallSectors.Count);
                hallSectors[index].Type = AreaType.Trap;
                hallSectors.RemoveAt(index);
            }

            dungeon.HallwayObstacles = rng.Next(genData.HallwayObstacleMin, genData.HallwayObstacleMax + 1);
            for (int i = 0; i < dungeon.HallwayObstacles; i++)
            {
                if (hallSectors.Count == 0)
                    return;

                int index = rng.Next(hallSectors.Count);
                hallSectors[index].Type = AreaType.Obstacle;
                hallSectors.RemoveAt(index);
            }

            dungeon.HallwayCurios = rng.Next(genData.HallwayCurioMin, genData.HallwayCurioMax + 1);
            for (int i = 0; i < dungeon.HallwayCurios; i++)
            {
                if (hallSectors.Count == 0)
                    return;

                int index = rng.Next(hallSectors.Count);
                hallSectors[index].Type = AreaType.Curio;
                hallSectors.RemoveAt(index);
            }

            dungeon.HallwayHunger = rng.Next(genData.HallwayHungerMin, genData.HallwayHungerMax + 1);
            for (int i = 0; i < dungeon.HallwayHunger; i++)
            {
                if (hallSectors.Count == 0)
                    return;

                int index = rng.Next(hallSectors.Count);
                hallSectors[index].Type = AreaType.Hunger;
                hallSectors.RemoveAt(index);
            }
        }

        private static void LoadRoomEnviroment(Dungeon dungeon, DungeonEnviromentData envData, int mashIndex, IRng rng)
        {
            foreach (var room in dungeon.Rooms.Values)
            {
                room.TextureId = envData.RoomVariations[rng.Next(envData.RoomVariations.Count)];
                switch (room.Type)
                {
                    case AreaType.Battle:
                        room.BattleEncounter = new BattleEncounter(
                            ChooseEncounter(envData, mashIndex, rng).MonsterSet);
                        break;
                    case AreaType.BattleCurio:
                        if (room.Prop == null)
                        {
                            string curioName = rng.Next(0, 100) >= 0
                                ? ChoosePropName(envData.RoomCurios, rng)
                                : null;
                            if (curioName != null)
                                room.Prop = new Curio(curioName);
                            room.BattleEncounter = new BattleEncounter(
                                ChooseEncounter(envData, mashIndex, rng).MonsterSet);
                        }
                        break;
                    case AreaType.BattleTresure:
                        if (room.Prop == null)
                        {
                            string tresureName = ChoosePropName(envData.RoomTresures, rng);
                            if (tresureName != null)
                                room.Prop = new Curio(tresureName);
                        }
                        room.BattleEncounter = new BattleEncounter(
                            ChooseEncounter(envData, mashIndex, rng).MonsterSet);
                        break;
                }
            }
        }

        private static void LoadHallEnviroment(Dungeon dungeon, DungeonEnviromentData envData, int mashIndex, IRng rng)
        {
            foreach (var hall in dungeon.Hallways.Values)
            {
                foreach (var sector in hall.Halls)
                {
                    sector.TextureId = (rng.Next(1, envData.HallVariations + 1)).ToString();
                    switch (sector.Type)
                    {
                        case AreaType.Battle:
                            sector.BattleEncounter = new BattleEncounter(
                                ChooseEncounter(envData, mashIndex, rng).MonsterSet);
                            break;
                        case AreaType.Curio:
                            if (sector.Prop == null)
                            {
                                string curioName = ChoosePropName(envData.HallCurios, rng);
                                if (curioName != null)
                                    sector.Prop = new Curio(curioName);
                            }
                            break;
                        case AreaType.Obstacle:
                            string obstacleName = ChoosePropName(envData.Obstacles, rng);
                            if (obstacleName != null)
                                sector.Prop = new Obstacle(obstacleName);
                            break;
                        case AreaType.Trap:
                            string trapName = ChoosePropName(envData.Traps, rng);
                            if (trapName != null)
                                sector.Prop = new Trap(trapName);
                            break;
                    }
                }
            }
        }

        private static DungeonBattleEncounter ChooseEncounter(DungeonEnviromentData envData, int mashIndex, IRng rng)
        {
            var mash = envData.BattleMashes.FirstOrDefault(item => item.MashId == mashIndex);
            var encounters = mash != null ? mash.RoomEncounters : new List<DungeonBattleEncounter>();
            return ChooseByRandom(encounters, rng) ?? new DungeonBattleEncounter();
        }

        private static string ChoosePropName(List<DungeonPropsEncounter> props, IRng rng)
        {
            var chosen = ChooseByRandom(props, rng);
            return chosen != null ? chosen.PropName : null;
        }

        private static T ChooseByRandom<T>(List<T> items, IRng rng) where T : IProportionValue
        {
            if (items == null || items.Count == 0)
                return default(T);

            int total = items.Sum(item => item.Chance > 0 ? item.Chance : 0);
            int roll = rng.Next(total);
            foreach (var item in items)
            {
                if (roll < item.Chance)
                    return item;
                roll -= item.Chance;
            }
            return items[items.Count - 1];
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        /// <summary>Working room node during generation.</summary>
        private class GenRoom
        {
            public string Id;
            public bool Exists;
            public int MinPath = int.MaxValue;

            public readonly int GridX;
            public readonly int GridY;

            public GenHall Left;
            public GenHall Right;
            public GenHall Top;
            public GenHall Bot;

            public GenRoom LeftRoom { get { return Left == null ? null : Left.GetOpposite(this); } }
            public GenRoom RightRoom { get { return Right == null ? null : Right.GetOpposite(this); } }
            public GenRoom TopRoom { get { return Top == null ? null : Top.GetOpposite(this); } }
            public GenRoom BotRoom { get { return Bot == null ? null : Bot.GetOpposite(this); } }

            public int BorderingRooms
            {
                get
                {
                    int conns = 0;
                    if (Left != null && Left.GetOpposite(this).Exists)
                        conns++;
                    if (Right != null && Right.GetOpposite(this).Exists)
                        conns++;
                    if (Top != null && Top.GetOpposite(this).Exists)
                        conns++;
                    if (Bot != null && Bot.GetOpposite(this).Exists)
                        conns++;
                    return conns;
                }
            }

            public GenRoom(int x, int y)
            {
                GridX = x;
                GridY = y;
                Exists = false;
            }
        }

        /// <summary>Working hallway node during generation.</summary>
        private class GenHall
        {
            public string Id;
            public bool Exists;

            public GenRoom RoomA;
            public GenRoom RoomB;

            public bool RoomsExist { get { return RoomA.Exists && RoomB.Exists; } }

            public GenRoom GetOpposite(GenRoom room)
            {
                if (RoomA.Id == room.Id)
                    return RoomB;
                return RoomA;
            }

            public GenHall()
            {
                Exists = false;
            }
        }
    }
}