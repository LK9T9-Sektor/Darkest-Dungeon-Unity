# GAME_LOGIC.md — Игровая логика: потоки и процессы (как в коде)

> **Что это.** Документация игровой логики по коду: не числовые правила (это `GAME_RULES.md`), а
> **потоки/процессы**: как генерируются подземелья и рейды, как устроен рейд-флоу, curio/пропы,
> кампания/город, сейвы. Описывается **как реализовано в этом репозитории** (legacy `unity\Assets\Scripts`
> + ядро `src\Core`), расхождения с оригиналом DD отмечаются отдельно. Обновляется в том же коммите,
> что и код, меняющий задокументированные факты.
>
> **Карта классов:** `UNITY_LEGACY_MAP.md` (где что живёт), **статус выноса:** `EXTRACTION_STATUS.md`,
> **правила/числа:** `GAME_RULES.md`, **долг:** `KNOWN_ISSUES.md`.

---

## 1. Генерация подземелий и рейдов

### 1.1 Когда генерируется

- **Квесты** генерируются при создании/обновлении кампании: `Campaign.GenerateQuests()`
  (`unity\Assets\Scripts\Campaign\Campaign.cs:232-234`) → `QuestGenerator.GenerateQuests(campaign)`.
- **Подземелье** генерируется при старте рейда: `RaidSceneManager` (`RaidSceneManager.cs:153`) →
  `DungeonGenerator.GenerateDungeon(CurrentRaid.Quest)`.
- Оба генератора — **детерминированные**: при `seed != 0` вызывается `Random.InitState(seed)`
  (`DungeonGenerator.cs:82-83`, `QuestGenerator.cs:36-37`). Без сида — глобальный Unity-Random.

### 1.2 QuestGenerator — состав квестов на визит в город

Файл `unity\Assets\Scripts\Generation\QuestGenerator.cs`. Вход — `Campaign`; выход — `List<Quest>`.

**Число квестов** (`GetQuestNumber`, `:77-97`) — ступенчато от прогресса кампании
`campaign.QuestsComleted` (0–2 → состояние 0, 3 → 1, 4 → 2, 6 → 3, 10 → 4, 16 → 5, 20 → 6, иначе 7),
число берётся из `genData.QuestsPerVisit[state]`.

**Доступные подземелья** (`GetQuestInfo`, `:57-75`) — открыты те, где
`dungeon.RequiredQuestsCompleted <= campaign.QuestsComleted`; для каждого берётся набор типов квестов
по уровню мастерства подземелья (`QuestTypeSets[campaign.Dungeons[id].MasteryLevel]`).

**Распределение** (`DistributeQuests`, `:99-214`):
1. Клэмп: `QuestCount ≤ DungeonCount × MaxPerDungeon`, но `≥ DungeonCount`.
2. **Сложность** — пропорционально числу героев подходящего resolve-уровня
   (`Difficulties[i].ResolveLevels`): доли уровней 1/3/5 (`:108-133`). Если героев нет — фолбэк 4×1.
3. Первый проход: по одному квесту на каждый открытый данж (`:135-147`); далее добивают остатком
   случайно (`:149-170`).
4. Добавляются **plot-квесты**: текущий plot-квест кампании (`CurrentPlotQuest`, `:172-189`) и
   plot-квесты от городских событий типа `PlotQuest` (`:191-213`).

**Типы квестов** (`DistributeQuestTypes`, `:216-229`) — через `RandomSolver.ChooseByRandom`
(весовой выбор) из `GeneratedTypes` подземелья; plot-квесты не трогаются.

**Цели** (`DistributeQuestGoals`, `:231-258`) — из `QuestType.GoalLists` для данного данжа
(`"all"` или `quest.Dungeon`), случайная цель; `Quest.Goal` ссылается на `QuestDatabase.QuestGoals`.

