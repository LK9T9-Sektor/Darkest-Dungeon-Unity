# dungeon_generation.md — Генерация подземелья (`DungeonGenerator`)

> Домен: `raid` (ядро `Core.Raid\Generation` + клиентская граница). Статус: **реализовано**
> (топология + население + enviroment + quest-цели в ядре).

## 1. Назначение и когда работает

Процедурная генерация подземелья для рейда: из параметров (`MapGenerator.txt`) и среды региона
(`Dungeons/<Env>.bytes`) создаётся карта — комнаты, коридоры, двери, типы областей (бой/curio/
ловушка/голод/вход), environment-пропы и quest-цели (boss/curio-комнаты). Ядро генерирует **чистый
результат** `Dungeon` (топология + население + enviroment + quest-цели через `ApplyQuestGoal`).
Срабатывает при старте рейда.

## 2. Модель данных (ядро, `Core.Raid\Generation`)

- **Вход-данные:**
  - `DungeonGenerationData` — параметры из `MapGenerator.txt`: `Length`/`QuestType`/`Dungeon`,
    `BaseRoomNumber`, `BaseCorridorNumber`, `GridSizeX/Y`, `Spacing`, `GoalRoomNumber`,
    `MinFinalDistance`, min/max по `HallwayBattle/Trap/Obstacle/Curio/Hunger`,
    `TotalRoomBattle`, `RoomGuardedCurio/Tresure`.
  - `DungeonEnviromentData` — среда региона: `HallVariations`, `RoomVariations`,
    `BattleMashes` (пулы энкаунтеров по сложности), проп-пулы `HallCurios`/`RoomCurios`/
    `RoomTresures`/`Traps`/`Obstacles`/`SecretTresures`.
  - `DungeonBattleMash` — `MashId`, `Hall/Room/Boss/StallEncounters`, `NamedEncounters`.
  - `DungeonBattleEncounter` (`Chance`, `MonsterSet`), `DungeonPropsEncounter` (`Chance`, `PropName`)
    — оба `IProportionValue` (для `ChooseByRandom`).
- **Модель результата:**
  - `Dungeon` — `Name`, `GridSizeX/Y`, `StartingRoomId`, счётчики, `Rooms`/`Hallways`.
  - `DungeonRoom : Area` — `Connections` (Doors), `MinPath` (расстояние от входа, для quest-целей).
  - `Hallway` — `RoomA/RoomB`, `Halls` (секторы), `DirectionFromA/B`.
  - `HallSector : Area` — сектор коридора (включая door-секторы).
  - `Door : Prop` — `TargetArea`, `Direction` (ядровой).
  - `Curio`/`Trap`/`Obstacle` — пропы (`Prop.Type` задаёт `AreaType`).
  - enums `Direction` (Top/Bot/Left/Right), `Knowledge` (Hidden/Scouted/Visited/Completed).
- **`IRng`** (`Core.Common`) — детерминированный источник случайности (ядро не использует
  глобальный `RandomSolver`; `SystemRandomRng(seed)` — реализация).

## 3. Парсинг контента

- `DungeonGenerationDataParser.Parse(text)` — DSL `MapGenerator.txt`: блоки `map:` → entry;
  строки `.key value` / `.key min max`; значения собираются по ключу до следующего `.key`
  (`gridsize 4 3` → `"4 3"`).
- `DungeonEnviromentDataParser.Parse(text)` — DSL `Dungeons/*.bytes`: секция `mash:` → `BattleMashes`
  (строки `hall:`/`room:`/`boss:`/`stall:`/`named:` с `.chance N .types a b c`), секция `props:`
  → проп-пулы (`hall_curios:`/`room_curios:`/`room_treasures:`/`traps:`/`obstacles:`/
  `secret_room_treasures:`), корневые `hall_variants`/`room_variants`/`id`.
- Фасады: `Clients.Content\GameDataReader.ReadDungeonGenerationData` / `ReadDungeonEnviromentData`.

## 4. Порядок срабатывания (трассировка)

`Core.Raid.Generation.DungeonGenerator.Generate(genData, envData, dungeonName, difficulty, rng)`
(`Core.Raid\Generation\DungeonGenerator.cs`):

1. Инициализация `Dungeon`, счётчики из `genData` (`GridSizeX/Y`, `roomsLeft`, `hallsLeft`).
2. `GenerateRooms` — сетка `GenRoom[xSize,ySize]`, случайный выбор `roomsLeft` существующих,
   построение коридоров `GenHall` между соседями (Left/Right/Top/Bot).
3. `FindMaxConnectivityRoom` — «хаб» (максимум соседей).
4. `ForceBorderRooms` — доводит число существующих комнат до `roomsLeft` (выключает/включает
   комнаты, пока связность от хаба не совпадёт).
5. `ForceHallConnection` — включает коридоры между существующими комнатами (до `hallsLeft`;
   **пустышка**: реальное доборение/удаление коридоров — TODO, как в Unity).
6. `CreateFinalRooms` / `CreateFinalHallways` — финальные `DungeonRoom`/`Hallway`/`HallSector`/`Door`
   (координаты `1+(x-1)*7`, `Spacing` секторов между комнатами).
7. `MarkEntrance` — стартовая комната (минимальные связи, рандом при равных).
8. `RecomputeMinPaths` — BFS от входа по `Hallways` → `DungeonRoom.MinPath`.
9. `PopulateRooms` — `TotalRoomBattles`, `RoomGuardedTresure`/`RoomGuardedCurio` (клэмп по
   `maxBattles - current`), затем обычные `Battle` комнаты (случайные из `Empty`).