**Награды** (`DistributeQuestRewards`, `:260-315`):
- Не-plot: `ResolveXP` + два heirloom (типы из `HeirloomTypes[quest.Dungeon]`, количество из
  `HeirloomAmounts`) + предмет из `ItemTable[difficulty][length]` + (если в `TrinketChances`
  для сложности/длины стоит 1) случайный тринкет нужной редкости.
- Plot: тринкет из редкости `PlotTrinket.Rarity`.

### 1.3 DungeonGenerator — структура подземелья

Файл `unity\Assets\Scripts\Generation\DungeonGenerator.cs`. Вход — `Quest`; выход — `Dungeon`.

**Данные** (`DungeonGenerationData`, ключ = данж+длина+тип квеста; `DungeonEnviromentData` по данжу):
`BaseRoomNumber`, `BaseCorridorNumber`, `GridSizeX/Y`, `Spacing`, диапазоны боев/ловушек/препятствий/
curio/голода по коридорам и комнатам.

**Шаги** (`GenerateDungeon`, `:80-126`):
1. `GenerateRooms` (`:128-171`): сетка `GridSizeX×GridSizeY` узлов (`GenRoom`); случайно отмечаются
   `BaseRoomNumber` комнат как существующие; между соседями создаются рёбра (`GenHall`).
2. `hub = FindMaxConnectivityRoom` (`:173-189`) — комната с максимальным числом соседей.
3. `ForceBorderRooms` (`:221-241`): пока связная компонента (BFS от hub через `FindBorderingRooms`,
   `:200-219`) не совпадёт по числу с `BaseRoomNumber` — «лишние» существующие комнаты выключаются,
   взамен включаются случайные соседние.
4. `ForceHallConnection` (`:243-295`): все рёбра между существующими комнатами помечаются
   существующими коридорами. *(TODO в коде: балансировка числа коридоров до `BaseCorridorNumber`
   не доведена — `break` на `:292`.)*
5. `CreateFinalRooms` (`:297-320`): `DungeonRoom` на «игровых» координатах
   `1 + (GridX−1)·7`; двери по сторонам.
6. `CreateFinalHallways` (`:322-367`): `Hallway` из последовательности `HallSector`-ов между комнатами
   (шаг по оси + `Spacing` промежуточных секторов + двери по краям).
7. `MarkEntrance` (`:386-414`): вход — комната с минимальным числом связей (при ничьей — случайная);
   `Type = Entrance`, `StartingRoomId`.
8. `PopulateQuestGoals` (`:532-622`) — **квест-цели**:
   - `kill_monster`: босс в самой дальней комнате от входа (`FindLongestPathRoom`, `:191-198`,
     Dijkstra-подобный `CalculateMinPath` `:369-384`); `Boss` + `BattleEncounter` из
     `BossEncounters` по типу монстра.
   - `activate` / `gather`: N целей (curio `IsQuestCurio`) распределяются по «слоям» удалённости от
     входа — комната выбирается из `MinPath`-диапазона `[i/N, (i+1)/N]·lastRoom.MinPath`
     (`:555-560`, `:593-596`).
9. `PopulateRooms` (`:416-461`): `TotalRoomBattles` боевых комнат; из них сначала
   `RoomGuardedTresure` (тип `BattleTresure`), потом `RoomGuardedCurio` (`BattleCurio`), остаток —
   `Battle`; тип ставится случайной пустой комнате.
10. `PopulateHalls` (`:463-530`): случайные сектора коридоров получают `Battle`, `Trap`,
    `Obstacle`, `Curio`, `Hunger` (количество из диапазонов данных).
11. `LoadRoomEnviroment` (`:624-669`) / `LoadHallEnviroment` (`:671-711`): для каждого типа зоны —
    `BattleEncounter` (из `BattleMashes[mashIndex].RoomEncounters` через весовой `RandomSolver.
    ChooseByRandom`) и/или `Prop` (curio/obstacle/trap по `PropName`), текстуры.
12. Финал (`:118-123`): `GridSize` в игровых координатах, `DungeonMash`/`SharedMash` по сложности,
    имя данжа.

### 1.4 Модель результата

- `Dungeon` (`Raid\Dungeon.cs`): сетка, `StartingRoomId`, счётчики (`TotalRoomBattles`,
  `RoomGuardedCurio`, `HallwayBattles`, `HallwayTraps`, ...), `DungeonMash`/`SharedMash`;
  реализует `IBinarySaveData` (сейв позиции/пропов).
- `DungeonRoom` (`Area\Room.cs`) / `HallSector` (`Area\HallSector.cs`) — наследники `Area`
  (тип `AreaType`, `Prop`, `BattleEncounter`, `Knowledge`, `Scout()`).
- `AreaType` (ядро, `Core.Content\Raid\AreaType.cs`): Empty/Entrance/Tresure/Curio/Boss/Battle/
  Trap/Hunger/Obstacle/Door/BattleCurio/BattleTresure.

### 1.5 Оригинал Darkest Dungeon

*(заполняется позже — проверка соответствия сетки/целей оригиналу.)*

---

## 2. Рейд-флоу (жизненный цикл подземелья)

Оркестратор — `RaidSceneManager` (`unity\Assets\Scripts\Managers\RaidSceneManager.cs`, 6043 стр.),
singleton `Instanse` (`:18`). Ссылки ниже — из этого файла, если не указано иное.

### 2.1 Вход в рейд

1. `EstateSceneManager.FinalEmbarkClick` (`EstateSceneManager.cs:632-643`) → `RaidManager.
   DeployFromPreparation` (`RaidManager.cs:13-18`): берётся выбранный квест, собирается `RaidParty`,
   снапшот провизии.
2. `FinalEmbarkFadeEnded` (`EstateSceneManager.cs:719-740`): `EmbarkRecord`, `CheckEmbarkBuffs`,
   загрузка сцены `Dungeon` через `LoadingScreen`.
3. `RaidSceneManager.Awake` (`:130-171`):
   - **Сейв внутри рейда**: `CurrentRaid = new RaidInfo(SaveData)` (`RaidInfo.cs:36-74`) —
     восстанавливает Quest/Dungeon/Party/позицию.
   - **Новый рейд**: `DungeonGenerator.GenerateDungeon(Quest)` (`:153`; для plot-квестов —
     `SaveLoadManager.LoadDungeonMap`); `RaidParty = RaidManager.RaidParty` (`:154`).
4. `RaidSceneManager.Start` (`:173-266`): восстановление/создание UI, `Formations.Initialize()`
   (спавн отряда), `TorchMeter.Initialize(100)` (`:261`), затем `RoomLoadingEvent(StartingRoom, Entrance)`.
   Отряд размещается `Formations.TransferToRoom` внутри `RoomLoadingEvent` (`:703`).

### 2.2 Основной цикл движения

- Движение по коридору — `RaidPartyController.Update` (`RaidPartyController.cs:26-91`).
- «Сердцебиение» сектора — `RaidHallSector.OnTriggerEnter2D` (`RaidHallSector.cs:223-349`):
  - штраф факела: −6 (скрытый) / −1 (`:233`);
  - `RaidInfo.EnteredSector` → новый сектор → `AdvanceThroughDungeon` → `ExecuteRoundAdvance` (`:1168-1171`);
  - **бой в коридоре**: сектор `Type == Battle` и не `Cleared` → (опц. подмена `SharedMash` при
    потухшем факеле / инвентаре ≥65%) → `EncounterMonsters` → `EncounterEvent` (`:271`);
  - **голод**: сектор `Hunger` при `HungerCooldown <= 0` → `ActivateHunger` (`HungerEvent`), кулдаун
    18 шагов (`:324-343`);
  - curio-триггеры от трейтов/квирков → `CurioEvent` (`:281-320`).
- Состояние позиции — `RaidInfo.CurrentLocation` (`RaidInfo.cs:17`), `LastRoom`/`LastSector`,
  `sceneState` (`DungeonSceneState.Room/Hall`).