10. `LoadRoomEnviroment` — `TextureId` из `RoomVariations`; `Battle`/`BattleCurio`/`BattleTresure`
    получают `BattleEncounter` (из `RoomEncounters` mash'а по сложности) и пропы
    (`RoomCurios`/`RoomTresures`).
11. `PopulateHalls` — `HallwayBattles/Traps/Obstacles/Curios/Hunger` на случайные не-door секторы.
12. `LoadHallEnviroment` — `TextureId` секторов (`1..HallVariations`); `Battle` → энкаунтер,
    `Curio`/`Obstacle`/`Trap` → пропы из пулов.
13. Финальные `GridSizeX/Y = 1 + (xSize-1)*7`.

**Quest-цели** — `DungeonGenerator.ApplyQuestGoal(dungeon, goal, envData, difficulty, rng)`
(после `Generate`):
- `kill_monster` → boss-комната (`AreaType.Boss`) на самом длинном пути по `MinPath` +
  `BattleEncounter` из `BossEncounters` (по первому `MonsterNameIds`);
- `activate` → `Amount` curio-комнат (`IsQuestCurio`), распределённых по `MinPath`-сегментам
  (`i/Amount * lastPath .. (i+1)/Amount * lastPath`);
- `gather` → то же + `CurioInteraction`("loot") с `CurioResult(item)`.

**Unity-адаптер** (`unity\Assets\Scripts\Generation\DungeonGenerator.cs`):
- `GenerateDungeon(Quest, seed)` → конвертит Unity `DungeonGenerationData`/`DungeonEnviromentData`
  в ядровые → `CoreGen.DungeonGenerator.Generate` → конвертит `quest.Goal` в `DungeonQuestGoal`
  (`ConvertGoal`: kill_monster/activate/gather из `QuestKillMonsterData`/`QuestActivateData`/
  `QuestGatherData`) → `CoreGen.DungeonGenerator.ApplyQuestGoal` → мапит ядровой `Dungeon`
  в Unity-модели (`Dungeon`/`DungeonRoom`/`Hallway`/`HallSector`/`Door`/`Prop`) →
  `DungeonMash`/`SharedMash`.

## 5. Очередь и обновления

- Генерация — однократно при старте рейда (`RaidSceneManager.cs:153` → `GenerateDungeon`).
- Детерминизм: `IRng` с сидом; обе стороны дуэли/Тест-боя могут воспроизвести карту.

## 6. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Число комнат | `ForceBorderRooms` | `= BaseRoomNumber` |
| Guarded-клэмп | `PopulateRooms` | `min/max`, ограничено `maxBattles - current` |
| Координаты | `CreateFinal*` | `1+(x-1)*7`, секторы `Spacing` |
| MinPath | `RecomputeMinPaths` | BFS от входа |

## 7. Нюансы и подводные камни

- **Порядок вызовов RNG критичен** для детерминизма по сиду: `GenerateRooms` → `ForceBorderRooms`
  → `ForceHallConnection` → `MarkEntrance` → `Populate*` → `Load*Enviroment`. Менять порядок —
  менять карту для того же сида.
- **`ForceHallConnection` — пустышка (недоделка Unity-оригинала, перенесена 1-в-1):**
  - работает только «каркас»: включаются коридоры, оба конца которых — существующие комнаты
    (`RoomsExist`); результат — минимальный связный граф;
  - `while (existingHalls.Count != hallNumber)` — **мёртвый код**: `if/else if` с TODO →
    `break` сразу; цикл всегда выходит после 1-й итерации;
  - **`BaseCorridorNumber` не влияет** на число коридоров. Решение: оставлено как в Unity
    (паритет). Реализация TODO (довести до `hallNumber`) — отдельная задача с решением о паритете.
- **Quest-цели в ядре** (`ApplyQuestGoal`) — `DungeonQuestGoal` примитивная модель (без
  Unity `Quest`/`IQuestData`); Unity-адаптер конвертит `quest.Goal`. Роли quest-целей используют
  `IRng` (не `UnityEngine.Random`).
- **`MinPath` пересчитывается BFS после `MarkEntrance`** (в ядре), не из `GenRoom.MinPath`
  (который используется только в топологии).
- **Пропы** `Curio`/`Trap`/`Obstacle` — отдельные классы (тип из `Prop.Type`); не путать с
  `Door` (наследник `Prop`, `AreaType.Door`).
- **Имя `Dungeon`** в Unity и ядре совпадает — в адаптере обязателен alias
  (`CoreGen = Sektor.DarkestDungeon.Core.Raid.Generation`), иначе конфликт имён.

## 8. Взаимодействия

- Рейд-сцена (`RaidSceneManager`) потребляет `Dungeon`/`DungeonRoom`/`Hallway`.
- Encounters → бой (`BattleEncounter` на комнатах/секторах).
- Пропы → curio-взаимодействия / ловушки / препятствия.
- `IRng`/`SystemRandomRng` — `Core.Common` (детерминизм).

## 9. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.Raid/Generation/*.cs` (модели + парсеры + генератор,
  в т.ч. `DungeonQuestGoal.cs`)
- `src/Core/Sektor.DarkestDungeon.Core.Common/IRng.cs`, `SystemRandomRng.cs`
- `src/Clients/Sektor.DarkestDungeon.Clients.Content/GameDataReader.cs`
- `unity/Assets/Scripts/Generation/DungeonGenerator.cs` (адаптер),
  `DungeonGeneratorLegacy.cs` (старый, до проверки)
- `unity/Assets/Resources/Data/Mechanics/MapGenerator.txt`, `unity/Assets/Resources/Data/Dungeons/*.bytes`
- `tests/Core/Sektor.DarkestDungeon.Core.Raid.Tests/DungeonGeneratorTests.cs`