- Переходы: комната → коридор (`HallwayLoadingEvent`, `:527-647`), дверь → комната
  (`ActivateDoor` `:1236-1244` → `RoomLoadingEvent` `:649-839`).
- Пропы: curio → `ActivateCurio` (`:1192-1216`) → `CurioEvent`; ловушка → `TrapEvent` (`:5569`);
  препятствие блокирует движение → `ObstacleEvent` (`:5744`).
- **Шаговый тик коридора** — `ExecuteRoundAdvance` (`:4656-4808`): DoT (bleed/poison) по героям,
  смерти, bark-реплики.

### 2.3 Бой

- Старт: `EncounterEvent` (`:1970-2073`) — `BattleGround.InitiateBattle`/`SpawnEncounter`, ролл
  сюрприза, цикл раундов. Продолжение сейва — `LoadEncounterEvent` (`:1932-1968`).
- Цикл боя (`BattleRound`, `:2276-2800`): до `BattleStatus.Finished`: stalling-стресс/суммоны,
  статусы, бонус-ходы, затем `HeroTurn`/`MonsterTurn` из `Round.OrderedUnits[0]`.
- `HeroTurn` (`:2802-3427`): DoT/станы на старте хода, цикл ввода игрока (`Round.OnHeroTurn`),
  `ExecuteHeroSkill`/`ExecuteHeroItemUsage` (firewood → кемпинг)/move. **Сейв-чекпоинт после каждого
  хода героя** (`:3156-3160`).
- `MonsterTurn` (`:3428-3526`): ИИ через `MonsterBrainDecision` → `ExecuteMonsterSkill` (`:4070`).
- Завершение боя — `FinishEncouter` (`:2075-2254`): лут (`LootEvent`), `BattleEncounter.Cleared = true`,
  проверка квест-целей (`Raid.CheckQuestGoals` → `CompletionCrestEvent`), scouting, `CompleteArea`,
  **сейв** (`:2251-2252`).
- Ретраит из боя — `BattleGround.RetreatFromBattle` (`BattleGround.cs:522-577`).
- Разгром — `ProcessHeroDeaths`/`ProcessRaidFailure` (`:1884-1892`) → `RaidResultsEvent`.

### 2.4 Кемпинг

Только из комнаты предметом **firewood** (`ExecuteHeroItemUsage`, `:4409-4416`) → `CampingEvent`
(`:841-1103`):
1. Переход: факел +100, `CampController.SwitchCamping(true)`, фаза `Meal`.
2. **Еда**: `LoadCampingMeal`; ранг еды 0 → `ProcessStarvation`; ранги 2–3 → хил 10%/25% + снятие стресса.
3. **Скиллы**: `CampingPhase = Skill`, герой тратит `CampingTimeLeft` (12); `CampEffect`-ы применяются
   через `ExecuteCampEffectGroup` (баффы/хилы/стресс/лут/факел/снятие DoT).
4. **Выход**: `SwitchCamping(false)`, ролл ночной засады `0.5 − NightAmbushReduced` → при успехе
   `EncounterEvent(room, campfireAmbush:true)`.

### 2.5 Завершение / ретраит / разгром

- Квест выполнен: `CompletionCrestEvent` (`:2256-2274`) → `RaidResultsEvent` или продолжение.
- `RaidResultsEvent` (`:1105-1141`): `RaidManager.Status` = `Success`/`Abandon`/`Defeat`; окно
  результатов → `ResultItemWindow.PrepareRewards`: на `Success` — `Campaign.QuestsComleted++`
  (`ResultItemWindow.cs:36`), XP, награда (золото/heirloom/тринкеты), конвертация вынесенного лута.
- Возврат в имение → сцена `EstateManagement`; `EstateSceneManager.Start` (`EstateSceneManager.cs:180-277`):
  выжившие — `TownReset`, мёртвые — в склеп, `Campaign.ExecuteProgress()` + `AdvanceNextWeek()`
  (неделя++, `WeekActivityLog`, городские события), затем сейв.

### 2.6 Сейвы внутри рейда

Точки (каждая: `SaveData.UpdateFromRaid()` + `SaveGame()`): после каждого хода героя (`:3156-3160`),
после боя (`:2251-2252`), после `HungerEvent`/`TrapEvent`/`ObstacleEvent`. Сериализуется
`SaveCampaignData.UpdateFromRaid` (`SaveCampaignData.cs:483-530`): `Quest`, `Dungeon`
(`IBinarySaveData`), `RaidParty`, кемпинг-фаза, голод-кулдаун, факел, позиция, счётчики
(`ExploredRoomCount`, `KilledMonsters`, `InvestigatedCurios`), инвентарь, при бое — `InBattle` +
`BattleGroundSaveData`.

### 2.7 Ключевые корутины (топ-левел)

`RoomLoadingEvent` (649), `HallwayLoadingEvent` (527), `EncounterEvent` (1970), `LoadEncounterEvent`
(1932), `BattleRound` (2276), `HeroTurn` (2802)/`MonsterTurn` (3428), `FinishEncouter` (2075),
`CampingEvent` (841), `CurioEvent` (5258), `TrapEvent` (5569)/`ObstacleEvent` (5744), `HungerEvent`
(4982)/`ProcessStarvation` (5935), `ExecuteRoundAdvance` (4656), `RaidResultsEvent` (1105),
`CompletionCrestEvent` (2256), `ScoutingEvent` (5091)/`ScoutingHallway` (5864).

---

## 3. Curio и пропы (взаимодействия)

Все ссылки на `RaidSceneManager.cs` (RSM), `RaidHallSector.cs` (RHS), `ScrollEventInteraction.cs` (SCE),
`ScrollEventLoot.cs` (SEL), `Mechanics\RaidSolver.cs` (RS).

### 3.1 Модель (ядро, `src\Core\...Content\Raid\`)

- `Prop` — базовый проп (StringId, Type, бинарные Write/Read); `Trap`/`Obstacle`/`Door` в Unity наследуют
  его; **`Curio` живёт в ядре и используется Unity напрямую** (`RSM.cs:5275`).
- `Curio` — `OriginalId` (tutorial-маппинг: tutorial_shovel→unlocked_strongbox и т.п.), `IsFullCurio`,
  `IsQuestCurio`, `Tags`, `Results` (List\<CurioInteraction\>), `ItemInteractions`.
- `CurioInteraction` (вес `Chance`, implements `IProportionValue`) → `Results` (List\<CurioResult\>);
  `CurioResult` — `Item`, `Draws`, `IsCombined`, `Chance`; `ItemInteraction : CurioInteraction` + `ItemId`.
- `CurioCsvParser` (`Content\Database\CurioCsvParser.cs:17-105`): блок = 15 строк на curio;
  шапка (id/ResultTypes/RegionFound/IsFullCurio/Tags), 8 interaction-строк (по 3 результата), 3
  item-interaction-строки; ячейка `"<- # Draws"` → `Draws = Chance` + `IsCombined = true`, иначе `Draws = 1`.

### 3.2 Триггеры curio

Клик в коридоре (`RHS.cs:186-199`) / в комнате / клавиша W; **force-tag от трейта/квирка**
(`CurioTag == "All"` или совпадение тега + `CheckSuccess(TagChance)`, `RHS.cs:281-319`) — квестовые
curio исключены; после зачистки боевой комнаты (`RSM.cs:2167-2192`). Guard: `CurrentEvent != null` — блок.

### 3.3 Разрешение `CurioEvent` (`RSM.cs:5258-5567`)

1. UI выбора показывается только для `IsQuestCurio` или `(IsFullCurio && нет триггеров)`
   (`:5279`); иначе — автоматический весовой выбор.
2. Выбор: `ItemInteraction` → по `SelectedItem.Id` (`:5298`); `ManualInteraction` → весовой выбор
   `curio.Results` → `curio.Results.Results` (два уровня `RandomSolver.ChooseByRandom`, `:5300-5305`).
   Нет совпадающего item-interaction → «предмет не сработал» (`:5846-5855`).
3. Диспатч по `ResultType` (`:5386-5532`): `nothing`/`teleport` (no-op), `summon` (бой, напр.
   shambler), `scouting` (`CurioScoutingEvent`), `loot` (→ `LootEvent`, учёт `KeepLoot` трейтов/квирков),
   `quirk` (+/−/имя), `effect` (применение `Effects[item]`), `purge` (снятие негативного квирка),
   `disease` (случайная/именованная). Стресс/хил приходят только как эффекты, не как ResultType.
4. После: bark-реплика, снятие блокировки, **сейв** (`:5556-5566`).
5. Лут: quest-curio → 1× `quest_item`; обычный — если `IsCombined`, лутаются все результаты
   взаимодействия, иначе `GenerateLoot` с `Draws` итераций (`SEL.cs:80-135`, `RS.cs:105-140`).

### 3.4 Trap / Obstacle / Door

- **Trap** (`TrapEvent`, `RSM.cs:5569-5742`): триггер по шагу (`RaidTrap.OnTriggerEnter2D`) или клику.
  Разминирование — только при ручной активации: `disarmChance = Trap − RoundToInt(Difficulty/2)·0.2`
  (`:5589-5598`). Не разминирована → ролл уклонения (`Clamp(Dodge, 0, 0.9)`), попадание →
  `TakeDamagePercent(|HealthPenalty|)` + fail-эффекты. Жертва — передний/выбранный герой.
- **Obstacle** (`ObstacleEvent`, `RSM.cs:5744-5827`): вкладка с **лопатой** (если есть). Лопата —
  чистая зачистка; ручное действие — урон по отряду `HealthPenalty` + `FailEffects` всем.
- **Door** — только переход между зонами (`ActivateDoor` `:1236-1244` → `RoomLoadingEvent`);
  механики «запертой двери» в рейде нет.

---

## 4. Кампания и город (имение)

### 4.1 Модель

`Campaign.cs`: `CurrentWeek`, `QuestsComleted`, `Estate`, `RealmInventory`, `Logs` (week-логи),
`Heroes` (ростер), `Quests`, `CompletedPlot`, `TriggeredEvent`/`GuaranteedEvent`, `EventModifiers`,
`Dungeons` (Dictionary\<string, DungeonProgress\>), narration-словари. Золото/реликвии — на `Estate.
Currencies` (gold/bust/deed/portrait/crest, `Estate.cs:38-43`).

### 4.2 Смена недели

`Campaign.ExecuteProgress()` (`:105-164`): применение событийных эффектов (IdleBuff/IdleResolve/
InActivityBuff) → `Estate.ExecuteProgress()` (NomadWagon/StageCoach/активности) → ролл городского
события (`EventsOption.Frequency[3]` + весовой выбор, `:142-160`) → `GenerateQuests()`. 
`Campaign.AdvanceNextWeek()` (`:166-181`): неделя++, новый `WeekActivityLog`, при успешном рейде —
`CheckGuarantees` (гарантированное событие), снятие `BuffSourceType.Estate`-баффов. Вызывается из
`EstateSceneManager.Start` после возврата из рейда (`EstateSceneManager.cs:204-216`).

### 4.3 Здания и апгрейды

- `BuildingType` (10 зданий), абстрактный `Building`; `Estate` собирает здания из `Data.Buildings`
  (`Estate.cs:48-77`). `UpgradeTree` (IsInstanced: hero vs town) → `Upgrade`:
  `TownUpgrade` (Code/Cost/Prerequisites), `HeroUpgrade` (+PrerequisiteResolveLevel), ITownUpgrade
  (`CostUpgrade`, `RecruitUpgrade`, `SlotUpgrade`, `StressUpgrade`, `DiscountUpgrade`, `ChanceUpgrade`).
  Покупки — `UpgradePurchases` (town-wide и per-hero), статусы Purchased/Available/Locked в
  `GetUpgradeStatus` (`Estate.cs:298-343`).
- **Abbey/Tavern** — `ActivityBuilding` + `TownActivity` (6 активностей): платные слоты снимают
  `StressHealAmount`, ролл `SideEffectChance` (герой пропал/заблокировано/квирк/тринкет/бафф/золото).
- **Sanitarium** — лечение квирков (`QuirkTreatmentActivity`) и болезней (`DiseaseTreatmentActivity`,
  `CureAllChance`).
- **Guild/Blacksmith/CampingTrainer** — дисконты апгрейдов; **NomadWagon** — тринкеты по `RarityTable`;
  **StageCoach** — найм (гарантия партии из 4), **Graveyard** — `DeathRecords`, **Statue** — маркер.

### 4.4 Городские события

`TownEvent` (`Chance = base + ChancePerNotRolled·notRolled`), требования в `IsPossible` (`:50-77`);
15 типов `TownEventDataType`. `EventModifiers` аккумулирует эффекты на неделю (`IncludeEvent`,
`Reset()` после недели). Потребители: слоты активностей, провизия (`ShopSlot.cs:25,37`), бесплатные
апгрейды, embark-баффы (`Campaign.CheckEmbarkBuffs`).

### 4.5 Estate-флоу

`EstateSceneManager` (`EstateSceneManager.cs`): состояние EstateScreen/QuestScreen/ProvisionScreen;
возврат из рейда — выжившие `TownReset()`, мёртвые в склеп, `ExecuteProgress`+`AdvanceNextWeek`,
затем сейв (`:268-273`). Путь к рейду: выбор квеста → `ProvisionClick` (провизия) →
`FinalEmbarkClick` → `RaidManager.DeployFromPreparation`.

---

## 5. Сейвы

### 5.1 Интерфейс и кодек

`IBinarySaveData` живёт в ядре (`src\Core\...Content\Save\IBinarySaveData.cs`): `Write/Read(BinaryWriter/
Reader)`. `BinarySaveDataHelper` (`Setup\SaveSystem\IBinarySaveData.cs:8-199`): count-prefixed helpers для
List/Dictionary; `Create<T>` — полиморфное чтение (Quest/PlotQuest, Door/Curio/Obstacle/Trap).

### 5.2 Формат и флоу

`SaveLoadManager.WriteSave` (`SaveLoadManager.cs:14-163`) → `persistentDataPath/Saves/DarkestSave{slot}.darkestsave`.
Версия `SaveVersion = "1"` (`:11`), расхождение → исключение «updater не реализован» (`:186-187`).
Блок имения (валюты, ростер, StageCoach, тринкеты, `DungeonProgress`, покупки, activity log, события,
modifiers, narration) + блок рейда (только `InRaid`: квест/данж через `IBinarySaveData`, партия, позиция,
кемпинг, факел, инвентарь, бой). `ReadSave` при ошибке **удаляет повреждённый сейв** и возвращает null
(`:337-343`). Plot-рейды: карты в `persistentDataPath/Maps/{map}.bytes` (`:369-387`).

### 5.3 Что сериализуется у героя

`SaveHeroData` (`SaveHeroData.cs:5-109`): Status, MissingDuration, InActivity, Trait, RosterId, Name,
HeroClass, ResolveLevel/XP, CurrentHp, StressLevel, Weapon/ArmorLevel, Left/RightTrinketId, Quirks,
Buffs, SelectedCombatSkillIndexes, SelectedCampingSkillIndexes.

### 5.4 Точки сейва и входа

Старт: `DarkestDungeonManager.LoadSave()` → `campaign.Load(SaveData)`. `SaveSlot`/`SaveSelector`:
пустые слоты → `WriteStartingSave` (стартовое имение, 2 героя Reynald/Dismas, 6 данжей). Внутри рейда —
чекпоинты после хода героя/боя/событий (см. §2.6).