# PLAN.md — Активный план задач

## Задача: Save (Фаза 2) — бинарный кодек + DTO + `ISaveStorage` в `Core.Save`

### Цель

Вынести сериализацию сохранений из legacy `SaveLoadManager` (Unity, 1751 стр.) в чистый
`Core.Save` (netstandard2.0): бинарный кодек + DTO + `ISaveStorage`. Поведение кампании — Фаза 4;
здесь только перенос/структурирование персистентности, не меняя семантику данных. Legacy Unity
остаётся, ядро — источник истины для формата.

### Шаги

1. [x] Изучить `SaveLoadManager` + `SaveCampaignData`/`SaveRaidData`/все `IBinarySaveData`-типы
     (структура бинарного формата, порядок записи, versioning).
2. [x] `Core.Save`: `SaveCodec` (BinaryWriter/Reader-версия формата) + `SaveVersion` + `ISaveStorage`
     (файл ↔ поток). **`SaveCampaignData` DTO — отложен**: его поля зависят от кампанийных
     runtime-моделей (Quest/Dungeon/WeekActivityLog/DeathRecord/UpgradePurchases/QuirkInfo) — вынос
     вместе с Фазой 4 (поле-маппинг и `Create<T>` остаются в Unity).
3. [x] Unity-адаптер: `BinarySaveDataHelper` делегирует коллекции/версию в `SaveCodec`;
     `SaveLoadManager` пишет/читает версию через `SaveCodec`. Минимальный diff.
4. [x] Тесты: `Core.Save.Tests` (14) — round-trip кодеков, версия, фильтр `IsMeetingSaveCriteria`,
     storage файл↔поток.
5. [x] Доки: `docs/mechanics/save/save_binary.md` (актуализирована), `CHANGELOG`, `ARCHITECTURE`,
     `00_index.md`.

### Проверка

6. [x] `dotnet build` + все 10 сьютов (172 теста, +14); `check-using-placement` OK;
     `unity-compile-check` оба дерева (unity + unity-2017) — компиляция и script-reference OK.

---

## Задача: MS-абстракция логирования + файловый логгер + паритет эффектов §2.8

### Цель

Заменить самописный логгер на MS-абстракцию `Microsoft.Extensions.Logging.Abstractions 3.1.12`
(чистый netstandard2.0, совместим с Unity 2017.4) **на границе** (ядро остаётся чистым без NuGet —
вариант A); файловый логгер (запись в текстовый файл) для WPF и Unity. Закрыть парсинг эффектов
§2.8: `.kill`/`.kill_enemy_type`/`.performer_rank_target`/`.clear_rank_target` (классы уже есть,
`MarkedForDeath` уже потребляется `DeathCheck`). `.summon`/`.control`/`.capture` — **пропускаем**
(кампанийные монстры, в дуэли не встречаются).

### Фаза LG1 — MS-абстракция на границе

1. [x] `Microsoft.Extensions.Logging.Abstractions` 3.1.12 → WPF (net8.0) + оба Unity-дерева.
2. [x] `MsLoggerAdapter : Core.Common.ILogger` — оборачивает MS `ILogger` (Log/Warn → LogInformation/
     LogWarning). В WPF + unity + unity-2017.
3. [x] `FileLogger` + `FileLoggerProvider` — запись `[timestamp] [level] message` в текстовый файл.
4. [x] WPF: `DuelLobbyViewModel`/`SinglePlayerLobbyViewModel` передают логгер в `DuelController`;
     Unity — аналогично (по возможности).

### Фаза LG2 — Паритет эффектов §2.8

5. [x] `EffectParser`: `.kill` → `KillEffect`, `.kill_enemy_types <type>` → `KillEnemyTypeEffect`,
     `.performer_rank_target <rank>` → `PerformerRankTargetEffect`,
     `.clear_rank_target` → `ClearRankTargetEffect`; константы в `EffectIds`/enum.
6. [x] `.disease` → `DiseaseEffect(null, true)` (только `any`; конкретные id — кампанийный остаток).

### Фаза LG3 — Тесты

7. [x] Parser-тесты новых эффектов (+2); файловый логгер (+3: запись/формат/категория, Warn,
     мин-уровень); DuelController с логгером; все 158 тестов зелёные.

### Фаза LG4 — Доки

8. [x] `BATTLE_PARITY` §2.8 (`.kill`/`.kill_enemy_types`/rank-target/disease — парсятся; `:114`
     устарела — обновлена), §5 (пункты 9-11); `06_mark.md` + `14_death_stress.md`; `ARCHITECTURE`
     (логгер на границе); `CHANGELOG`; `CLEANUP` C6.

### Проверка

9. [x] `dotnet build` + 9 сьютов (158 тестов, +5 новых) зелёные; lockstep;
     `check-using-placement` OK. Unity-деревья не затронуты (только `src/`/`tests/`/`docs/`) —
     `unity-compile-check` не требуется.

---

## Задача: Quest-цели в ядро + документирование `ForceHallConnection` (QuestGenerator — отдельно)

### Цель

Перенести `PopulateQuestGoals` (kill_monster/activate/gather) из Unity-адаптера в ядро
(`Core.Raid\Generation`) как `DungeonGenerator.ApplyQuestGoal`. Unity `Quest`/`QuestGoal`/`IQuestData`
в ядро не тащим — примитивная модель `DungeonQuestGoal`. `ForceHallConnection` — **оставляем как в
Unity** (паритет; `BaseCorridorNumber` не влияет — недоделка оригинала, документируем).
`QuestGenerator` — отдельная задача после (зависит от runtime-моделей Campaign, Фаза 4).

### Фаза QG1 — Модель цели

1. [x] `Core.Raid\Generation\DungeonQuestGoal.cs` — `Type`, `MonsterNameIds`, `CurioName`,
     `Amount`, `ItemId`, `ItemAmount`.

### Фаза QG2 — Ядро `ApplyQuestGoal`

2. [x] `DungeonGenerator.ApplyQuestGoal(Dungeon, DungeonQuestGoal, envData, difficulty, rng)`:
     kill → boss-комната + `BattleEncounter`; activate/gather → curio-комнаты по `MinPath`-сегментам
     с `CurioInteraction`/`ItemInteraction`/`CurioResult`; `UnityEngine.Random` → `IRng`.

### Фаза QG3 — Unity-адаптер

3. [x] `unity\...\Generation\DungeonGenerator.cs` (оба дерева): конвертер `quest.Goal` →
     `DungeonQuestGoal`, вызов ядра; удалить свою `PopulateQuestGoals`.

### Фаза QG4 — Тесты

4. [x] kill → boss+encounter; activate → `Amount` curio-комнат; gather → curio с Item/CurioResult;
      детерминизм по сиду (новые тесты в `DungeonGeneratorTests`, 14/14).

### Фаза QG5 — Доки

5. [x] `dungeon_generation.md` — quest-цели в ядре, `ForceHallConnection`-пустышка (каркас работает,
      `while count != hallNumber` мёртвый с TODO+break, `BaseCorridorNumber` не влияет — недоделка
      оригинала, оставлена для паритета); `EXTRACTION_STATUS`, `CHANGELOG`, `PLAN`, `00_index`;
      `QuestGenerator` — эскиз отдельной задачи в `PLAN.md`.

### QuestGenerator — эскиз отдельной задачи (после Campaign-моделей, Фаза 4)

- Модели runtime: `Quest`/`QuestGoal`/`QuestData` (kill/activate/gather/visit/trait/deaths_door),
  `Campaign` (QuestsComleted, Dungeons MasteryLevel, Heroes Resolve, EventModifiers, CompletedPlot),
  `CompletionReward`/`ItemDefinition`.
- Data: `QuestDatabase` (QuestGeneration, QuestTypes/GoalLists, QuestGoals, PlotQuests);
  `JsonQuests`/`JsonQuestGoal` уже в `Core.Campaign\Database`.
- Чистый `IRng` вместо `Random`/`RandomSolver`-глобала; `ChooseByRandom` по `IProportionValue`.
- Unity-адаптер + тесты (детерминизм, распределение по сложностям/типам/целям/наградам).
- Статус: `EXTRACTION_STATUS`, `UNITY_LEGACY_MAP` §9, `dungeon_generation.md`.

### Проверка

6. [x] `dotnet build` + все 9 сьютов; lockstep; `unity-compile-check` оба дерева;
      `check-using-placement`.

---

## Задача: DungeonGenerator → ядро (`Core.Raid\Generation`) — вариант A

### Цель

Перенести процедурную генерацию подземелья из `unity\Assets\Scripts\Generation\DungeonGenerator.cs`
(712 стр.) в чистый детерминированный модуль `Core.Raid\Generation`:
`(genData, envData, каталоги, seed) → Dungeon-результат`. Всё Unity (Random/Mathf/Assertions,
глобальный `DarkestDungeonManager.Data`, живые модели) — за пределами ядра. Unity-адаптер — тонкая
обёртка, старый генератор остаётся до ручной проверки (минимальный дифф). **Quest-цели
(`PopulateQuestGoals`) срезаются** из ядра (топология + население + enviroment в ядре; quest-цели —
отдельно позже, в Unity-адаптере или отдельным шагом).

### Фаза DG1 — Модели данных генерации (чистые, `Core.Raid\Generation`)

1. [x] `DungeonGenerationData` — поля из `MapGenerator.txt` (length/quest_type/dungeon_type,
     base_room/corridor, gridsize, spacing, connectivity, min_final_distance, все min/max).
2. [x] `DungeonEnviromentData` + `DungeonBattleMash` + `DungeonPropsEncounter` +
     `DungeonBattleEncounter` — из `Dungeons/*.bytes` (hall_variants, room_variants, mash-секции,
     prop-encounters, named-encounters).
3. [x] Парсеры DSL (`MapGenerator` / `Enviroment`) → `Clients.Content\GameDataReader`
     (`ReadDungeonGenerationData`, `ReadDungeonEnviromentData`).

### Фаза DG2 — Модели результата (вариант A, чистые)

4. [x] `Dungeon`, `DungeonRoom`, `Hallway`, `HallSector`, `Area` (база), `Door` + enums `Direction`,
     `Knowledge` — в `Core.Raid\Generation` (без `IBinarySaveData`, без Unity; `Prop`/`Curio`/
     `BattleEncounter` — из существующего `Core.Raid`).

### Фаза DG3 — Чистая топология

5. [x] Внутренние `GenRoom`/`GenHall` (без `Assert` — честные ветки/fallback).
6. [x] `GenerateRooms`, `FindMaxConnectivityRoom`, `FindLongestPathRoom`, `FindBorderingRooms`,
     `ForceBorderRooms`, `ForceHallConnection`, `CalculateMinPath`, `MarkEntrance` — чистая
     топология на `AreaType` + позиции (как в Unity, с тем же порядком `RandomSolver` вызовов).

### Фаза DG4 — Население + enviroment

7. [x] `PopulateRooms`/`PopulateHalls` — чистые (количества боев/ловушек/curio/голода из genData,
     `AreaType`), `Mathf.Clamp` → локальный.
8. [x] `LoadRoomEnviroment`/`LoadHallEnviroment` — `RandomSolver.ChooseByRandom` + каталоги
     (Curios/Obstacles/Traps/encounters) аргументом, `BattleEncounter` — из ядра.
9. [x] `DungeonGenerator.Generate(genData, envData, questParams, catalogs, seed) → Dungeon`
     (оркестратор; **без** `PopulateQuestGoals`).

### Фаза DG5 — Unity-адаптер-обёртка

10. [x] `unity\Assets\Scripts\Generation\DungeonGenerator.cs` → тонкая обёртка: читает
      `DarkestDungeonManager.Data` → зовёт ядро → мапит результат в свои `Dungeon`/`Room`/`Hallway`.
      Старый код — рядом (вынесен в `DungeonGeneratorLegacy.cs`), удаляется после ручной проверки.

### Фаза DG6 — Тесты

11. [x] Детерминизм по сиду; структура (кол-во комнат/коридоров/сетка/вход/связность);
      население (типы в пределах min/max); реальные `MapGenerator.txt` + `Crypts.bytes`;
      порядок `RandomSolver`-вызовов (важно для сида).

### Фаза DG7 — Доки

12. [x] `docs/mechanics/raid/dungeon_generation.md` — полная спецификация (модели, парсинг,
      порядок срабатывания с file:line, роли, население, детерминизм, **нюансы**: порядок RNG,
      ForceHallConnection-пустышка, MarkEntrance-рандом, `1+(x-1)*7`); `00_index.md`,
      `EXTRACTION_STATUS.md`, `CHANGELOG.md`, `PLAN.md`.

---

## Задача: Death's door + heart attack в ядре (закрытие паритет-разрыва `BATTLE_PARITY.md` §2.2)

### Цель

В ядре герой умирает сразу при 0 HP, а `AtDeathsDoor`/`AtDeathRecovery` никогда не устанавливаются;
heart attack — только событие в лог. Закрыть по Unity-эталону (`Hero.ApplyDeathDoor/RevertDeathsDoor/
ApplyMortality/RevertMortality`, `PrepareDeath`, heart-attack-очередь): вход в death's door, ролл
`DeathBlow`-резиста при повторном ударе, survival-бафф, снятие хилом, heart attack при стрессе 200.
Death's door — только для героев (обеих сторон дуэли); монстры умирают сразу.

### Фаза 1 — Модель: `DeathDoor`-данные + методы `Hero`

1. [x] `Character/DeathDoor.cs` — модель (`Buffs`, `RecoveryBuffs`, `HeartAttackBuffs`).
2. [x] `HeroClass.DeathDoor` + парсинг `deaths_door:` секции в `HeroClassFileParser`
     (`.buffs`, `.recovery_buffs`, `.recovery_heart_attack_buffs`).
3. [x] `Hero`: `DeathResist` (DeathBlow, клэмп 0..0.87, дефолт 0.5); `ApplyDeathDoor`/
     `RevertDeathsDoor`/`ApplyMortality`/`RevertMortality`; `SupportsDeathDoor` (Hero=true,
     Monster/Character=false); `Monster.CanDieFromDamage` + парсинг `death_class:`; ресолв баффов
     case-insensitive (`BuffCatalog`/`TestDuelContent`).

### Фаза 2 — `DeathCheck`: смерть vs death's door

4. [x] Монстры: `HealthRatio <= 0` → смерть (с учётом `CanDieFromDamage=false` — не умирает).
5. [x] Герои при `HealthRatio <= 0 && !IsDead`: если не `AtDeathsDoor` и не `MarkedForDeath` →
     вход в death's door (флаг + death-door-баффы + survival-бафф + `BarkStress` 6 + попап
     `DeathsDoor`), НЕ умирает; если уже на death's door/`MarkedForDeath` → ролл
     `DeathResist - resistIgnoreBonus(0.3 при численном перевесе)` + survival-бафф (если не
     `MarkedForDeath`) или смерть + `StressParty`.
6. [x] Константы: `DeathsDoorSurvivalDebuff`, `MaxDeathResist 0.87`, `ResistOverrideBonus 0.3`,
     `DeathsDoorSurvivalDuration 3`, `EffectIds.BarkStress`.

### Фаза 3 — Heart attack (стресс = 200)

7. [x] `DuelBattleEvents.HeartAttackHandler` (колбэк, как `TorchDelta`); регистрация в
     `DuelController.InitializeMechanics`.
8. [x] `HeartAttackHandler` (Core.Duel/Mechanics): на death's door → `MarkedForDeath` + смерть +
     `StressParty`; иначе → `TakeDamagePercent(1.0)` + стресс 75% + вход в death's door.

### Фаза 4 — Снятие при хиле

9. [x] `DuelController.RecoverDeathsDoorIfHealed` после скилла: герой `AtDeathsDoor` и HP > 0 →
     `Hero.RevertDeathsDoor(recovery-баффы)` + mortality.

### Фаза 5 — Тесты

10. [x] `DeathsDoorTests` (6): вход в death's door (не умирает, флаг); ролл на death's door
      (выжил/умер + стресс отряду); хил снимает death's door; heart attack (оба пути);
      парсер `deaths_door:` (2 в `HeroClassFileParserTests`); `MonsterClass.CanDieFromDamage`
      default true (баг: bool default false → монстры не умирали, `FightSessionTests` упал).

### Фаза 6 — Доки (тот же коммит)

11. [x] `BATTLE_PARITY.md` §2.2/§5; `docs/mechanics/combat/14_death_stress.md`; `EXTRACTION_STATUS.md`;
      `TESTING.md`; `CHANGELOG.md`; `PLAN.md` шаги `[x]`.

### Проверка

12. [x] `dotnet build` + 9 сьютов зелёные (Duel 32, Combat 55, Clients 7); lockstep;
      `check-using-placement`; unity-compile-check оба дерева — в финальном коммите.

---

## Задача: уборка кода ядра (`Core.*`) — магия, ветвления, приватные методы, логи

### Цель

Ядро выросло из Unity-легаси и несёт legacy-стиль: магические строки (id эффектов/баффов/теги
логов), магические числа (клэмпы 0.95/0.65/0.1, шансы), большие switch/if (диспетчеры правил,
строковые парсеры), god-классы с приватными методами (в т.ч. `DuelController`, ~720 строк),
отсутствие логирования. Копирование из Unity не оправдывает плохой код. Уборка: именованные
константы, полиморфизм вместо ветвлений (AGENTS.md §II), вынос приватной логики в тестируемые
классы, структурное логирование, минимум изменений поведения (тесты — охранник). Legacy Unity и
`src/External` не трогаем.

### Фаза C0 — Аудит и карта уборки

1. [x] `docs/CLEANUP.md` — каталог проблем по модулям (магия, god-классы, ветвления) с file:line и
     приоритетами; правило «ядро чистится, legacy/External — нет» зафиксировать в AGENTS.md.

### Фаза C1 — Константы и идентификаторы

2. [x] Каталог контент-идентификаторов: `Core.Combat/Content/EffectIds.cs`, `BuffIds.cs`,
     `BattleConstants.cs` (named-константы). Заменены магические строки в `DuelController`,
     `DuelBattleContext`, `BattleSolver`, эффектах; магические числа (клэмпы 0.95, длительности 3/1,
     крит ×1.5, сюрприз 0.1/0.65, минимальная меткость 0.1).
3. [x] `ChanceMath.Clamp01` — единая утилита вместо 9 дублей private `Clamp01(..., 0.95f)`.

### Фаза C2 — Полиморфизм вместо ветвлений

4. [x] `Character.ApplyBuffRule` (switch по `BuffRule`, ~90 строк) → `BuffRuleEvaluator`
     (реестр функций по правилу, OCP); `StringToStatusType`/`StringToMonsterType` перенесены.
5. [x] `EffectCatalog.ParseEffect` (god-метод ~140 строк) → `EffectParser` (отдельный тестируемый
     класс); `MapTarget` → реестр-словарь.
6. [ ] AI-desires (`SkillSelectionDesire`/`TargetSelection*`) — switch по строкам-ключам → реестр
     (отложено, P2 в CLEANUP §3b).

### Фаза C3 — Вынос приватной логики в классы

7. [x] `DuelController` декомпозиция: `SurpriseResolver`, `DotTickApplier`, `StunRecoveryApplier`,
     `DeathCheck`, `TurnMover` (все `Core.Duel/Mechanics`, конструкторы-DI).
8. [x] `BattleSolver` — `DamageResolver`/`HealResolver` (чистые расчёты, без BattleContext);
     `ExecuteSkill` — оркестратор (guard-редирект, эффекты, стресс).

### Фаза C4 — Логирование

9. [x] `Core.Common/ILogger` + `NullLogger` (no-op для тестов); DI через конструктор
     `DuelController(content, logger = null)`. Логирование: ходы (стан/пропуск), скиллы
     (`[duel] <name> used <skill> -> <target> (<type> <amount>)`).
10. [ ] `DuelBattleEvents.Log` → структурированные записи (отложено, P2 — затронет WPF-UI).

### Фаза C5 — Проверка и доки

11. [x] Все 9 сьютов зелёные (135); `dotnet build` без новых ошибок; lockstep-тесты проходят.
12. [x] `docs/mechanics/*` синхронизированы с новыми file:line/классами; `TESTING.md` —
      паритет-механики; `CHANGELOG.md` — версия; `PLAN.md` — шаги `[x]`.

---

## Задача: спецификации всех механик по доменам — `docs/mechanics/` — ветка `docs/mechanics`

### Цель

Детальные документы по каждой механике (не только боевые DoT/эффекты, а вообще все) с условиями,
проверками, порядком срабатывания и очередями — чтобы агенты не переоткрывали поведение методом
проб (как было с станом/`UpdateRound`/`IsApplied`-гейтом в паритете). Папки по доменам (зеркало
`TARGET_LAYOUT.md`): `combat/`, `duel/`, `content/`, `campaign/`, `raid/`, `save/`, `common/`,
`clients/`, `networking/`, `presentation/`. Правило «механика = файл в `docs/mechanics/<domain>/`,
документировать в том же коммите» фиксируется в `AGENTS.md`.

### Единый шаблон документа

1. Назначение и когда работает (условие, область, статус: реализовано/частично/данные/стаб).
2. Модель данных (классы/статусы/атрибуты, `file:line`).
3. Парсинг контента (ключи `Effects.txt`/`JsonBuffs`/`.bytes` → куда).
4. Порядок срабатывания (трассировка шагов 1..N с `file:line`).
5. Очередь и обновления (instant vs queued, per-turn/per-round, истечения).
6. Проверки и клэмпы (таблица: условие → где → границы).
7. **Нюансы и подводные камни** (очереди, гейты, нетривиальные условия — критично).
8. Взаимодействия (RemoveConditions, смерть, immobilize, guard-редирект).
9. Файлы-источники.

### Шаги

1. [x] `docs/mechanics/00_index.md` (навигатор: механика → файл → домен → статус) + шаблон;
     правило в `AGENTS.md` (обязательное расписывание механик, разделы «Порядок срабатывания»
     и «Нюансы/подводные камни»); правки `INDEX.md`, `GAME_RULES.md` (ссылки на `mechanics/`),
     `PLAN.md`.
2. [x] `combat/` — 14 боевых механик: `01_damage.md` (урон/хил/крит/меткость), `02_dot.md`,
     `03_stun.md`, `04_riposte.md`, `05_guard.md`, `06_mark.md`, `07_rank_move.md`
     (pull/push/shuffle), `08_immobilize.md`, `09_buffs.md` (+RemoveConditions), `10_torch.md`,
     `11_modes.md`, `12_surprise.md`, `13_turn_order.md` (инициатива/очередь/per-turn), `14_death_stress.md`.
3. [x] `duel/` — lockstep, `DuelSeed`/`DuelPayload`, `DuelPhase`-машина, `DuelAi`, `FightSession`,
     `TextFightContent`.
4. [x] `content/` (квирки, бафф-контент, trinket/camping), `common/` (Result, RandomSolver/IRng,
     токен-парсер, feature-flag), `clients/` (`GameDataReader`), `save/` (`IBinarySaveData`),
     `networking/` (Contracts/Steam/Photon), `presentation/` (WPF-экраны, Unity-оверлеи).
5. [x] `campaign/`, `raid/` — честно по текущему состоянию (модели/DTO/парсеры/каталоги,
     статус «поведение — Фаза 4»).
6. [x] Проверка: документы читаемы, `file:line` существуют (grep по ключевым путям), статусы
     согласованы с `BATTLE_PARITY.md`/`EXTRACTION_STATUS.md`; `check-using-placement` не требуется
     (только `.md`); `PLAN.md` — шаги `[x]`.

---

## Задача: Механики-паритет с Unity (закрытие разрывов `BATTLE_PARITY.md` §5) — ветка `core-parity`

### Цель

Закрыть в ядре (`Core.Combat` + `Core.Duel`) разрывы боевых механик, которые в Unity-мультиплеере
работают, а в дуэли/ядре отсутствуют или неполны. Legacy Unity **не правится**. Каждый пункт —
задача по `BATTLE_PARITY.md` §5: DoT-тик, stun, riposte, guard, pull/push/shuffle, immobilize,
RemoveConditions; death's door / heart attack — отдельно (кампанийные, больше объём).

### Фаза A — Статусы по ходам + DoT-тик + стан

1. [x] Статусы обновляются per-turn (Unity-паритет): `Character.UpdateRound()` вызывается в начале
     хода юнита (`DuelController.BeginTurn`), а не раз в раунд (`NextRound`).
2. [x] DoT-тик урона: в начале хода цели применять `CurrentTickDamage` (bleed + poison) к HP;
     смерть от тика обрабатывается `CheckDeaths`.
3. [x] Stun: в `BeginTurn` проверять `StatusType.Stun` → снять `StunApplied`, применить
     `STUNRECOVERYBUFF` (через `IDuelContent.GetBuff`), пропустить ход (`CompleteTurn`).

### Фаза B — Guard

4. [x] `EffectCatalog.ParseEffect`: ключи `.guard` → `GuardEffect`, `.swap_source_and_target` →
     `GuardEffect.SwapTargets`, `.clearguarding`/`.clearguarded` → `ClearGuardEffect`.
5. [x] Редирект атак: в `DuelController.ExecuteSkill`/`BattleSolver` при `Guarded.IsApplied` бить по
     `Guarded.Guard`, а не по цели.

### Фаза C — Riposte-контратака

6. [x] После попадания по цели с рипост-статусом исполнять `target.RiposteSkill` против атакующего
     (в `DuelController.ExecuteSkill`, как `RaidSceneManager.ExecuteRiposteSkillActivation`).
     Парсинг `riposte_skill` → `HeroClass.RiposteSkill` (`HeroClassFileParser`).

### Фаза D — Pull / Push / Shuffle (реальные ранги)

7. [x] `DuelBattleEvents.Pull/Push` двигают юнита в его партии (с учётом `IsImmobilized` и границ),
     пересчитывают `Rank`; `ShuffleTargetEffect`/self-move скилла получают реальное перемещение.

### Фаза E — Immobilize

8. [x] `DuelController.TryMove` возвращает false при `IsImmobilized`; `.unimmobilize`/`.unstun`/
     `.untag` парсятся `EffectCatalog` (эффекты уже есть в ядре).

### Фаза F — RemoveConditions

9. [x] В конце `DuelController.ExecuteSkill` (после `ProcessEventQueues`/`CheckDeaths`) вызывать
     `Solver.RemoveConditions` для перформера и целей.
9a. [x] Buff-идемпотентность: `Character.ApplyBuff`/`RevertBuff` получили `IsApplied`-гейт (как в
     Unity), чтобы повторное применение правил не накладывало бафф дважды.

### Фаза G — Тесты и проверка

10. [x] Тесты: DoT-тик урона по ходам, stun-пропуск + recovery-бафф, riposte-контратака,
      guard-редирект атаки, pull/push меняют ранг, immobilize блокирует move, RemoveConditions
      снимает условия после скилла. Все 9 сьютов зелёные (Duel 26, Combat 53) + lockstep (WPF 17).
11. [x] `dotnet build` + тесты; `unity-compile-check.ps1` для обоих деревьев (код ядра доставляется
      в оба `Plugins\Internal`).

### Фаза D2 — Документация (в том же коммите)

12. [x] `BATTLE_PARITY.md` — разрывы закрыты; `TESTING.md` — что проверить; `CHANGELOG.md` — версия;
      `PLAN.md` — шаги `[x]`; death's door/heart attack остаётся в roadmap как отдельная задача.

---

## Задача: Тест-бой (FightTester) на ядре + библиотека данных `Core.Data` + монстры — ветка `core-data`

### Цель

Тест-бой — стенд проверки вынесенного в ядро кампанийного контента: герои и монстры из кампании,
ИИ с кампанийным поведением (`JsonAI.json` brains), бой целиком на ядре. Для этого:

1. Новая библиотека **`src\Core\Sektor.DarkestDungeon.Core.Data`** — единый читатель всего набора JSON
   из `Data\` (+ `Mechanics\*.json`): один источник данных, поведение идентично во всех клиентах
   (Unity/unity-2017/WPF). **Старый Unity-код не трогаем** (`DarkestDatabase`/`DarkestJsonReader` живут
   как есть до cutover); существующие core-мапперы (`QuirkMapper`/`BuffContentMapper`/`TraitMapper`/
   `EffectCatalog`/`HeroCatalog`) переиспользуем на месте.
2. Вынос **монстров** (`Data\Monsters\*.txt`) и **мозгов** (`JsonAI.json`) в ядро. Сейчас в core нет
   `Monster`/`MonsterClass`/`MonsterCatalog`, а дуэль использует дефолтный `BuildDefaultBrain`,
   не кампанийные мозги; desires (Skill/Target/Bonus) уже портированы в `Core.Combat\Mechanics\AI`.
3. Раннер **Тест-боя** в `Core.Duel\Fight\` (без enum/исключений, полиморфно): 2 стороны × 4 слота,
   герои+монстры, пустые слоты разрешены.
4. Тонкие **Unity-клиенты**: сначала unity-2017, затем active `unity\`; оверлей + вход из `TestActions`,
   стрелки выбора `[пусто → герой → монстр…]` по имени, seed, «Игрок/ИИ» и «ИИ vs ИИ», кнопка «Бой».

### Фаза M0 — Статус-манифест выноса

0. [x] `docs\EXTRACTION_STATUS.md` — таблица «Unity → twin в ядре → статус» (вынесено / частично /
    не вынесено); `tools\check-extraction.ps1` сверяет пути манифеста с файловой системой и печатает
    отчёт (31 строка, все пути существуют). Поддерживается в том же коммите, что и вынос (агентам —
    один grep-таргет вместо сканирования Unity-дерева). `[Obsolete]` на legacy-коде НЕ ставим
    (код живой до cutover; при cutover — `[Obsolete(error: true)]` на удаляемых типах).
0a. [x] Ветка `core-data` создана от обновлённого `main` (wpf слит, fc9a4f5).

### Фаза DB — Библиотека данных `src\Core\Sektor.DarkestDungeon.Core.Data`

По смыслу: «данные игры». netstandard2.0, рефы Core.Content + Core.Combat; post-build — копия dll+pdb
в оба `Plugins\Internal` (как у Combat). Newtonsoft добавить пакетом (netstandard2.0-совместимый).

1. [x] csproj `Sektor.DarkestDungeon.Core.Data` + `Newtonsoft.Json`; папки `Dto\`, `Readers\`, `Catalogs\`.
2. [x] DTO + читатели на весь набор JSON: `Data\*.json` + `Mechanics\*.json` (JsonAI, Buffs, Quirks,
    Traits, Camping, Loot, Quests, Trinkets, Narration, PartyNames, Campaign, Provision, Roster,
    TownEvents, HeirloomExchange, апгрейды/здания/curios). Эталон — существующие core-DTO
    (`JsonBuffData`/`JsonQuirkData`/`JsonTraitData`/...), недостающие добавляем (в т.ч. `JsonMonsterBrains`).
3. [x] `GameDataReader` — фасад десериализации (Newtonsoft внутри): файл = метод `ReadX(text)`.
4. [x] Загрузчики каталогов из текстов, где модель в core есть: `QuirkCatalog`/`BuffCatalog`
    (портированы из WPF), `TrinketCatalog`/`CampingSkillCatalog` (новые модели `Content\Trinket`/
    `Content\Camping`), `TraitCatalog` (traits через `ReadTraits`), `MonsterBrainCatalog`,
    `MonsterCatalog`/`HeroCatalog`/`EffectCatalog` (существующие).
5. [x] WPF пересадить на `GameDataReader` (убрать инлайн `JsonConvert` в `DuelContent`/`QuirkCatalog`/
    `BuffCatalog`); поведение идентичное, тесты зелёные (13/13).

### Фаза M1 — Модель и парсер монстров (в `Core.Combat`, зеркало `Assets\Scripts\`)

6. [x] `Character\MonsterClass.cs` — контент-модель: StringId, TypeId, Size, Attributes,
    EnemyTypes (MonsterType), CombatSkills (+резолв `.effect` через `EffectCatalog`), PreferableSkill,
    MonsterBrainId, BattleModifier (флаги сюрприза), InitiativeTurns (`number_of_turns_per_round`).
    Loot/DeathClass/Companions/etc. — позже (не нужны Тест-бою).
7. [x] `Character\MonsterClassFileParser.cs` — парсер DSL `Data\Monsters\*.txt`: `name`/`type`,
    `display: .size`, `enemy_type: .id`, `stats:` (.hp/.def/.prot/.spd/.stun|poison|bleed|debuff|move_resist),
    `skill:` (все `.effect`, `.move`, `.launch`/`.target`, `.is_crit_valid`, кулдауны), `personality:
    .prefskill`, `initiative: .number_of_turns_per_round`, `monster_brain: .id`,
    `battle_modifier:` (флаги сюрприза), `death_class:` (лёгкий вариант).
8. [x] `Character\MonsterCatalog.cs` — `Load(contents, effects)`.
9. [x] `Character\Monster.cs` — конкретный персонаж (`ICharacter`): IsMonster=true, MonsterTypes,
    Size, CombatSkills/CurrentCombatSkills, BattleModifiers, PreferableSkill; атрибуты/резисты из
    MonsterClass; без стресса (как в DD). `Character\BattleModifier.cs` — реализация `IBattleModifier`.

### Фаза M2 — Мозги кампании: `JsonAI.json` → core (`Core.Data`)

10. [x] DTO `JsonMonsterBrains`(+`Database`)/`JsonSkillCooldown`/desire-DTO в `Core.Data\Dto\`.
11. [x] `JsonBrainParser` в `Core.Data\Readers\`: `JsonAI.json` → `List<MonsterBrain>`; mapping
    type-строк → конструкторы desires через реестр фабрик (без switch); данные желаний —
    `Dictionary<string, object>` из `data` (как сейчас в конструкторах желаний).
12. [x] `MonsterBrainCatalog` в `Core.Data\Catalogs\` — `Load(text)`, `Get(id)`; тест `TestDuelContent`
    подменяет дефолтный brain кампанийным. `IDuelContent.GetMonsterBrain(string)`.

### Фаза M3 — Дуль-интеграция + раннер Тест-боя

13. [x] `DuelController`: аддитивный overload под юнит-спецификацию (`classId`+`seed`): герой → `new Hero`,
    монстр → `new Monster(class)` + мозг кампании; AI: герой → `DuelAi`, монстр →
    `BattleSolver.UseMonsterBrain` (мозг кампании вместо дефолтного). `StartFight`
    (+`IDuelContent.GetMonsterClass/GetMonsterBrain`)
14. [x] Сюрприз-ролл гейтится на `BattleModifiers.CanSurprise/CanBeSurprised/AlwaysSurprise/
    AlwaysBeSurprised` (сейчас хардкод без гейта).
15. [x] `Round`: поддержка `number_of_turns_per_round > 1` (монстр ходит N раз за раунд).
16. [x] `Core.Duel\Fight\`: `FightUnitSpec`/`HeroFightUnitSpec`/`MonsterFightUnitSpec` (полиморфно),
    `FightSession` (`Tick`/`RunToCompletion`, герои → `DuelAi`, монстры → `UseMonsterBrain`),
    `TextFightContent` (строки файлов → каталоги через Core.Data, реализует `IDuelContent`).
    `StressParty` — только героям. Стороны — просто списки (`PlayerFightSide`/`AiFightSide`
    опущены как лишняя обёртка).

### Фаза FC — Unity-клиенты (сначала unity-2017, затем active `unity\`)

17. [x] unity-2017: `FightContentLoader` (`Resources` → Core.Data), `FightScreen` — оверлей поверх всего,
    вход из `TestActions`: 2 стороны × 4 слота, стрелки `[пусто → герой → монстр…]` по имени, seed,
    режим «Игрок/ИИ» и «ИИ vs ИИ», кнопка «Бой»; `FightBattleView` (карты/скиллы/цели/лог).
18. [x] active `unity\`: то же.
19. [x] Проверки: `dotnet build` core + тесты; `unity-compile-check.ps1` для обоих деревьев;
    `unity-check-script-references.ps1`. `.meta` для новых Unity-файлов — коммитить вместе с `.cs`.

### Фаза M4 — Тесты и проверка

20. [x] `GameDataReaderTests` (DTO всех читаемых JSON), `JsonBrainParserTests`, `MonsterClassFileParserTests`
    (реальные монстры: статы, enemy_type, резолв эффектов, battle_modifier); дуэль/бой «герои vs монстры»
    (атаки/хилы, AI-выбор скилла, сюрприз-гейт, multi-turn, пустые слоты, детерминизм по сиду).
21. [x] Все сьюты зелёные + navigation (WPF не ломается).

### Фаза D — Документация (в том же коммите)

22. [x] `docs\TESTING.md` — ручная проверка Тест-боя в обоих клиентах; `docs\CHANGELOG.md` — версия;
    `docs\EXTRACTION_STATUS.md` — M1/M2 (монстры, мозги) → вынесено, новый модуль `Core.Data`;
    `docs\ARCHITECTURE.md` — модуль данных и раннер.

### Дорожная карта «всё в ядро» (после Тест-боя)

Полный по-классный инвентарь легаси — `docs\UNITY_LEGACY_MAP.md`; манифест — `docs\EXTRACTION_STATUS.md`;
декомпозиция ядра по доменам — `docs\TARGET_LAYOUT.md`.

- [x] **Механики-паритет** (приоритет из `BATTLE_PARITY.md` §5; закрываются в ядре): DoT-тик урона,
      stun-пропуск хода/истечение, riposte-контратака, guard (`EffectCatalog` + редирект атак),
      pull/push/shuffle-ранги, immobilize-Move, `RemoveConditions` в `ExecuteSkill` (см. задачу
      «Механики-паритет» вверху). Остаётся: death's door / heart attack (кампанийные, отдельно).
- [x] **Save** (Фаза 2): бинарный кодек + версии + `ISaveStorage` (в `Core.Save` уже есть
      `IBinarySaveData`); `BinarySaveDataHelper`/`SaveLoadManager` делегируют коллекции/версию коду
      ядра. Остаётся: DTO-перенос `SaveCampaignData` — с Фазой 4 (зависит от кампанийных моделей).
- [ ] **Campaign** (Фаза 4): поведение имения/зданий/апгрейдов/квестов/города в `Core.Campaign`
      (модели + DTO уже вынесены).
- [ ] **Encounters/Bosses/Curios/Loot**: энкаунтеры/боссы → `Core.Combat`, контент-модели → `Core.Raid`.
- [ ] **Generation**: `DungeonGenerator` → `Core.Raid\Generation` (сделано, quest-цели в Unity-адаптере);
      `QuestGenerator` → `Core.Campaign\Generation` (чистые, детерминированные, RNG на границе).
- [ ] **Networking** (Фаза 5): Steam + Photon (`Sektor.Networking`, `PhotonTransport`, SessionManager).
- [ ] **Presentation cutover** (Фаза 6): view-слой остаётся Unity/WPF; `RaidSceneMultiplayerManager`/
      `MultiplayerSync` → тонкие адаптеры поверх `Core.Duel`.
- [ ] **Чистка ядра** (из `ARCHITECTURE_REVIEW.md`): `Core.Ui` → клиентская граница; единый
      токен-парсер и `Buff`-фабрика; `DuelAi` на кампанийном брейне; тринкеты в WPF-дуэли (P1.4);
      локализация (P2.7).
- [x] **Структура ядра**: декомпозиция по доменам исполнена — `Core.Common/Save/Campaign/Raid` +
      `Clients.Content`, `Core.Data` распущен, `TextFightContent` → `Core.Duel\Fight`,
      `Result` → `Core.Common`.
- [ ] **Проверка `unity-2017\`** в реальном редакторе 2017.4 (человек; в окружении — только Unity 6000,
      vendor-ошибки FMOD/Photon/MovieTexture не связаны с выносом).

## Задача: стресс по правилам кампании + каталог эффектов (по частям, простое → сложное)

### Цель

Стресс в дуэли должен работать «как в кампании». Ядро уже содержит стресс-движок
(`StressEffect`/`StressHealEffect`/`IStress`), но дуэль не применяет его: `DuelBattleContext.ApplyEffectById`
— пустой стаб, а каталога эффектов (`Effects.txt`) в ядре нет. Выносим по частям (от простого к
сложному): каталог эффектов → крит-стресс → потоковые стресс-правила → resolve → эффекты скиллов (P1.1).
Плюс заводим `docs\GAME_RULES.md` (вариант A): секции «Как в этом репо» / «Оригинал DD (позже)».

### Фаза 0 — Документ правил

1. [x] `docs\GAME_RULES.md`: секция «Стресс» по фактам кампании (крит 15 цели, смерть героя 15
    отряду, ретраит 15, голод 15, death's door 6, снятие 4; пасс — без стресса), сводная таблица,
    статусы, подблок «Оригинал DD»; `INDEX.md` обновить.

### Фаза 1 — Каталог эффектов + крит-стресс

2. [x] `Core.Combat`: `EffectCatalog` — парсер DSL `Effects.txt` (`.stress`→`StressEffect`,
    `.healstress`→`StressHealEffect`, остальное partial) → словарь; `Effect.ApplyIndependent` уже был.
3. [x] `IDuelContent.GetEffect`; `DuelBattleContext.ApplyEffectById` резолвит из каталога и применяет
    instantly → крит-стресс (15) и крит-хил-снятие (4) работают через core-классы.
4. [x] WPF `DuelContent.GetEffect` (груз `Effects.txt`); тесты линкуют `Effects.txt`; тест
    `Crit_AppliesStressToTheTargetHero` (6/6 зелёные). Доки/статусы.

### Фаза 2 — Стрессовые правила потока

5. [x] Смерть героя → «Stress 2» (15) выжившим союзникам (`DuelController.CheckDeaths`/`StressParty`,
    тест `HeroDeath_StressesTheSurvivingParty`). Пасс — как в кампании (без стресса).
    **Death's door → BarkStress (6)** — отложено: в дуэли 0 HP = смерть, механики death's door нет.

### Фаза 3 — Resolve-ролл (аффекция/виртуда)

6. [x] `Trait` + `BuffIds`; `Hero.ApplyTrait`/`RevertTrait` (удаляет trait-баффы); `JsonTrait`/
    `JsonTraitData`/`TraitMapper`; `IDuelContent.GetAfflictions/GetVirtues`; `DuelBattleContext.
    ResolveOverstress` (шанс виртуды 0.25+ResolveCheckPercent, клэмп 0.01–0.6, аффекция → стресс
    союзникам `AfflictedAllyStress` 33%×5, виртуда → стресс 20–40); WPF `DuelContent` грузит
    `JsonTraits.json`; тест `Overstress_TriggersResolveRoll` (8/8 зелёные).

### Фаза 4 — Эффекты скиллов (P1.1)

7. [x] `EffectCatalog` парсит общие не-бафф ключи `Effects.txt` (`.stress`, `.healstress`, `.heal`,
    `.stun`, `.dotBleed`/`.dotPoison`+`.duration`, `.pull`, `.push`, `.cure`, `.riposte`,
    `.shuffleparty`/`.shuffletarget`, `.tag`/`.mark`, `.immobilize`) → `SubEffect`; каталог
    case-insensitive. `HeroClassFileParser.Parse(content, effects)` / `HeroCatalog.Load(contents, effects)`
    резолвят `.effect "id"` (до 2) → `CombatSkill.Effects`; дуэль применяет через `BattleSolver.
    ApplyEffects`. WPF `DuelClasses` единый источник каталога. Тесты: `EffectCatalogTests`,
    `Parse_ResolvesSkillEffects_FromEffectsCatalog`, реальный ростер резолвит эффекты (38/38 combat).
7b. [x] Stat-баффы/дебаффы: `.combat_stat_buff` + `*_add`/`*_multiply`/`critical_rating`/
    `speed_rating[_add]` → `CombatStatBuffEffect`/`RiposteEffect` (StatAddBuffs/StatMultBuffs);
    `HeroClassFileParser.ParseTokens` собирает все значения ключа (`key#N`) → резолвятся все
    `.effect` (до N) скилла; **EventQueue drain** в `DuelController.ExecuteSkill`
    (`ProcessEventQueues` → `EffectEvent.Execute`) — иначе квеянные эффекты (стан/бафф/гард)
    никогда не применялись в дуэли. Тесты: `Load_ParsesStatBuffsAndRiposteStatMods`,
    `StatBuffSkill_AppliesStatBuffsToThePerformer` (take_aim: +6% acc, x1.12 dmg),
    `StunSkill_AppliesTheStunStatusToTheTarget` (10/10 duel, 39/39 combat).
7c. [x] `.buff_ids` → `BuffEffect.BuffIds`; `IBattleContext.GetBuff(string)` (дуэль резолвит через
    `IDuelContent.GetBuff`) → `BuffEffect.ApplyInstant/ApplyQueued` резолвят контент-баффы из
    `JsonBuffs.json` на лету; `EffectCatalog.ParseTokens` собирает все значения ключа (`key#N`).
    Тест `BuffIdEffect_AppliesContentBuffToTheTarget` (flashing_daggers → bleed_debuff_1: -20% к
    сопротивлению кровотечению; 11/11 duel).

### Фаза 5 — Прочее

8. [x] **Торч**: `EffectCatalog` парсит `.torch_decrease`/`.torch_increase` → `IntegerParams[Torch]`
    + Global target (торч-only эффекты сохраняются); `DuelBattleEvents.TorchDelta` → `DuelController`
    мьютит `Context.TorchAmount` (клэмп 0–100). Тест `TorchEvents_MutateTheDuelTorch`.
    **Лимиты скиллов**: `HeroClassFileParser` читает `.per_turn_limit`/`.per_battle_limit`/
    `.is_continue_turn`; `BattleSolver.IsSkillUsable` учитывает `SkillsUsedThisTurn`/
    `SkillsUsedInBattle`, `ExecuteSkill` пишет использование. Тест `SkillLimit_BlocksFurtherUsesAfterLimit`
    (13/13 duel, 41/41 combat).
    **Отложено**: кулдауны (только у монстров, в скиллах героев нет `.cooldown`), bark-реакции
    (кампанийная наррация).

### Фаза 7 — Сюрприз первого раунда

10. [x] `DuelController.CheckSurprise` при старте боя: шанс сюрприза монстров/героев
    `0.1 + torch-бонус диапазона` (Radiant 0.25 → Out 0.4) + `MonsterSurpirseChance`/
    `PartySurpriseChance` героев, клэмп 0.65; `BattleGround.SetSurpriseStatus`;
    `Round.NextRound` (раунд 0) ставит -100 к инициативе застигнутой стороны (действует последней);
    `IsSurprised` на юнитах, сюрприз-шаффл героев; снятие флага на своём ходу. Тест
    `SurpriseTests` (seeds 1/11: монстры/герои последними + флаг; 17/17 duel, 43/43 combat).

### Фаза 6 — Mode-система (Абоминация)

9. [x] Парсер: `mode:` секции → `HeroClass.Modes` (+`is_raid_default`); `.valid_modes` → `CombatSkill.
    ValidModes`; `.X_effects` → `ModeEffects[X]` (ключ DSL `<mode>_effects`, режим без `_`);
    Category=Support для accuracy-0/self-target скиллов (legacy-правило: без accuracy-ролла).
    `EffectCatalog`: `.set_mode` → `SetModeEffect`; `.on_miss`/`.queue`/`.apply_once`.
    `Hero` стартует в raid-default mode; `BattleSolver.IsSkillUsable` фильтрует по `CurrentMode`;
    `DuelController.FinishSkillAction` — continue-turn (тот же юнит действует снова);
    `FormationParty.AddUnit` линкует `unit.Party` (фикс `PerformersOther`-эффектов, напр.
    `beast_stress_party`). Тесты: `Parse_ReadsModesValidModesAndModeEffects`, `Load_ParsesSetMode`,
    `ModeTests` (human→transform→beast, rage/manacles по модам, continue-turn; 15/15 duel, 43/43 combat).

---

## Задача: фикс NRE квирков + полировка низа боя и шапки

### Выполнено

1. [x] **NRE-фикс**: `weapons_haggler`/`armor_haggler` (town-баффы «upgrade_discount») падали в
   `ApplyBuff` (`GetAttribute` null). `DuelController.ApplyQuirks` теперь применяет только баффы,
   атрибут которых есть у героя; регрессия `QuirkBuffTests` (все квирки применяются в дуэли).
2. [x] Низ боя — одна панель в рамке: скиллы + LOG/INVENTORY/MAP в верхней полосе (разделитель),
   ниже три секции (статы/тултип/лог) с вертикальными разделителями.
3. [x] Шапка едина везде: `ScreenHeaderView` (X + панель заголовка/квеста) с `Subtitle`; бой —
   «Duel» + статус; меню/лобби — без «скачков».
4. [x] Torch прибит к верху; скиллы меньше (50), текст под кнопкой капсом (как MOVE/PASS);
   Turn Order без «SPD» (только числа); имена по командам (красные/синие); лог — промахи и
   криты с шансом.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: боевой HUD, Turn Order по DD, общий хром, переиспользование в формировании отряда

### Цель

Отклик на визуальный проход: левая панель статов — к низу, панели низа одной высоты,
LOG/INVENTORY/MAP — в ряд со скиллами сверху, скиллы меньше и без «Skills», низ ниже → больше
центру; карточки к низу с отступом + пробел между отрядами; границы команд (красная/синяя) +
подсветка ходящего; «Round N» — ниже факела, не на поле; крестик всегда слева-сверху с панелью
заголовка/квеста вплотную; единый стиль закруглённых рамок; Turn Order шире, квадратики,
справа-налево, с реальной и нароленной скоростью, по правилам DD; слоты под изображения
(просто возможность); переиспользуемый тултип навыков; экран формирования отряда переиспользует
карточку боевого поля (портрет, стрелки вверх/вниз, статы, скиллы кроме Move/Pass, квирки).

### Фаза 1 — Core: инициатива наружу

1. [x] `InitiativeRoll` (double) на `IFormationUnitInfo`/`FormationUnitInfo`; `Round.NextRound`
    сохраняет `Speed + roll`. Ролл расширен 0-10 (DD-инициатива, не «всегда игрок 1 первый»);
    тест `Initiative_FirstActor_VariesBetweenTeams` (4/4 зелёные).

### Фаза 2 — Общий хром + верх

2. [x] `ScreenHeaderView`: крестик слева + панель заголовка справа вплотную (закруглённая рамка);
    бой: [X Retreat][QuestLogView «Duel»] вплотную.
3. [x] Закруглённые рамки (`OverlayPanel`) — на TURN ORDER, Duel, Torch и пр.
4. [x] `TurnOrderView`: шире (до Torch), «TURN ORDER» к правому краю, квадратики, скорость+
    нароленная, красные/синие рамки + белая у ходящего, слот изображения.
5. [x] «Round N» — из поля в верхнюю панель (под факелом).

### Фаза 3 — Низ боя

6. [x] Левая панель статов к низу; три панели одной высоты (заполняют низ).
7. [x] LOG/INVENTORY/MAP — в верхний ряд со скиллами (вплотную); скиллы меньше, «Skills» убрана;
    низ компактнее (1*) — больше центр (1.6*).

### Фаза 4 — Поле боя

8. [x] Карточки к низу с отступом, пробел между отрядами; рамки красная/синяя по командам,
    белая у ходящего (возврат); слоты изображений в карточках/скиллах/очередности (пока пустые).

### Фаза 5 — Формирование отряда (переиспользование)

9. [x] Слот в `HeroSlotsPanel` — как карточка боя: портрет/класс, стрелки вверх/вниз,
    статы (`HeroStatsView` + `HeroSlotViewModel.Stats`), скиллы-кнопки (все кроме Move/Pass)
    с тултипами (`Ui.SkillDetails`), квирки+реролл.

### Фаза 6 — Тултип

10. [x] Стилизованный переиспользуемый `ToolTip` (тёмная рамка) + богатый `Ui.SkillDetails` —
    на скиллах боя/лобби, классах, квирках.

### Фаза 7 — Проверка и доки

11. [x] build + тесты (duel 4, combat 35, content 15, wpf 16) + запуск + навигация меню→лобби→бой;
    `TESTING.md`, `CHANGELOG.md`, `PLAN.md` обновлены. Ручной визуальный проход — за пользователем.

### Затронутые файлы

`Round.cs`, `IFormationUnitInfo.cs`, `FormationUnitInfo.cs`, `ScreenHeaderView`, `Hud.xaml`,
`DuelBattleView.xaml`, `RaidHudView.xaml`, `TurnOrderView.xaml`, `DuelTurnEntryViewModel.cs`,
`DuelBattleViewModel.cs`, `EventsLayerView.xaml`, `TorchView.xaml`, `QuestLogView.xaml`,
`HeroSlotsPanel.xaml`, `HeroSlotViewModel.cs`, `PartySelectionView`, `MainMenuView`,
`DuelLobbyView`, `SinglePlayerLobbyView`, доки.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: адаптивный WPF + исправления боя и выбора отряда

### Цель

Клиент WPF не должен зависеть от фиксированного холста (1920×1080 + Viewbox): окно ресайзится,
экраны забирают всё доступное пространство, раскладка на `*`-строках/колонках. Заодно: починить
дефолт классов («все abomination»), богатый тултип скиллов, широкие тултип/лог/левая панель,
центр боя меньше / низ выше, Retreat «X» в квест-панели, переиспользуемая панель выбора отряда
(`#1 Player`/`#2 AI`), крестик в меню закрывает приложение. Коммиты по фазам.

### Фаза 1 — Адаптивная оболочка

1. [x] `MainWindow`: убран `Viewbox`/фикс. размер → ресайз (min 1100×700), `ContentControl`
   растягивается; все экраны — без фикс. `Width/Height`, на `*`-раскладке.
2. [x] `MainMenuView`: растягивается, заголовок «ВЫБЕРИТЕ РЕЖИМ», крестик закрывает приложение;
   `MainMenuViewModel.CloseCommand`.

### Фаза 2 — Боевой экран

3. [x] `DuelBattleView`: строки верх `Auto` / центр `*` / низ `2*` (центр меньше, низ выше);
   карточки в масштабируемой сцене (`Viewbox` поля), Retreat «X» в квест-панели, стат-лист —
   оверлей поверх всего экрана.
4. [x] `RaidHudView`: колонки левая 360 / центр `*` / правая `*`; `UnitTooltipView` растягивается;
   `LogView` крупнее (15pt); левая панель шире.

### Фаза 3 — Тултип скиллов

5. [x] `BuildSkillDetails` богаче (урон/меткость/крит/хил/ранги, лимит за ход); глобальный
   стилизованный `ToolTip` (тёмная рамка) в `Hud.xaml`.

### Фаза 4 — Выбор отряда

6. [x] Новый `PartySelectionView` (заголовок + слоты, DPs `PlayerLabel`/`Slots`); `HeroSlotsPanel`
   на `UniformGrid` 4 колонки (замощается по ширине); `SinglePlayerLobbyView` — две панели
   (#1 Player, #2 AI) + «Reroll AI»; `SinglePlayerLobbyViewModel.AiSlots` (редактируемые);
   `DuelLobbyView` — панель #1 Player + статус + session-контролы; случайные разные дефолты
   классов слотов (`AssignClass`); `DisplayNames.Class` (читаемые имена классов).

### Фаза 5 — Кнопка закрыть

7. [x] `ScreenHeaderView` везде: меню — закрыть приложение, лобби — назад, заголовки
   «ВЫБЕРИТЕ РЕЖИМ» / «СФОРМИРУЙТЕ ОТРЯД».

### Фаза 6 — Проверка и доки

8. [x] build + тесты (duel 3, combat 35, content 15, wpf 16) + запуск + навигация меню→лобби→бой
   (UI Automation) — работает; `TESTING.md`, `CHANGELOG.md`, `PLAN.md` обновлены. Ручной
   визуальный проход и подгонка пропорций — за пользователем.

### Затронутые файлы

`MainWindow.xaml`, `MainMenuView(.xaml)`, `MainMenuViewModel.cs`, `DuelBattleView.xaml`,
`RaidHudView.xaml`, `UnitTooltipView.xaml`, `LogView.xaml`, `HeroInfoPanelView.xaml`,
`ScreenHeaderView(.xaml)`, `HeroSlotsPanel.xaml`, `HeroSlotViewModel.cs`,
`SinglePlayerLobbyView(.xaml)`, `SinglePlayerLobbyViewModel.cs`, `DuelLobbyView(.xaml)`,
`DuelBattleViewModel.cs`, новый `PartySelectionView.xaml(.cs)`, `AGENTS.md` (правило
адаптивной раскладки), доки.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: ИИ дуэли ведёт себя как в Darkest Dungeon (поверх ядра, без правок оригинала)

### Цель

Соперник в vs-AI должен действовать как монстр в DD: взвешенные skill/target-desires
(по `base_chance`), хилер лечит раненого союзника ниже порога HP, цели — random/marked/health,
кулдауны после использования скилла, выбор через детерминированный `RandomSolver`.
**Оригинал не трогаем** (Core.Combat и legacy Unity остаются как есть) — реализация «поверх»:
DD-зеркальные желания и брейн в `src\Core\Sektor.DarkestDungeon.Core.Duel`, расширяя базовые
(не-sealed) классы `SkillSelectionDesire`/`TargetSelectionDesire`. Коммиты по фазам.

### Фаза 1 — Core.Duel: DD-зеркальный ИИ

1. [x] `DuelSkillSelection` (rework): случайный скилл через **базовый** `SelectSkill`
   (RandomSolver, DD-цикл), `GetMonsterCombatSkills` → `CurrentCombatSkills`,
   `GetMonsterBrain` → внедрённый брейн.
2. [x] `DuelSkillSelectionHeal` (новый): `skill.Heal != null`, цель с HP < порога,
   только Health-цель-desire (корректная DD-логика хила).
3. [x] `DuelTargetSelectionRandom` / `DuelTargetSelectionMarked` / `DuelTargetSelectionHealth`
   (новые): random (враж.), marked-фильтр, health — сортировка по `HealthRatio` с
   `is_greater_comparison` и enemy/friendly-флагами (обязательный ключ
   `specific_combat_skill_id=""` — DD-JSON всегда его задаёт).
4. [x] `DuelAi`: строит DD-«default» брейн (heal 100/<0.5 + random 1; цели random 2/enemy +
   marked 1/enemy + health 100/friendly), гоняет DD-цикл (`ChooseByRandom` → `SelectSkill` →
   кулдаун), возвращает payload. Старый `DuelTargetSelection` (min-HP) удалён.
5. [x] `DuelAiTests`: lockstep (зелёный) + «хилер лечит раненого союзника» (детерминированный).

### Фаза 2 — Документация

6. [x] Новый `docs\AI_BEHAVIOR.md`: модель AI DD (брейн, desires, кулдауны/лимиты, `JsonAI.json`,
   цикл выбора, зеркало в дуэли, разрывы/будущее: парсинг эффектов P1.1).
7. [x] Правки: `INDEX.md`, `DUEL_ARCHITECTURE.md`, `PLAN.md`, `TESTING.md`, `CHANGELOG.md`.

### Затронутые файлы

- `src\Core\Sektor.DarkestDungeon.Core.Duel\DuelAi.cs`, `DuelSkillSelection.cs`,
  `DuelSkillSelectionHeal.cs`, `DuelTargetSelectionRandom.cs`, `DuelTargetSelectionMarked.cs`,
  `DuelTargetSelectionHealth.cs`; удалить `DuelTargetSelection.cs`.
- `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests\DuelAiTests.cs`; документы.

### Критерии приёмки

- vs-AI: хилеры лечат раненого союзника (<50%), прочие — random/marked атаки; кулдауны применяются;
  поведение детерминировано (RandomSolver, сид). Core.Combat/Unity — без изменений.
- Тесты зелёные; доки обновлены.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: тонкий WPF — вынос дуэли в ядро (A), ИИ на MonsterBrain (B), документация (C)

### Цель

Дуэльная оркестрация (локстап PvP: host=герои, rival=«сторона монстров») сейчас живёт в
WPF-клиенте (`Combat\DuelController.cs` 413 строк и др.). Это ре-имплементация Unity-мультиплеера,
который в `unity\Assets\Scripts\Networking\RaidSceneMultiplayerManager.cs` (2285 строк) +
`MultiplayerSync.cs` (426 строк) не «разнесён» по слоям. Фаза A: вынести оркестрацию в чистый
core-модуль `Sektor.DarkestDungeon.Core.Duel`; WPF становится тонким. Фаза B: ИИ соперника на
`MonsterBrain`-инфраструктуре ядра. Фаза C: документация (новый `docs\DUEL_ARCHITECTURE.md` +
правки INDEX/ARCHITECTURE/KNOWN_ISSUES/FEATURE_DESKTOP_CLIENT/AGENTS/EXTRACTION_PLAN/CHANGELOG),
чтобы агенты быстро ориентировались. Коммиты: A, B, C — отдельными.

### Фаза A — вынос в `src\Core\Sektor.DarkestDungeon.Core.Duel`

1. [x] Новый модуль (netstandard2.0, C# 7.3, Nullable disable; ссылки Core.Combat + Core.Content;
   post-build доставка в `Assets\Plugins\Internal` обоих деревьев — как `Core.Combat.csproj`).
2. [x] Переезд: `DuelController` + `DuelHeroPick`, `DuelPhase`, `DuelSeed`, `DuelBattleContext`,
   `DuelBattleEvents`; новые `IDuelContent` (`GetHeroClass/GetQuirk/GetBuff`) и `DuelPayload`
   (wire-парсинг `skill|target` / `move|rank` / `pass|0`). Снять nullable-аннотации под C# 7.3.
3. [x] WPF: `DuelContent` (реализация `IDuelContent` поверх `DuelClasses`/`QuirkCatalog`/`BuffCatalog`);
   обновить `using`/точки создания в VMs и линках (`AiRivalLink`, `NetworkRivalLink`, `IDuelRivalLink`);
   удалить переехавшие файлы.
4. [x] Тесты: `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests` (net10.0, NUnit+NSubstitute) —
   `DuelTurnFlowTests` (локстап, `TestDuelContent` из связанного контента); WPF VM-тесты остаются.
5. [x] Проверка: build + dotnet test (duel 1, combat 35, content 15, wpf 16) + запуск приложения;
   Core.Duel.dll доставлен в оба `Assets\Plugins\Internal`.

### Фаза B — ИИ на MonsterBrain

6. [x] Core `DuelAi` в Core.Duel: выбор скилла+цели соперника через AI-инфраструктуру ядра
   (`MonsterBrain`/`DuelSkillSelection`/`DuelTargetSelection`/`MonsterBrainDecision`). Выбор — на
   клиент-локальном `System.Random`, чтобы не трогать `RandomSolver` и сохранить локстап;
   цель — по минимальному HP (умнее случайного).
7. [x] WPF `AiRivalLink` → тонкая обёртка (таймер + `DuelAi` + `RivalActionReceived`);
   тесты: `DuelAiTests` (локстап обеих сторон с ИИ) — 2/2 зелёные; WPF 16/16.

### Фаза C — документация

8. [x] Новый `docs\DUEL_ARCHITECTURE.md`: что такое дуэль, происхождение (Unity-мультиплеер PvP),
   инвентарь по слоям, критика (логика в презентации, god-classes, дубли оркестрации/протокола,
   случайный ИИ, нестабильный сид), роадмап (B, cutover Unity, фаза 6).
9. [x] Правки: `INDEX.md`, `ARCHITECTURE.md`, `KNOWN_ISSUES.md`, `FEATURE_DESKTOP_CLIENT.md`,
   `AGENTS.md`, `EXTRACTION_PLAN.md`, `CHANGELOG.md` (только B — видимое поведение ИИ).

### Затронутые файлы

- Новые: `src\Core\Sektor.DarkestDungeon.Core.Duel\*`, `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests\*`,
  `docs\DUEL_ARCHITECTURE.md`.
- Изменённые: `src\Wpf\...\ViewModels\*`, `...\Combat\AiRivalLink.cs`, `...\Networking\*`,
  `src\Wpf\...\Data\DuelContent.cs`, документы.

### Критерии приёмки

- WPF теряет ~700+ строк доменной логики; Core.Duel чистый (netstandard2.0, C# 7.3, без engine-ссылок).
- После A поведение дуэли идентично (тесты зелёные). B: ИИ через MonsterBrain, `AiRivalLink` тонкий.
- Документация обновлена; агенты ориентируются по AGENTS.md + INDEX.md + DUEL_ARCHITECTURE.md.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: ускорить pre-commit проверку скрипт-GUID (ripgrep + параллельно + fast-path)

### Цель

Pre-commit хук тратит ~104 с на каждый коммит (скан `unity-check-script-references.ps1`
на `unity` и `unity-2017` последовательно). Сделать: скан на ripgrep (~2-5 с/проект),
параллельный запуск обоих проектов в хуке, и быстрый путь — пропуск скана, когда не
менялись файлы под `unity/`/`unity-2017/` (WPF-коммиты ~0.5 с). Защита от stale-GUID
сохраняется.

### Шаги

1. [x] `tools/unity-check-script-references.ps1` — переписан на ripgrep:
   индекс guid `rg -o --no-filename --replace '$1' '^guid: ([0-9a-f]+)' -g '*.meta'`;
   ссылки `rg -o --no-heading --replace '$1' 'm_Script: ... guid: ([0-9a-f]+)' -g '*.unity' -g '*.prefab'`
   (разбор path:guid по длине — последние 32 hex); `.cs.meta` через `rg --files -g '*.cs'` +
   `Test-Path`. Фолбэк на прежнюю PS-реализацию, если `rg` отсутствует. Контракт
   (`builtinGuids`, формат ошибок, exit code) — без изменений. Замеры: unity ~1.5 с,
   unity-2017 ~2.7 с (было ~52 с/проект).
2. [x] `.githooks/pre-commit` — быстрый путь: если `git diff --name-only HEAD` +
   `git ls-files --others --exclude-standard` не содержат путей `unity/`/`unity-2017/` →
   «No Unity changes, skipping», `exit 0` (~0.35 с). Иначе оба проекта параллельно
   (`&` + `wait`, exit 1 при любой ошибке); Unity-коммит ~1.9 с wall.
3. [x] Документация: `AGENTS.md` (хук гоняет проверку параллельно/ripgrep и пропускает
   при отсутствии изменений в Unity), `TESTING.md` (заметка в «Автопроверки»), `PLAN.md`.
4. [x] Проверка: скан обоих проектов чистый и быстрее; фолбэк без `rg`; fast-path
   (только `src/` → мгновенно, `unity/` → сканирует параллельно); синтетика ловит
   stale-GUID в rg-пути.

### Затронутые файлы

- `tools/unity-check-script-references.ps1`
- `.githooks/pre-commit`
- `AGENTS.md`, `TESTING.md`, `PLAN.md`

### Критерии приёмки

- WPF-коммит: хук завершается < 1 с, скан не запускается.
- Unity-коммит: оба проекта сканируются параллельно на ripgrep, ~5-10 с.
- Скан по-прежнему ловит stale-GUID (ошибки как раньше, exit 1).

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Задача: WPF-дуэль — фиксы UI (тултип, левая панель, Move/Pass, Абоминация, лобби)

### Цель

Привести боевой HUD дуэли и оба лобби (vs AI и мультиплеер) к единому широкоформатному
виду без перекрытий: починить обрезанный тултип, вернуть левой панели DD-корректный
состав, сделать Move/Pass квадратными с глифами, устранить крэш Абоминации при
превращении, перестроить лобби рядами сверху вниз с крестиком-возвратом сверху.

### Шаги

1. [x] **Крэш Абоминации** — в `DuelBattleView.xaml` убрана анимация
   `(UIElement.RenderTransform).(TranslateTransform.Y)` (замороженный Freezable в
   шаблоне карточки); анимируются только элементные DP — `Opacity` + `Margin`.
2. [x] **Левая панель (DD-корректно)** — `HeroBannerView`: убраны слоты скиллов;
   `HeroStatsView`: добавлен DP `ShowFullDetails` (default false), скрывающий секции
   SKILLS/RESISTANCES/QUIRKS; листы статов правого клика (`DuelBattleView`,
   `BattleScreenView`) — `ShowFullDetails="True"`.
3. [x] **Тултип** — `RaidHudView`: левая колонка 690→`Auto`, центр `*`;
   `UnitTooltipView`: убран жёсткий `Width="560"`, размер по контенту.
4. [x] **Move/Pass** — квадратные 64×64 как скиллы, глиф `⇄` у Move и `✕` у Pass,
   подпись под кнопкой.
5. [x] **Лобби** — новый общий `ScreenHeaderView` (заголовок + крестик-возврат);
   `DuelLobbyView` и `SinglePlayerLobbyView` → 1920×1080 рядами: верх = 4 героя игрока,
   середина = ИИ/второй игрок (неактивно для живого PvP), низ = кнопки.
6. [x] **Доки** — `TESTING.md` (WPF чек-лист + «Что проверить»), `CHANGELOG.md`;
   шаги здесь отмечены `[x]`. Добавлен `ScreenSmokeTests` (загрузка всех экранов).

### Затронутые файлы

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelBattleView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\HeroBannerView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\HeroStatsView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\HeroStatsView.xaml.cs` (DP)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\RaidHudView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\UnitTooltipView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\BattleScreenView.xaml` (ShowFullDetails=True)
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelLobbyView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\SinglePlayerLobbyView.xaml`
- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\ScreenHeaderView.xaml` + `.xaml.cs` (новый)
- `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\ScreenSmokeTests.cs` (новый)
- `docs\TESTING.md`, `docs\CHANGELOG.md`, `PLAN.md`

### Критерии приёмки

- Превращение Абоминации не роняет WPF-клиент (`XamlParseException` исчез); обычные
  попапы урона/хила работают.
- Левая панель боя: портрет+имя, статы без резистов, шмот и 2 слота тринкетов; скиллов
  и секций SKILLS/RESISTANCES/QUIRKS нет. Лист статов правым кликом — со всем полным
  набором.
- Тултип при наведении полностью виден, не перекрывается левой панелью, не выходит
  за экран.
- MOVE/PASS квадратные (64×64), у MOVE стрелки, у PASS крестик, подписи снизу.
- Оба лобби на 1920×1080, элементы в рядах сверху вниз, ничего не прячется под другим,
  крестик-возврат сверху, единый стиль.
- `dotnet build` и `dotnet test tests/Wpf/...` зелёные; ручной прогон по `TESTING.md`.

---

## Остаток работ (после WPF-дуэли, сентябрь 2026)

## Что уже сделано (зафиксировано)

WPF-дуэль работает: меню → лобби (классы, активные скиллы 4/7, черты+reroll, подсказки) →
бой (полный HUD, детерминированный локстап, Move/Pass, подробный лог, всплывающий урон,
HP блоками + стресс 10 квадратов, тултипы, лист статов со всеми скиллами/резистами/квирками).
Квирки влияют на статы (permanent-баффы `BuffSourceType.Quirk`). Вынесено в ядро: квирки,
буффы (`JsonBuffs.json`), константа активных скиллов, `SelectedCombatSkills`, `Hero.Quirks`.
Детали — в `CHANGELOG.md`, механики — в `FEATURE_DESKTOP_CLIENT.md` §«Механики боя».

## Осталось — по приоритету

### P1. Механики боя, которых ещё нет (самое ценное)

1. [ ] **Эффекты скиллов (статусы)** — парсер `.bytes` должен читать эффекты `combat_skill`
   (стун, блайт, блед, баффы/дебаффы, riposte, guard, пулл/пуш) и заполнять
   `CombatSkill.Effects`. Ядро готово (`BattleSolver.ApplyEffects`, классы эффектов в
   `Mechanics\Skills\Effects\`), но скиллы сейчас — чистый урон/хил (комментарий в
   `HeroClassFileParser.cs:13`). Самый большой механик-разрыв.
2. [ ] **Resolve / аффекция / добродетель / сердечный приступ** — триггер при 100 стресса
   (`Resolve`, `Trait`, `OverstressType` есть в ядре, вызова нет). Набор виртуадов/аффекций
   из контента (trait_buffs) пока не вынесен.
3. [ ] **Modes (Абоминация)** — парсинг `.mode` в контент, `CharacterMode`/`ModeEffects`
   (ядро поддерживает).
4. [ ] **Транкеты / экипировка** — Unity-дуэль несёт их с героя из имения; в WPF-дуэли нет.
   Большая фича (парсинг `JsonTrinkets`/`JsonBuffs`, слоты, влияние на статы).

### P2. Сопутствующее WPF-клиенту

5. [ ] **Факел/свет в бою** — механика света (light buffs, темнота) в Unity влияет на бой;
   сейчас факел — плейсхолдер.
6. [ ] **ИИ-соперник** — сейчас случайные легальные ходы (`AiRivalLink`); можно использовать
   `MonsterBrain`/предпочтения скиллов для «умного» противника.
7. [ ] **Локализация** — имена квирков/скиллов из `Localization\Quirks.xml` (сейчас id).

### P3. Полный вынос из Unity (фазы EXTRACTION_PLAN)

8. [ ] Фаза 2: сейвы (`src\Core\Save`).
9. [ ] Фаза 4: кампания/имение/здания/квесты/город.
10. [ ] Фаза 5: Photon-транспорт (WPF сейчас на Steam).
11. [ ] Фаза 6: тонкие адаптеры презентации в обоих Unity-проектах.

### P4. Проверка и качество

12. [ ] Ручной чек по `TESTING.md` (меню → vs AI → бой; мультиплеер 2 инстанса; лог/попапы/
    MOVE/PASS/черты).
13. [ ] Глядя на тесты: расширить покрытие локстапа (двусторонний одинаковый бой с квирками).

## Задача: WPF-дуэль — порядок рангов отряда 1 (4→1) + вынос правила раскладки в ядро

### Цель

В WPF-дуэли отряд 1 (герои, левая сторона) отображался слева-направо рангами 1,2,3,4 —
фронт (ранг 1) оказывался у левого края, вдали от врага. В DD фронт должен смотреть на
врага: слева-направо **4,3,2,1** (ранг 1 — у центра). Вражеская сторона (1,2,3,4) корректна.
Причина: `DuelBattleViewModel.RefreshUnits()` кладёт `Heroes` в порядке `HeroParty.Units`
(`Units[0]` = ранг 1), XAML рендерит в left-aligned StackPanel.

Правило «ранг 1 — фронт, герои слева и смотрят вправо» в Unity есть, но зашито в презентацию
(`unity\`/`unity-2017\`: `FormationRanks` — `facingRight`, `Slots.Reverse()`, `Units[Count-i-1]`;
`FormationRanksSlot` — `SetSiblingIndex(4-Rank)`/`Rank-1`; `BattleFormation`). Выносим сущность
правила в чистое ядро как инстансный класс (движковые типы в netstandard2.0 не переносятся;
имя `FormationRanks` в ядре занято маркерами таргетинга). **Объём A (согласован):** ядро + WPF;
Unity не трогаем до cutover `Core.Duel` (фаза 6 EXTRACTION_PLAN).

### Шаги

1. [x] `src\Core\Sektor.DarkestDungeon.Core.Combat\Raid\Party\FormationDisplayOrder.cs` (новый,
   netstandard2.0/C# 7.3, один публичный тип, XML-доки): поле `bool facesRight` (семантика
   Unity `FacingRight`); `List<ICombatUnit> OrderLeftToRight(IFormationParty party)` — сортировка
   по `Rank` + реверс при `facesRight`; фабрики `HeroSide()` (facesRight: true) / `MonsterSide()`
   (false) — конвенция «герои слева, фронт к врагу» в одном месте.
2. [x] `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\DuelBattleViewModel.cs` — `RefreshUnits()`:
   герои через `FormationDisplayOrder.HeroSide()`, монстры через `MonsterSide()` (+`using
   Sektor.DarkestDungeon.Core.Combat.Raid.Party;`). Таргетинг/ход не затрагиваются
   (`CombatId`/`Rank`).
3. [x] Тесты: `tests\Core\Sektor.DarkestDungeon.Core.Combat.Tests\Raid\Party\
   FormationDisplayOrderTests.cs` (NSubstitute): `HeroSide`→[4,3,2,1], `MonsterSide`→[1,2,3,4],
   устойчивость к разбросанному порядку списка. Регрессия в `tests\Wpf\...\DuelRenderTests.cs`:
   ранги `view.Heroes` = реверс рангов `duel.HeroParty.Units`; `view.Monsters` = как в
   `duel.MonsterParty.Units` (устойчиво к surprise-shuffle).
4. [x] Доки: `docs\TESTING.md` (шаг 4 — «отряд игрока слева→направо 4→1, фронт к врагу»),
   `docs\CHANGELOG.md` (фикс + вынос правила в ядро).
5. [x] Проверка: `dotnet test tests\Core\Sektor.DarkestDungeon.Core.Combat.Tests`,
   `dotnet test tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests`, `dotnet build src\Wpf\...\Wpf.csproj`.
   Unity-compile-check не нужен (нет правок под `unity/`).

### Затронутые файлы

- Новые: `src\Core\...\Raid\Party\FormationDisplayOrder.cs`,
  `tests\Core\...\Raid\Party\FormationDisplayOrderTests.cs`.
- Изменённые: `src\Wpf\...\ViewModels\DuelBattleViewModel.cs`,
  `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\DuelRenderTests.cs`, `docs\TESTING.md`,
  `docs\CHANGELOG.md`, `PLAN.md`.

### Критерии приёмки

- Отряд 1 в дуэли слева-направо = ранги 4,3,2,1; враг — 1,2,3,4; Move/Pass, таргетинг,
  поп-апы, Turn Order работают как раньше (по `CombatId`/`Rank`).
- Правило живёт в ядре один раз (`FormationDisplayOrder`); Unity-код не изменён.
- Тесты зелёные; доки обновлены.

---

## Задача: единый порядок usings — снаружи namespace (owned src + tests)

### Цель

В 43 owned-файлах (`src\Lan` — 8, `tests\` — 35) `using`-директивы лежат внутри тела
`namespace` (стилистика StyleCop SA1200). Остальной код (`src\Core`, `src\Wpf`) использует
usings в начале файла, до `namespace`. Требование: привести все owned `/src`/`/tests` к единому
порядку — usings первым, затем `namespace`. Функциональных изменений нет (алиасы резолвятся на
уровне compilation unit); это чистый рефакторинг оформления.

Объём согласован: **только owned** — `src\Lan` и `tests\`. Legacy `unity\`/`unity-2017\`
(включая `SaveLoadManager.cs`) и vendored (Photon/Spine/FMOD) не трогаем. Анализаторов и
`.editorconfig` в репо нет — регрессии стиля не будет ни с той, ни с другой стороны.

### Шаги

1. [x] `src\Lan\` (8): `Sektor.DarkestDungeon.Lan.Cmd\Program.cs`,
   `Sektor.DarkestDungeon.Lan.Contracts\Transport\ITransport.cs`,
   `Sektor.DarkestDungeon.Lan.Steam\SteamTransport.cs`, `...\JsonTransportCodec.cs`,
   `...\Interop\SteamRuntime.cs`, `...\Interop\SteamNative.cs`, `...\Interop\SteamCallbacks.cs`,
   `...\Interop\NativeUtf8.cs`.
2. [x] `tests\Core\Sektor.DarkestDungeon.Core.Combat.Tests` (12): `EffectCatalogTests`,
   `HeroClassFileParserTests`, `HeroSkillSelectionTests`, `Mechanics\BattleSolverTests`,
   `Mechanics\DeterminismTests`, `Mechanics\EffectTests`, `Mechanics\FormationSetTests`,
   `Mechanics\MonsterBrainTests`, `Mechanics\RandomSolverTests`, `Mechanics\RecordingSubEffect`,
   `Mechanics\RoundTests`, `Raid\Party\FormationDisplayOrderTests`.
3. [x] `tests\Core\Sektor.DarkestDungeon.Core.Content.Tests` (7) и
   `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests` (7): Database-тесты (`BuffContentMapperTests`,
   `CurioCsvParserTests`, `HeirloomExchangeMapperTests`, `LootMapperTests`, `NarrationMapperTests`,
   `PartyNameMapperTests`, `QuirkMapperTests`) и `DuelAiTests`, `DuelTurnFlowTests`, `ModeTests`,
   `QuirkBuffTests`, `SkillEffectsTests`, `StressTests`, `SurpriseTests`.
4. [x] Остальные тесты (9): `UiStyleTests`; `Lan.Tests` (`Codec\JsonTransportCodecTests`,
   `Support\InMemoryTransport`, `Transport\MessageRoundTripTests`, `Transport\TransportLifecycleTests`);
   `Wpf.Tests` (`DuelFlowTests`, `DuelRenderTests`, `LobbySlotTests`, `ScreenSmokeTests`).
5. [x] Проверка: `dotnet build` всех затронутых проектов (Lan.*, тестовые) + полный прогон
   тестов (`dotnet test`, 111 зелёных); линтер `tools\check-using-placement.ps1` = 0.
   Unity-compile-check не нужен (правок под `unity/` нет).

### Механика правки (одинаково для всех 43)

- Вырезать блок `using ...;` из тела `namespace` (после `{`), вставить в начало файла до
  `namespace`; убрать одинаковый отступ (4 пробела). Сохранить существующие пустые строки и
  порядок групп (System / third-party / first-party) как есть. Пустую строку между usings и
  `namespace` — оставить.

### Затронутые файлы

- 43 owned-файла: 8 в `src\Lan\`, 35 в `tests\`. Документация обновляется только в `PLAN.md`;
  `docs\CHANGELOG.md` — косметика, не user-visible, не трогаем.

### Критерии приёмки

- Во всех owned файлах `src\` и `tests\` `using` стоят до `namespace`.
- Все сборки компилируются; все тестовые сьюты зелёные; под `unity/`, `unity-2017\`,
  `src\External\`, `src\Wpf\Sources\`, `src\Core\` — диффа нет.

---

## Задача: защитить правило «using до namespace» (3 слоя + AGENTS.md)

### Цель (план на будущее, после основного рефакторинга)

Паттерн «using внутри namespace» — легальная конвенция StyleCop SA1200, от которой уже нет
смысла что-то отстаивать: всюду по проекту (включая `src\Core`, `src\Wpf`) принят противоположный
порядок — usings снаружи. Нужно зафиксировать стандарт и не пускать регрессию. Объём защиты —
только owned `src\` и `tests\` (vendored и legacy Unity не трогаем ничем).

### Шаги

1. [x] `AGENTS.md` — добавлен пункт про Using Placement в секцию «III. Clean Code &
   Documentation»: в owned C# (`src\`, `tests\`) все `using`-директивы в начале файла, до
   `namespace`; не внутри тела namespace (SA1200).
2. [x] Корневой `.editorconfig`: секции `[src/**/*.cs]` и `[tests/**/*.cs]` с
   `csharp_using_directive_placement = outside_namespace:warning` (IDE0065),
   `dotnet_sort_system_directives_first = true` и `dotnet_separate_import_directive_groups = true`.
   Legacy/vendored Unity (`unity\`, `unity-2017\`) исключены.
3. [x] `tools\check-using-placement.ps1`: сканирует `src\` и `tests\` (кроме `src\External\`,
   obj/bin), находит owned-файлы с `using`-директивой с отступом (после `namespace {`),
   exit code 1 при находке. `using (...)`-statement'ы и `using var` в телах методов не считаются
   директивами и игнорируются.
4. [x] `.githooks\pre-commit`: перед Unity-веткой добавлен вызов
   `tools\check-using-placement.ps1`, когда коммит содержит owned `.cs` под `src/`/`tests/`
   (staged + untracked). Синтаксис проверен (`bash -n`, exit 0).
5. [x] Verify: staged-проба owned-файла с using внутри namespace → хук падает (exit 1, файл
   назван); чистый индекс → exit 0 (fast-path «No Unity changes»). Негативный кейс линтера
   через `-Roots` на sandbox: `Bad.cs` пойман (exit 1), `Good.cs` пропущен.

### Затронутые файлы

- `AGENTS.md`, новый `src\.editorconfig` (или корневой), новый `tools\check-using-placement.ps1`,
  `.githooks\pre-commit`, `PLAN.md`. Сам рефакторинг 43 файлов — отдельная задача выше.

### Критерии приёмки

- ИИ-агент (по AGENTS.md) и человек (по IDE) больше не создают owned-файл с using внутри namespace.
- Защита не пересекается: vendored/legacy Unity исключены из проверки; pre-commit не замедляет
  C#/docs-коммиты ощутимо (быстрый rg-проход по staged owned `.cs`).

---

## Задача: паритет-документация, карта Unity-легаси, критика и целевая декомпозиция

### Цель

Зафиксировать, что и как реализовано/различается между мультиплеерной дуэлью Unity и WPF-дуэлью
(ядром); полностью задокументировать Unity-легаси (какие механики и в каких классах); дать
профессиональную архитектурную критику (Unity + текущего ядра + разделения проектов); предложить
целевую декомпозицию `src\Core\` по доменам (данные = домен). **Легаси Unity не трогаем** — только
документируем; разрывы закрываются в ядре.

### Решения

- Глубина карты: класс-уровень — доменные папки (Mechanics/Character/Raid/Campaign/Database),
  папка-уровень — презентация (UI/Managers/Setup/Networking/Generation/ImageEffects/Sounds).
- Данные = домен: `Core.Data` складывается в доменные модули, `GameDataReader` остаётся тонким
  фасадом (подтверждено).
- Критика — отдельный `docs\ARCHITECTURE_REVIEW.md`.
- Эталон — активный `unity\`; расхождения `unity-2017\` фиксируются только где существенны.

### Порция 1 — Паритет и правило «легаси не трогаем»

1. [x] `docs\BATTLE_PARITY.md`: матрица Unity MP vs WPF-дуэль/ядро — «одинаково» / «разрыв» (file:line)
     / «стаб в обоих»; группы эффектов (DoT, stun, riposte, guard, pull/push/shuffle, immobilize,
     deaths-door, heart attack, rule-баффы, мультиплеер-специфика); скиллы-жертвы (ManAtArms
     Defender/Retribution, HoundMaster Guard Dog, DoT/stun-скиллы).
2. [x] `AGENTS.md` + `EXTRACTION_STATUS.md`: правило «легаси Unity живёт до cutover; разрывы
     отслеживаются в `BATTLE_PARITY.md`, в Unity не правятся».
3. [x] `INDEX.md`, `PLAN.md`.

### Порция 2 — Карта Unity-легаси

4. [x] `docs\UNITY_LEGACY_MAP.md` суб-порциями по папкам (Managers/Setup → Mechanics → Character →
     Raid → Campaign → Database → Networking → Generation → UI/ImageEffects/Sounds). Формат: папка →
     ответственность → ключевые классы (god-классы с размером) → механики → статус выноса. Каждая
     суб-порция — отдельным коммитом + точечное обновление `EXTRACTION_STATUS.md`.

### Порция 3 — Фронт выноса

5. [x] Углубить `EXTRACTION_STATUS.md` (пер-модульные разрывы) + `PLAN.md` дорожная карта по доменам:
     Save, Campaign, Encounters/Curios/Loot, Networking, Presentation cutover, закрытие механик-разрывов
     из BATTLE_PARITY (в ядре).

### Порция 4 — Критика

6. [x] `docs\ARCHITECTURE_REVIEW.md`: Unity-легаси (god-классы, синглтоны, корутины, magic-strings,
     RPC vs локстап, сид-хак, мёртвый код) + текущее ядро (слои Data→Duel, DTO-сплит Content vs Data,
     Result-инвариант, мёртвый код, stale-доки, Newtonsoft vs 2017.4, `Core.Ui`). Каждый пункт —
     файл:строка + рекомендация.

### Порция 5 — Целевая декомпозиция

7. [x] `docs\TARGET_LAYOUT.md`: раскладка `src\Core\<модуль>` по доменам (Common/Content/Combat/
     Campaign/Raid/Save/Duel/Networking/Ui), правила зависимостей (DAG, без восходящих), что куда
     переезжает (складывание Core.Data, TextFightContent → Duel), обоснование, миграционный путь.

### Порция 6 — Проверка

8. [x] `INDEX.md`, финальный проход `tools\check-using-placement.ps1`, build/test (если затронут код),
     сверка `PLAN.md`.

### Критерии приёмки

- Каждая строка паритета и критики сверена с кодом (file:line).
- Карта покрывает все 502 файла; манифест выноса согласован с картой.
- Из карты+паритета+декомпозиции виден полный приоритизированный фронт работ.
- Unity-легаси не изменён; доки в том же коммите, что и затронутый код.

---

## Задача: библиотеки доменов (данные = домен) — исполнено

### Цель

Завести библиотеки под вычисленные домены (см. `TARGET_LAYOUT.md`): `Core.Common`, `Core.Save`,
`Core.Campaign`, `Core.Raid`, клиентская граница `Clients.Content`; распустить `Core.Data`
(данные = домен: DTO/парсеры/каталоги живут в модуле своего домена, Newtonsoft — на границе).

### Шаги

1. [x] `Core.Common` — `Result`/`Result<T>` из `Lan.Contracts` (+правка Lan/Wpf), `IProportionValue`/
     `ISingleProportion` из `Content\Raid`; ребро `Combat → Content` убрано (Combat зависит только от
     Common после переноса примитивов).
2. [x] `Core.Save` — `IBinarySaveData` из `Content\Save`; `Content\Raid\Prop → Core.Save`.
3. [x] `Core.Campaign` — модели `Content\Campaign\*` + мапперы + DTO из `Content\Database` и `Data\Dto`
     (`JsonQuests/TownEvent/Building/Upgrades/Roster/Provision/Inventory/...`); тесты → `Campaign.Tests`.
4. [x] `Core.Raid` — модели `Content\Raid\*` (Curio/Prop/AreaType) + `CurioCsvParser`/`Loot`/`CsvReader`
     + DTO; `JsonCurrencyCost` → `Content\Database` (общий); тесты → `Raid.Tests`.
5. [x] Brains: `JsonMonsterBrains` DTO + чистый `JsonBrainParser` + `MonsterBrainCatalog` → `Combat\AI`;
     Newtonsoft-десериализация — в `GameDataReader`.
6. [x] `Clients.Content` (`src\Clients\`) — `GameDataReader` (Newtonsoft-фасад); `BuffCatalog`/`QuirkCatalog`
     стали чистыми (`Load(Json*Data)`) и переехали в `Combat\Character` / `Content\Character`;
     `TrinketCatalog`/`CampingSkillCatalog` + DTO — в `Content\Trinket|Camping`.
7. [x] `TextFightContent` → `Core.Duel\Fight`; `FightContentLoader` (оба Unity-дерева) обновлён;
     **`Core.Data` распущен**; `GameDataReaderTests`/`FightSessionTests` → `Clients.Content.Tests`.
8. [x] Проверка: `dotnet build` + все 9 тест-сьютов зелёные (Combat 49, Duel 17, Content 5, Campaign 6,
     Raid 4, Clients.Content 10, Ui, Lan 14, Wpf 17); `unity-compile-check` для `unity\` — зелёный;
     `unity-check-script-references.ps1` для обоих деревьев — зелёный; `check-using-placement` — зелёный.

### Примечание по проверке `unity-2017\`

`unity-2017\` в этом окружении компилируется редактором Unity 6000 (2017.4 не установлен) → известные
vendor-ошибки (FMOD `EventBrowser`, Photon `Hashtable`/`GUIText`, `MoviePlayer.MovieTexture`) — **не
относятся к этому выносу**. Ошибок по типу `Core.*` в 2017-дереве нет (мои изменения компилируются чисто);
script-reference check — зелёный. Финальную проверку 2017.4 проводит человек в реальном редакторе.

---

## Правила

- Сначала ядро (`src\Core`), потом адаптеры; `src\External\` — read-only.
- Доки обновляются в том же коммите; `CHANGELOG.md` — только user-visible.
- Всё новое — минимальными диффами, без оппортунистических рефакторов legacy.


---

# Plan: DuelUnitCardView as a CCG-style card + canonical hero names (WPF duel)

## Задача (завершено)

1. Исправить баг «перепутанных имён»: имена героев в дуэли генерировались из общего пула
   случайным образом по сиду и не соответствовали классу (Alhazred на Hellion, Boudica на PD —
   и дублировались между командами). Теперь у каждого класса каноническое имя.
2. Переделать `DuelUnitCardView` в цельный прямоугольный «CCG-стайл» карточку:
   - шапка: имя героя и позиция (ранк) в одном ряду, под шапкой линия;
   - центральная область (будущее арт/картинка) — как было: рамка портрета, подсветка
     `IsTarget`/`IsCurrent`; плюс зона баффов/дебаффов: для левой команды flow справа налево,
     сверху вниз; для правой — слева направо, сверху вниз (`WrapPanel.FlowDirection` по `IsEnemy`);
   - подвал: класс героя, ниже полоски HP (12 блоков + значение) и стресса (10 пипсов + значение).

## Изменения

1. `src\Core\Sektor.DarkestDungeon.Core.Combat\Character\Generation\HeroGeneration.cs` — словарь
   `CanonicalNames` (class id → имя) для всех 15 классов; случайный пул остаётся fallback для
   неизвестных классов. RNG-потребление (сид) сохранено как раньше для детерминизма.
2. `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelUnitCardView.xaml` — переписана под CCG-карточку
   (см. задачу). `Width=185`, высота по содержимому; буфера/дебаффы — `ItemsControl` на
   `WrapPanel` с `FlowDirection` по `IsEnemy`, биндинг на `StatusEffects` (пусто — данные не
   экспонируются ядром дуэли).
3. `Views\DuelUnitCardView.xaml.cs` — без изменений.
4. `Views\DuelBattleView.xaml` — без изменений (карточка по-прежнему вписывается в поле 290).
5. `ViewModels\DuelUnitViewModel.cs` — добавлены `StatusEffects` (IReadOnlyList<string>, резерв
   под баффы/дебаффы) и `StressText`.
6. Тесты: `tests\Core\Sektor.DarkestDungeon.Core.Combat.Tests\Mechanics\HeroGenerationTests.cs`
   (канонические имена, независимость от сида, уникальность, fallback), `DuelRenderTests.cs` +
   `Snapshot_HeroCardsShowCanonicalClassName`.

## Проверка

- `dotnet build src\Wpf\Sektor.DarkestDungeon.Wpf` — 0 errors (предупреждения ранее существующие).
- `dotnet test` по всем projects: 10/10 suites green (Core.Duel 32, Core.Combat 61, Wpf 22 и др.).
- `tools\check-using-placement.ps1` — OK.
- Причина бага имён подтверждена: `HeroGeneration.GenerateHero` брал имя из общего пула по PRNG-сиду,
  не привязываясь к классу (строки генерились до `SetRandomSeed(sessionSeed)`, поэтому на бой не
  влияли; RNG-потребление при генерации сохранено).

---

# Plan: Duel HUD redesign to Darkest-Dungeon-style layout (WPF duel)

## Цель (проверяема)

1. **Карточка юнита** (`DuelUnitCardView`): контрол номера позиции (ранк) прибит к углу карточки
   (герои — верхний правый, враги — верхний левый, без отступов, чуть крупнее). Центральный
   квадрат «арта» замощён полностью (Image Stretch=Fill, без внутренних отступов). База: HP-бар на
   всю ширину, значение по центру снизу; стресс-бар на всю ширину, значение по центру снизу; имя
   класса снизу, отделённое разделителем. Пипсы стресса серые по умолчанию, у заполненных — белая
   обводка. Баффы/дебаффы слева-направо, сверху-вниз для ВСЕХ команд. В футере у правого края
   (отступ 1) вертикальные полоски действий: белая — персонаж не ходил, серая — походил; несколько
   — если способность даёт несколько ходов.
2. **Нижняя левая панель** (DD-стиль, выше и компактнее): портрет + имя/класс, статы в 2 колонки
   (HP cur/max, Stress cur/max, ACC, CRIT, DMG min-max, DODGE, PROT, SPD) одним контролом с
   границами; справа **контрол снаряжения** (2 ряда: глиф скрещенных мечей + уровень оружия / глиф
   щита + уровень брони; под ними вытянутые прямоугольники под будущие картинки) и **контрол
   тринкетов** (верхний ряд span с глифом мешочка, ниже 2 слота).
3. **Верхний ряд способностей** переделан (по сути в 2 ряда): слева квадрат иконки текущего
   персонажа, правее имя/класс (адаптивно, без растягивания). Далее 4 выбранные способности
   (квадрат только под картинку, имя снизу): доступная — жёлтая рамка (существующие цвета),
   недоступная — серая рамка + серое затенение. 5я MOVE как есть. 6я PASS — вытянутый
   прямоугольник (не квадрат, сдавленный), красная рамка и красный X.
4. **Подсказка снизу-по-центру** при наведении: во всю высоту панели (информация о персонаже +
   способности), не выезжает за границы, адаптивно (ScrollViewer).
5. **Правая панель** (LOG/INVENTORY/MAP): кнопки-глифы вертикальным столбцом справа снизу вверх —
   INVENTORY снизу, MAP выше, LOG выше; текст убран. Контент MAP — единственный квадрат по центру,
   без выезда за границы.
6. **Поле боя**: карточки прибиты к низу, отступ 4.
7. **Torch+Round**: текст «ROUND» убран, цифра раунда — отдельный контрол внутри кружочка, крупным
   жёлтым текстом; верхний ряд: стрелка влево, глиф огня по центру, стрелка вправо (прибито к
   верху) — отдельный контрол (TorchView) + свой VM. Затенение: 0-100, шаг 25 (0 — чёрно-серый,
   выше — желтее/светлее; глиф пламени красится кистью от TorchLevelBrush).
8. **Крестик и квест слева**: кнопка-крестик внутри контрола, прибита к верху и левому краю; сам
   контрол с границами выровнен со всеми остальными (отступ слева уменьшен).

## Изменения (affected files)

1. `Views\DuelUnitCardView.xaml` — ре-макет CCG-карточки (ранк-бейдж к углу, полный тайлинг арта,
   база = HP/стресс/класс, обводка стресс-пипсов, LTR-баффы, полоски действий у правого края).
2. `ViewModels\DuelUnitViewModel.cs` — + `ActionsTotal`, `RemainingActions`, `ActionPips`.
3. `ViewModels\DuelBattleViewModel.cs` — `RefreshTurnOrder` считает `RemainingActions` (уже
   сходившие в раунде = 0), `RefreshActor` отдаёт полные статы, `RefreshEvents` дублирует раунд в
   `Torch.Round`.
4. `ViewModels\TorchViewModel.cs` — + `Round`, + `TorchLevelBrush` (цветовые диапазоны по 25).
5. `Views\TorchView.xaml` — новый макет (стрелки/пламя/кружок раунда).
6. `Views\ScreenHeaderView.xaml` + `DuelBattleView.xaml` (margin) — крестик внутри контрола, выравнивание.
7. `Views\DuelBattleView.xaml` — переделка верхней полосы способностей, убраны кнопки
   LOG/INVENTORY/MAP из неё, поле боя прибито к низу, «ROUND» убран.
8. `Views\RaidHudView.xaml` — правая колонка: вертикальный столбец кнопок-глифов справа.
9. `Views\UnitTooltipView.xaml` — на всю высоту, ScrollViewer, секция SKILLS.
10. `Views\HeroInfoPanelView.xaml` + новые `Views\HeroEquipmentView.xaml`,
    `Views\HeroTrinketsView.xaml` — компактная DD-панель слева.
11. `Views\MapView.xaml` — один квадрат по центру, ClipToBounds.
12. `ViewModels\RaidHudViewModel.cs`, `ViewModels\HeroViewModel.cs` — расширенный `ApplyActor`
    (полные статы для панели).
13. `tests\Wpf\...\DuelRenderTests.cs` — тесты на ActionPips/RemainingActions и статы панели.

## Готово (acceptance)

- [x] `dotnet build src\Wpf\Sektor.DarkestDungeon.Wpf` — 0 errors.
- [x] `dotnet test` — WPF и связанные suites green.
- [x] `tools\check-using-placement.ps1` — OK.
- [ ] Визуальная проверка по `docs\TESTING.md` (дуэльный экран: карточки внизу поля, ранк в углу,
  стресс-пипсы с белой обводкой, полоски действий, torch-кружок раунда, глиф-кнопки справа,
  панель персонажа DD-стиля).

---

## Рев. 2 — правки по фидбеку визуального прохода (WPF-дуэль)

### Цель (правки по фидбеку, ветка `core/duel-unit-card-view`)

1. **Нижняя левая панель статов** — убраны портрет/имя/класс, только статы (+ шмот/тринкеты);
   портрет/имя/класс остаются в полосе способностей сверху того же контейнера.
2. **PASS** — вертикальный (по высоте), а не вытянутый горизонтальный.
3. **Объединённый левый контейнер**: сверху полоса (портрет + имя/класс + способности + MOVE +
   PASS), снизу статы/шмот/тринкеты; по центру-справа от него — всплывающий тултип.
4. **Карточки**: прибиты к низу с небольшим отступом, сохраняют нативные пропорции
   (не сплющиваются); порядок в карточке — имя класса НАД HP/стрессом; полоски HP/стресс/`|`
   вынесены в отдельный UserControl (`UnitStatusBarsView`) под вертикальной карточкой-портретом.
5. **Имена под ранк-бейджем** — по центру, не под бейджем.
6. **Баг «пипсы не середы»** — полоски действий серы после хода персонажа.
7. **Квест-контрол слева** — прибит к верху (без пробела), не на всю ширину, только текст
   квеста (раунд и имя ходящего вынесены).
8. **Имя ходящего** — в торч-контрол: под раундом, цветом команды.
9. **TURN ORDER** — не сворачивается и не прыгает: ходящий не пропадает весь свой ход (белая
   рамка), мёртвые гаснут на месте; тайл — имя сверху, разделитель под будущую иконку, скорость
   внизу-слева и нароленная инициатива внизу-справа «в стиле позиции».

### Корень бага пипсов и прыжков

Ядро `Round` при подготовке хода (`PreHeroTurn`/`PreMonsterTurn`) вынимает юнита из
`OrderedUnits`, поэтому `currentIndex` в старом `RefreshTurnOrder` был −1 → все пипсы = 1 (белые)
и полоса складывалась. Фикс: WPF хранит `_roundStartOrder` (полный порядок раунда, захват при
смене `RoundNumber`, `SelectedUnit` добавляется в начало, если его нет) — `RemainingActions`
считается по позиции в этом списке.

### Изменения

1. `ViewModels\DuelBattleViewModel.cs` — `QuestText` (статичный квест), `_lastRound`/
   `_roundStartOrder`, переписан `RefreshTurnOrder` (полный порядок раунда, `IsCurrent`/`IsDead`),
   `Torch.ActorName`/`ActorColor` + `CreateTeamBrush` (герои #C96A5A, враги #6A8AC9).
2. `ViewModels\DuelTurnEntryViewModel.cs` — + `IsDead`.
3. `ViewModels\TorchViewModel.cs` — + `ActorName`, `ActorColor`.
4. `Views\TorchView.xaml` — имя ходящего под кружком раунда (цвет команды).
5. `Views\DuelUnitCardView.xaml` — контейнер: вертикальная карточка-портрет (имя по центру,
   ранк в углу через Style-Setter'ы, арт замощён, класс снизу над HP/стрессом) +
   `UnitStatusBarsView`.
6. Новые `Views\UnitStatusBarsView.xaml(.cs)` — HP-бар+значение, стресс-бар+значение, `|`-пипсы
   у правого края.
7. `Views\DuelBattleView.xaml` — верх: квест прибит (Title="", только Subtitle-квест); поле —
   Viewbox natural-size (без сплющивания), отступ снизу 12; низ: левый контейнер (полоса +
   `HeroInfoPanelView`), центр — сворачиваемый тултип, справа `RaidHudView`; PASS вертикальный.
8. `Views\RaidHudView.xaml` — right-only панель: контент + вертикальный глиф-столбец.
9. `Views\HeroInfoPanelView.xaml` — убран `HeroBannerView` (портрет/имя/класс).
10. `Views\TurnOrderView.xaml` — новый тайл (имя сверху, зона под иконку, скорость/инициатива
    по углам снизу), `IsCurrent`-белая рамка, `IsDead`-димминг.
11. `tests\Wpf\...\DuelRenderTests.cs` — `Actor_ReflectsCurrentUnit` (квест = "Defeat the rival
    party") + новый `Turn_End_GreysActedUnitPips`.

## Готово (acceptance, рев. 2)

- [x] `dotnet build src\Wpf\Sektor.DarkestDungeon.Wpf` — 0 errors.
- [x] `dotnet test` — WPF 25/25 (+1 новый) и все связанные suites green.
- [x] `tools\check-using-placement.ps1` — OK (прогон после всех правок).
- [ ] Визуальная проверка по `docs\TESTING.md` (ред. 2026-08-31): обновлённые шаги 4.

---

## Рев. 3 — правки по второму фидбеку визуального прохода (WPF-дуэль, ветка `core/agents-branching-rule`)

### Цель (фидбек)

1. **Карточки всё ещё сплющенные** — дамп визуального дерева подтвердил: рендер 185×166 (aspect
   1.11). Корень: карта меряется при неограниченной высоте → `*`-строка портрета схлопывается до
   высоты контента (~54px) → Viewbox Uniform даёт квадрат. Фикс: фиксированная `Height="330"` на
   корневом `Border` `DuelUnitCardView` (Viewbox масштабирует пропорционально, aspect 185:330≈0.56).
2. **MOVE и крестик PASS не в один ряд со способностями** — сейчас они ниже. Причина: скиллы в
   `WrapPanel` переносятся на следующую строку при узком контейнере. Фикс: `ItemsPanel` →
   горизонтальный `StackPanel` (гарантированный один ряд). Подтверждение ряда — регрессионный
   рендер-тест при локальном ходе (скиллы пустые на чужом ходу).
3. **Убрать подсказку при наведении на персонажа** — удаляется центр-колонка `UnitTooltipView`,
   hover-команды и `TooltipTarget`; выровненная разметка (сетка резистов) переносится в ПКМ-лист.
   `UnitTooltipView` используется только в `DuelBattleView.xaml:247` — файлы удаляются.
4. **ПКМ-лист статов — по центру экрана, без прозрачности** — `HorizontalAlignment/VerticalAlignment
   = Center`, солидный фон (#FF0C0A08), размер ~640×560; сетка резистов треугольной выкладкой
   (стили `ResistGrid`).
5. **Нижняя панель делится ровно по центру** — две колонки `*`/`*` (50/50): левая = полоса
   способностей + `HeroInfoPanelView`, правая = `RaidHudView` (в рамке `OverlayPanel`).
6. **TURN ORDER: убрать заголовок** — `TurnOrderView` без текста «TURN ORDER».
7. **Тайл очереди: без инициативы, только Speed** — `InitiativeRoll` убирается из
   `DuelTurnEntryViewModel` и разметки. Раскрыто: «скорость 90» — неверное прочтение локализованного
   броска (запятая-разделитель: «9,0»); реальные скорости 1–8 (ролл ≤14.5, замер временным
   `SpeedProbe`).
8. **Из панели факела и раунда убрать имя и цвет ходящего** — `TorchViewModel.ActorName`/
   `ActorColor` + `DuelBattleViewModel.CreateTeamBrush` удаляются, `TorchView` без строки имени.
9. **Походившие в раунде пропадают из очереди** — `RefreshTurnOrder` берёт `_roundStartOrder`,
   фильтрует index ≥ currentIndex и живых; ходящий держит белую рамку, мёртвые/сходившие исчезают.

### Изменения

1. `ViewModels\DuelBattleViewModel.cs` — `RefreshTurnOrder` (только оставшиеся в раунде и живые,
   `IsCurrent` для ходящего); удалены `HoverCommand`/`UnhoverCommand`/`TooltipTarget`/`Hover`/
   `Unhover`/`CreateTeamBrush` и задание `Torch.ActorName`/`ActorColor`; чистка `using
   System.Windows.Media;` если не нужен.
2. `Views\DuelBattleView.xaml` — низ 2×`*` (левая: полоса + `HeroInfoPanelView`; правая:
   `RaidHudView` в `Border OverlayPanel`); центр-колонка тултипа удалена; скиллы — `StackPanel`
   Horizontal; из `DuelSlotTemplate` убраны MouseEnter/MouseLeave; ПКМ-оверлей по центру, солидный
   фон #FF0C0A08, ~640×560.
3. `Views\DuelUnitCardView.xaml` — корневой `Border` `Width="185" Height="330"`.
4. `Views\TurnOrderView.xaml` + `ViewModels\DuelTurnEntryViewModel.cs` — убраны заголовок,
   `InitiativeRoll`, `IsDead`; внизу тайла только скорость.
5. `Views\TorchView.xaml` + `ViewModels\TorchViewModel.cs` — убраны `ActorName`/`ActorColor`.
6. `Views\HeroStatsView.xaml` + `ViewModels\HeroStatsViewModel.cs` — выровненная сетка резистов
   (8 резистов по колонке) вместо `ResistsText`; новые свойства `Resist*` заполняются в `Apply`.
7. `Views\UnitTooltipView.xaml(.cs)` — удалены (см. п.3 цели).
8. `ViewModels\DuelUnitViewModel.cs` — удалён `IsSelected` (использовался только для hover).
9. `tests\Wpf\...\RenderCaptureTests.cs` — из dev-харнесса в регрессию: карты выше чем шире
   (aspect <0.8), SKILL/MOVEBTN/PASSBTN в одном Y при локальном ходе, turn order = только оставшиеся,
   оверлей статов по центру. `DuelRenderTests` — при необходимости правки под новую очередь.

### Проверка

- [x] `dotnet build src\Wpf\Sektor.DarkestDungeon.Wpf` — 0 errors.
- [x] `dotnet test` — WPF и связанные suites green; `tools\check-using-placement.ps1` — OK.
- [x] Рендер-тест (1600×900, оффскрин + дамп визуального дерева) — ассерты выше зелёные.
- [x] Unity-compile-check не нужен (правок под `unity/` нет).
- [ ] Визуальная проверка по `docs\TESTING.md` — за пользователем.

## Стрелка «актор → цель» по ховеру (WPF-дуэль, ветка `core/agents-branching-rule`)

### Цель

Плоская полоса-стрелка «актор → цель» при наведении на валидную цель: один ряд по центру поля,
без псевдо-объёма; начинается сразу за слотом актора и заканчивается на слоте цели. Длина зависит
от рангов. Полоса выровнена ровно по фактическим карточкам (инверсия рангов учтена; колонки
калибруются по измеренным карточкам один раз при первом layout).

### Решения по фидбеку (подтверждены пользователем)

- **Инверсия рангов**: визуальный порядок слева-направо `0..7 = 4,3,2,1 | 1,2,3,4`. Слот героя
  rank r = `4 - r` (rank1 у центра = слот 3), слот монстра rank r = `3 + r` (rank1 = слот 4).
  Пример: «1 команда pos1 наводится на 2 команда pos1» = слот 3 → слот 4, короткий сегмент `{4}`
  (раньше из-за инверсии тянулось на всю ширину).
- **Один ряд**: псевдо-объём (4 ряда ячеек с нарастанием) убран. Полоса = 1 ряд фикс. высоты
  ~22px в координатах Viewbox, по вертикали по центру поля.
- **Точное выравнивание**: колонки полосы один раз при `LayoutUpdated` калибруются по измеренным
  карточкам (сортировка по X; col0 = X первой карточки, каждая слот-колонка = шаг до следующей
  карточки → полоса непрерывна по центрам слотов; Viewbox масштабирует всё равномерно, дальше
  только `Visibility`, без повторных измерений).
- **Предрасчёт**: 32 маски (команда × позиция актора × позиция цели) считаются один раз в
  static-конструкторе `DuelArrowCells` в таблицу `int[][][][] Tables` (team → source-1 → target-1
  → `int[]` индексов слотов 0..7); на ховер — только индексация.
- **Появление**: только валидные цели — локальный ход и `target.IsTarget == true` (валидность уже
  вычислена в `SelectSkill`/`SelectMove`) и цель ≠ текущий актор. Источник = `controller.CurrentUnit`.

### Изменения

1. `Ui\DuelArrowCells.cs` (переписан под один ряд): `CellCount = 8`, `SlotFor(Team, rank)` →
   0..7 (инверсия в формуле), `MaskFor(sourceTeam, sourceRank, targetRank)` → `IReadOnlyList<int>`
   из предрассчитанной таблицы: lit = `[srcSlot+1 .. targetSlot]`, правая команда зеркально
   `[targetSlot .. srcSlot-1]`. Убраны `RowsPerColumn`/`Index`.
2. `Views\DuelBattleView.xaml`: `ArrowGrid` = полоса: `Height="22"`, `VerticalAlignment="Center"`,
   одна `RowDefinition` (`1*`), колонки `[0]=lead(по умолч. *), 1..8=слоты, 9=trail(*)`, 8
   фикс. `Rectangle` (стиль `ArrowCell` — плоский, без taper) в `Grid.Column=1..8`;
   `IsHitTestVisible=False`, `Visibility=Collapsed`. Ховер-хендлеры на `DuelSlotTemplate` как есть.
3. `ViewModels\DuelBattleViewModel.cs`: `CurrentActorTeam`, `CurrentActorRank` и
   `CanShowArrow(DuelUnitViewModel)` — уже есть, без изменений.
4. `Views\DuelBattleView.xaml.cs`: `EnsureCells()` один раз при первом `LayoutUpdated` собирает 8
   `Rectangle` (`ArrowGrid.Children`) и калибрует колонки по измеренным карточкам (`DuelUnitCardView`,
   сортировка по X); `ShowArrowFor(target)` → `MaskFor(CurrentActorTeam, CurrentActorRank,
   target.Rank)` → `Visibility` слотов; `ClearArrow()` гасит всё; конструктор за маркап-компилятором,
   ячейки лениво.
5. Тесты: `DuelArrowCellsTests` — инверсия (`SlotFor`), примеры масок (герой1→монстр1=`{4}`,
   герой4→монстр4=`{1..7}`, монстр1→герой1=`{3}`, монстр4→герой4=`{0..6}`), все 32 маски
   непустые/валидные/уникальные; `RenderCaptureTests.DuelArrow_HoverShowsBandAndClears` —
   после layout колонки полосы настроены, `ShowArrowFor` подсвечивает ровно слоты маски,
   `ClearArrow` → всё Collapsed, `IsHitTestVisible == False`.
6. Документы (тот же коммит): `TESTING.md` строка «Duel hover arrow», `CHANGELOG.md`, `PLAN.md`.

### Проверка

- [x] `dotnet build src\Wpf\Sektor.DarkestDungeon.Wpf` — 0 errors (nullable-warning'и `InMemoryTransport`/`DuelContent` предсуществующие, не трогаем).
- [x] `dotnet test` — WPF и связанные suites green; `tools\check-using-placement.ps1` — OK.
- [x] Рендер-тест (1600×900, оффскрин) — прежние ассерты + новый ховер-тест полосы.
- [x] Unity-compile-check не нужен (правок под `unity/` нет).
- [ ] Визуальная проверка по `docs\TESTING.md` — за пользователем.

---

## Задача: WPF-бой — механики целиком на ядре + стрелка «бейдж скилла → цель» (ветка `core/agents-branching-rule`)

### Цель

Механики боя (баффы/дебаффы, pull/push, АОЕ) в ядре есть и работают (`BattleSolver` в `Core.Combat`),
но WPF-дуэль их не дотягивает: (1) `DuelController.ExecuteSkill` исполняет `Solver.ExecuteSkill` по
**одной** цели (в отличие от Unity `ExecuteSkillBase`, который итерирует `targetInfo.Targets`) → АОЕ
и партийные хилы/баффы бьют только по кликнутой цели; (2) `ExecuteLocalSkill` не валидирует цель →
атакующей способностью можно кликнуть себя/союзника; (3) полоска статусов на карточке пустая →
баффы/дебаффы не видны. Плюс заменить «полосу из прямоугольников» стрелки цели на: иконку
выбранной способности **над карточкой действующего героя** + прямую линию от неё до наведённой цели
со стрелкой на конце (чистая математика, ничего сложного). Фикс в `DuelController` чинит и AI
(`DuelAi`/`FightSession` — оба брали только `Targets[0]`). Legacy Unity не трогаем.

### Фаза 1 — Core: валидация цели + мультитаргет (`Core.Duel\DuelController.cs`)

1. [x] `ExecuteLocalSkill`: цель должна быть в `GetAvailableTargets(unit, skill)`, иначе `null`
     (клик игнорируется, выбор скилла сохраняется, ход не заканчивается).
2. [x] `ExecuteSkill(unit, primaryTarget, skill)`: раскрыть `Solver.SelectSkillTargets(unit,
     primaryTarget, skill)` → цикл `Solver.ExecuteSkill` по каждой цели; затем `ProcessEventQueues`,
     `CheckDeaths`, `ExecuteRiposte` по каждой цели, `RemoveConditions` (перформер + все цели),
     `RecoverDeathsDoorIfHealed`. Self-move внутри скилла клампится (паритет Unity). Бонус: корень
     «pull/push не работают» — `ProcessEventQueues` теперь итерирует снапшот `Units` (иначе
     `MoveUnit` ронял «коллекция изменена» посреди перечисления).

### Фаза 2 — Core: доступ к активным баффам (`Core.Combat\Character\Character.cs`)

3. [x] Публичный `IReadOnlyList<BuffInfo> BuffInfos` (интерфейс `ICharacter` не меняется).

### Фаза 3 — WPF ViewModel (`DuelBattleViewModel.cs`, `DuelUnitViewModel.cs`)

4. [x] `SelectTarget`: guard — только `IsTarget` (скилл) / смежность ранга (move) перед вызовом
     контроллера.
5. [x] `ToUnit`: заполнять `StatusEffects` (id + остаток длительности) из `BuffInfos`;
     `DuelUnitViewModel.StatusEffects` → сеттируемая.
6. [x] `SelectedSkill` (DuelSkillViewModel?) для бейджа стрелки (+ `IsLocalTurn` на VM).

### Фаза 4 — WPF: стрелка (новый `Ui\TargetArrowMath.cs`, `DuelBattleView.xaml(.cs)`)

7. [x] Новый `Ui\TargetArrowMath.cs` — чистые функции: точки линии + `ArrowHead(end, start, length,
     spread)` → 3 точки треугольника (тестируемо).
8. [x] `DuelBattleView.xaml`: удалить `ArrowGrid`/`ArrowCell`; внутри Viewbox-грида верхняя строка
     (`54`) под бейдж + `Canvas x:Name="TargetLayer"` (Grid.RowSpan/ColumnSpan=2,
     Panel.ZIndex=10, IsHitTestVisible=False) с `SkillBadge` + `ArrowLine` + `ArrowHead`.
9. [x] `DuelBattleView.xaml.cs`: бейдж — над карточкой действующего юнита (TransformToVisual в
     координаты Canvas, top-center), линия из центра бейджа в центр карточки-цели, стрелка через
     `TargetArrowMath`; `ClearArrow` гасит линию/стрелку (бейдж остаётся, пока скилл выбран);
     позиционирование бейджа на `LayoutUpdated`; move-режим — линия без бейджа.
10. [x] Удалить `Ui\DuelArrowCells.cs` + `tests\Wpf\...\DuelArrowCellsTests.cs`.

### Фаза 5 — Тесты

11. [x] Core `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests\DuelSkillExecutionTests.cs`: самоклик/
      алли-клик атакой отклонён (crusader `smite` → null, HP не меняется); hellion `breakthru` бьёт
      всех врагов в рангах 1–3; vestal `gods_comfort` хиляет всю партию (4 Heal-записи);
      vestal `divine_grace` (одиночный хил) лечит **только кликнутого** раненого аллея (другой
      раненый без изменений); PD `emboldening_vapours` вешает бафф (BuffInfos не пуст);
      occultist `daemons_pull` меняет ранг цели; lockstep `TurnFlow_BothSides_RemainInLockstep`
      остаётся зелёным.
12. [x] WPF `tests\Wpf\...\DuelRenderTests.cs`: `SelectTarget` по невалидной цели ничего не исполняет
      (log/HP без изменений); `HealSkill_SelectsOnlyAlliesAndHealsTheClickedAlly` — после выбора
      хил-скилла `IsTarget` только у союзников (враги нет), клик по врагу игнорируется, клик по
      раненому союзнику лечит его; новый `TargetArrowMathTests` (направление/длина/точки);
      `RenderCaptureTests` переписан на бейдж+линию+стрелку.

### Фаза 6 — Доки (тот же коммит)

13. [x] `BATTLE_PARITY.md` (§0 цепочка скилла + §5 — мультитаргет-исполнение в дуэли);
      `docs/mechanics/combat/01_damage.md` (мультитаргет-цикл), `09_buffs.md` (партийные баффы),
      `07_rank_move.md` (pull/push в дуэли); `docs/mechanics/presentation/` (стрелка);
      `TESTING.md` (ручные проверки); `CHANGELOG.md` (версия); `PLAN.md` шаги `[x]`.

### Проверка

14. [x] `dotnet test Darkest-Dungeon-Unity.slnx` — все 9 сьютов зелёные (Duel 39, Combat 61,
      Wpf 32 и др.); `tools\check-using-placement.ps1` — OK.
      Правки только `src\`/`tests\`/`docs\` → `unity-compile-check` не требуется.

### Затронутые файлы

- Core: `src\Core\Sektor.DarkestDungeon.Core.Duel\DuelController.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Combat\Character\Character.cs`.
- WPF: `ViewModels\DuelBattleViewModel.cs`, `ViewModels\DuelUnitViewModel.cs`,
  `Views\DuelBattleView.xaml(.cs)`, новый `Ui\TargetArrowMath.cs`, удалить `Ui\DuelArrowCells.cs`.
- Тесты: новый `DuelSkillExecutionTests.cs`, `TargetArrowMathTests.cs`; правки `DuelRenderTests.cs`;
  удалить `DuelArrowCellsTests.cs`.
- Доки: `BATTLE_PARITY.md`, `docs/mechanics/combat/{01_damage,07_rank_move,09_buffs}.md`,
  `docs/mechanics/presentation/`, `TESTING.md`, `CHANGELOG.md`, `PLAN.md`.

---

# Plan: Buff/debuff table popup on character cards (WPF duel)

## Цель (проверяема)

[x] 1. На карточке персонажа `DuelUnitCardView` в левом нижнем углу центральной (портретной) области
   добавить кнопку `i`. По нажатию — переключаемый `Popup` с таблицей баффов/дебаффов юнита:
   слева название, далее время действия/заряды, далее описание.
[x] 2. Убрать текущую полоску баффов/дебаффов (`StatusEffects`) с карточки.
[x] 3. Текстовые производные (название/описание/длительность) вынести в переиспользуемый хелпер,
   чтобы будущие raid-карты могли им пользоваться.

## Изменения

[x] 1. Новый `ViewModels\BuffRowViewModel.cs` — неизменяемая модель строки таблицы (constructor DI,
   get-only): `Name`, `DurationText`, `Description`, `Tone` ("Buff"/"Debuff").
[x] 2. Новый `Ui\BuffDetails.cs` — статический хелпер (переиспользуемый):
   `FormatName(Buff)` (id → заголовок, fallback на AttributeType), `FormatDescription(Buff)`
   (AttributeType + ModifierValue, знак по `IsPositive`), `FormatDuration(BuffInfo)`
   (`BuffDurationType` + `Duration` → "x2 rounds"/"Combat"/"Permanent").
[x] 3. `ViewModels\DuelUnitViewModel.cs` — заменить `StatusEffects` (List<string>) на
   `Buffs` (List<BuffRowViewModel>) и добавить `IsBuffPopupOpen` (bool).
[x] 4. `ViewModels\DuelBattleViewModel.cs` — `BuildStatusEffects` → строит `List<BuffRowViewModel>`
   из `character.BuffInfos` через `BuffDetails`.
[x] 5. `Views\DuelUnitCardView.xaml` — удалить `StatusEffects` ItemsControl; добавить `ToggleButton`
   `i` (низ-лево центральной области, TwoWay к `IsBuffPopupOpen`, изоляция клика `e.Handled`);
   добавить `Popup` с таблицей (3 колонки: Name / Duration / Description, заголовок + строки).
[x] 6. Новый `tests\Wpf\...\BuffDetailsTests.cs` — юнит-тесты `FormatName`/`FormatDescription`/
   `FormatDuration`.
[x] 7. `docs\TESTING.md` — пункт ручной проверки (открыть дуэль, нажать `i`, проверить таблицу и
   что полоска с карточки ушла; popup закрывается).

## Проверка

[x] 8. `dotnet build` + `dotnet test` (WPF-сьют и соседние зелёные); `check-using-placement` — OK.
   Правки только `src\Wpf\`/`tests\Wpf\`/`docs\` → `unity-compile-check` не требуется.

---

# Plan: Centered 6-column buff/debuff popup + reusable UnitHeaderView + skill tooltip debuffs (WPF duel)

## Цель (проверяема)

[x] 1. Окно баффов/дебаффов — по центру, как окно информации о персонаже; **6 колонок**:
   первые 3 — баффы (Название | Время/заряды | Описание), последние 3 — дебаффы; сверху
   позиция слева, имя с цветом команды и разделительная линия; шрифты как в подсказке персонажа.
[x] 2. Карточка персонажа: обе команды к одному виду — **позиция слева, далее имя с цветом**;
   переиспользуемый юзер-контрол `UnitHeaderView`.
[x] 3. Подсказка персонажа (стат-лист): в верхнем ряду позиция + имя с цветом, кнопка закрытия справа.
[x] 4. Тулы скиллов показывают, какой бафф/дебафф накладывает способность.

## Изменения

[x] 1. Новый `Views\UnitHeaderView.xaml(.cs)` — переиспользуемый хедер (DependencyProperties):
   `Rank`, `Name`, `ClassName`, `IsEnemy`, `CloseCommand`. Слева бейдж ранга, имя 16 bold
   (красный/синий по `IsEnemy`), класс 14, справа кнопка закрытия (если задана команда), тёмный фон
   + разделительная линия снизу.
[x] 2. `Views\DuelUnitCardView.xaml` — хедер заменён на `UnitHeaderView` (Rank/Name/IsEnemy);
   угловой бейдж ранга убран; кнопка `i` → обычная Button на `ToggleBuffTableCommand` (RelativeSource
   к DuelBattleView), клик изолируется `e.Handled`; старый Popup удалён.
[x] 3. `ViewModels\DuelUnitViewModel.cs` — убрать `IsBuffPopupOpen`; `Buffs` разделены на
   `Buffs` (положительные) и `Debuffs` (отрицательные).
[x] 4. `ViewModels\DuelBattleViewModel.cs` — `BuildBuffs` разбивает по `buff.IsPositive()`;
   `BuffTarget`, `IsBuffTableVisible`, `ToggleBuffTableCommand`, `CloseBuffTableCommand`;
   `RefreshUnits` переразрешает `BuffTarget` по `CombatId` (попап живёт между снапшотами).
[x] 5. `Views\DuelBattleView.xaml` — центрированный оверлей `BuffTableOverlay` (~860x500, непрозрачный):
   хедер `UnitHeaderView` (BuffTarget) + таблица 6 колонок (BUFFS cols 0-2, DEBUFFS cols 3-5,
   под-заголовки Name/Duration/Effect, шрифт 14, описание зелёное/красное по тону);
   стат-оверлей `StatsOverlay` (x:Name сохранён): Row 0 = `UnitHeaderView` (StatsTarget +
   `CloseStatsCommand`), отдельная кнопка закрытия убрана, `HeroStatsView ShowHeader=False`.
[x] 6. `ViewModels\HeroStatsViewModel.cs` + `Views\HeroStatsView.xaml` — добавлены `Rank`, `IsEnemy`
   (заполняются в `Apply(DuelUnitViewModel)`); `ShowHeader` bool DP (default true) прячет внутренний
   хедер, когда его предоставляет хост. BattleScreenView/HeroSlotsPanel не меняются.
[x] 7. Core (маленькие геттеры, без изменения поведения): `BleedEffect.DotAmount`,
   `PoisonEffect.DotAmount`, `StressEffect.StressAmount`, `PullEffect.PullParam`,
   `PushEffect.PushParam` — для тултипа с количеством.
[x] 8. `Ui\SkillDetails.cs` — секция эффектов: per `skill.Effects[].SubEffects` строка
   ("Stun", "Mark", "Immobilize", "Bleed 3 (2 rounds)", "Blight 4 (3 rounds)", "Stress +15",
   "Pull 1", "Riposte", "Cure", "Shuffle", "Guard", removers); для стат/контент-баффов —
   "Buff:"/"Debuff:" + `BuffDetails.FormatDescription` (BuffIds через `BuffCatalog`);
   аннотации `(self)`/`(party)` по `Effect.TargetType`.
[x] 9. Новый `tests\Wpf\...\SkillDetailsTests.cs` — тултип-строки (стан/кровь/стат-бафф/контент-дебафф).
[x] 10. `docs\TESTING.md` — строка про центрированную 6-колоночную таблицу, хедер карточки,
    дебаффы в тултипах скиллов; `PLAN.md` шаги `[x]`.

## Проверка

[x] 11. `dotnet build` + `dotnet test` (WPF-сьют и соседние зелёные); `check-using-placement` — OK.
    Правки `src\Wpf\`, `src\Core\` (только аддитивные геттеры), `tests\Wpf\`, `docs\` →
    `unity-compile-check` не требуется.

---

# Plan: Debuff/status visibility + informative log + AI pacing + card flashes + skill badge (WPF duel)

## Цель (проверяема)

1. Статусы (кровь/яд/стан/метка/рипост/guard) видны в таблице баффов/дебаффов — дебаффы реально
   накладываются и отображаются.
2. Лог боя информативный: бафф/дебафф применён или резист, DoT/стан/метка/рипост/guard.
3. Выбранная способность сверху — квадратик скилла с текстом и тултипом.
4. Ход ИИ с паузами на стороне UI: выбор скилла → выбор цели → ~2 с задержка → действие.
5. Вспышка карточки: красная (урон, 1.5 с), синяя (бафф), зелёная (хил).

## Изменения

1. [x] Core `Buff.Describe()` — краткое описание модификатора для лога.
2. [x] Core `DuelBattleEvents`: событие `PopupShown` + читаемые строки лога (`[effect] <имя> <фраза>`);
   эффекты передают значения (Bleed/Poison/Tag/CombatStatBuff/BuffEffect).
3. [x] WPF `DuelUnitViewModel`: `CardFlash` ("Damage"/"Heal"/"Buff"); карточка — оверлей-тинт с
   анимацией 1.5 с (DataTrigger).
4. [x] WPF `DuelBattleViewModel`: статусы в таблице (AppendStatusRows); очереди вспышек
   (урон/хил из SkillResult + Buff/Debuff из PopupShown); `AiSkillPreview`.
5. [x] `IDuelRivalLink` + `NetworkRivalLink` + `AiRivalLink` (фазовый пайсинг: Planning →
   SkillPreviewed → TargetPreviewed → задержка → действие).
6. [x] `DuelBattleView`: бейдж-квадратик скилла (текст + тултип), показ preview ИИ, реакция на
   PropertyChanged (AiSkillPreview/SelectedSkill).
7. [x] Тесты: `StatusTableTests` (кровь/стан/бафф в таблице), правки фейковых rival-link.
8. [x] `docs\TESTING.md` — статусы в таблице, лог, вспышки, пайсинг ИИ, бейдж скилла.

## Проверка

9. [x] `dotnet build` + все сьюты зелёные (WPF 50); `check-using-placement` — OK. Правки `src\`/
   `tests\Wpf\`/`docs\` → `unity-compile-check` не требуется.

---

# Plan: New elbow target arrow (skill mode) with tone colors (WPF duel)

## Цель (проверяема)

1. Новая стрелка цели для способностей (старую оставляем в move-режиме): из центра верхней грани
   карточки ходящего вверх к бейджу способности, из его левой/правой грани горизонтально, затем
   вниз со стрелкой-указателем в верхнюю грань цели.
2. Мультитаргет: для каждой валидной цели (2-4) — своя линия.
3. Цвет линии/стрелки по типу скилла: красный — атака, синий — бафф, зелёный — хил
   (классификация вынесена в `Ui/SkillTone` + `Ui/SkillToneClassifier`).

## Изменения

1. [x] `Ui\SkillTone.cs` + `Ui\SkillToneClassifier.cs` — классификация скилла (Attack/Heal/Buff) и
   кисти стрелки (красный/зелёный/синий).
2. [x] `DuelBattleView.xaml` — 4 слота локтевых стрелок (`SkillArrow1..4` + `SkillArrowHead1..4`);
   старая `ArrowLine`/`ArrowHead` оставлены для move-режима.
3. [x] `DuelBattleView.xaml.cs` — `DrawSkillArrows` (Path-геометрия из 4 сегментов на цель,
   стрелка-указатель в верхнюю грань), `DrawMoveArrow` (старая прямая), скрытие слотов.
4. [x] `DuelBattleView.cs` — массивы слотов стрелок.
5. [x] `DuelBattleViewModel` — `IsMoveMode`, `SelectedSkillTone`.
6. [x] `RenderCaptureTests` — проверка новой локтевой стрелки (4 сегмента, цвет по тону,
   число стрелок = числу валидных целей).
7. [x] `docs\TESTING.md` — обновлена строка hover arrow.

## Проверка

8. [x] `dotnet build` + WPF-сьют зелёный (50); `check-using-placement` — OK. Правки `src\Wpf\`,
   `tests\Wpf\`, `docs\` → `unity-compile-check` не требуется.

---

# Plan: Rework target arrow (top spine, AOE vs single), AI-turn consistency, slower sequential popups

## Цель (проверяема)

1. Стрелка цели: бейдж выше, над линиями; «спина»-линия вверху (спуск от бейджа к горизонтали,
   влево/вправо, вниз стрелкой в верхнюю грань цели); линия не исходит из карточки ходящего.
2. АОЕ/партийные способности — стрелка в каждую валидную цель; одиночные — только в наведённую.
3. Ход ИИ: в нижней левой панели видны способности соперника (не только MOVE/PASS); стрелка цели
   рисуется и для ИИ (его выбранный скилл и цель).
4. Попапы: медленнее (~2 с), поднимаются из центра к верхней грани; при одной атаке тексты по
   очереди (урон → BLEED/BUFF/DEBUFF и т.д.).

## Изменения

1. [x] `DuelSkillViewModel.Tone`; `DuelBattleViewModel.RefreshSkills` — скиллы текущего юнита на
   любом ходу (IsUsable только на локальном); `SelectedSkillIsMultiTarget` (по TargetRanks).
2. [x] `DuelBattleView`: бейдж поднят (`BadgeLift`), новая геометрия `DrawElbowArrows` (спина +
   спуск к цели, 3 сегмента), `DrawSkillArrows` — AOE→все/одиночная→наведённая, `RedrawAiArrow`
   для хода ИИ (AiTargetPreview).
3. [x] `DuelBattleViewModel`: `AiTargetPreview`, очередь попапов `popupQueues`, приоритеты
   (урон/хил → эффекты), `EffectPopupLabel` (BLEED/BLIGHT/STUN/MARK/BUFF/DEBUFF/RIPOSTE/GUARD/STRESS),
   таймер 2.4 с → `AdvancePopups`.
4. [x] `DuelUnitCardView.xaml` — анимация попапа 2.2 с, подъём из центра к верхней грани.
5. [x] `RenderCaptureTests` — 3 сегмента, цвет по тону, число стрелок (AOE→все, иначе 1).
6. [x] `docs\TESTING.md` — стрелка, скиллы/стрелка ИИ, попапы.

## Проверка

7. [x] `dotnet build` + WPF-сьют (50) зелёный; `check-using-placement` — OK. Правки `src\Wpf\`,
   `tests\Wpf\`, `docs\` → `unity-compile-check` не требуется.

---

# Plan: Fix Abomination transform crash + structured skill tooltip + arrow from badge side + AI-arrow hover guard

## Цель (проверяема)

1. Transform Абоминации не падает с `KeyNotFoundException 'human'`; эффекты скиллов реально
   применяются в WPF-дуэли.
2. Тултип способности — крупный контрол над кнопкой: бейдж уровня + имя, разделитель, инфа,
   таблица баффов/дебаффов.
3. Стрелка цели: горизонтальная линия выходит из вертикального центра бейджа (левая/правая грань),
   затем вниз в верхнюю грань цели.
4. Наведение в ход соперника не стирает стрелку ИИ.

## Изменения

1. [x] `DuelClasses` — порядок статической инициализации (`Effects` раньше `Catalog`): парсер
   получал `null`-каталог эффектов → `ModeEffects`/эффекты скиллов не заполнялись (корень краша и
   «неналожения» эффектов). `BattleSolver.ApplyEffects` — `ModeEffects.TryGetValue` (защита).
2. [x] `SkillDetails` — `BuildBaseInfo`/`BuildEffectRows` (+ `SkillEffectRowViewModel`);
   `Build` сохранён. `DuelSkillViewModel` — `Level`, `BaseInfo`, `EffectRows`.
3. [x] Новый `Views\SkillTooltipView`; кнопки скиллов — `ToolTipService.Placement="Top"` +
   тултип-контрол; бейдж скилла — тот же тултип.
4. [x] `DrawElbowArrows` — 2 сегмента: из боковой грани бейджа на уровне его вертикального центра
   горизонтально, затем вниз в цель (без «спины»/низа). `ShowArrowFor`/`ClearArrow` — no-op на
   нелокальном ходу (не трогают стрелку ИИ).
5. [x] Тесты: `AbominationTransformTests` (transform без краша, режим human→beast), `SkillDetailsTests`
   (rows/base info), `RenderCaptureTests` (2 сегмента). `docs\TESTING.md` обновлён.

## Проверка

6. [x] `dotnet build` + все сьюты (WPF 54); `check-using-placement` — OK. Правки `src\Core\` (защита),
    `src\Wpf\`, `tests\Wpf\`, `docs\` → `unity-compile-check` не требуется.

---

# Plan: Тринкеты/экипировка в WPF-дуэли (P1.4)

## Цель (проверяема)

Тринкеты в дуэли: 2 слота на героя, парсинг `JsonTrinkets`/`JsonBuffs`, баффы тринкетов влияют на
статы (permanent, `BuffSourceType.Trinket`), выбор в лобби, показ в боевом HUD. Оркестрация — в ядре
(как `ApplyQuirks`), WPF — тонкий выбор слотов. Оружие/броня остаются «Lv. 1» (нет имения в дуэли).

## Фаза T1 — Core: тринкеты на `Hero` + контент

1. [x] `Hero`: `Trinkets` (List<string>, как `Quirks`) + `AddTrinket(string)`; `EquippedTrinketIds`.
2. [x] `IDuelContent.GetTrinket(string)` → `Trinket`; `DuelController.ApplyTrinkets(hero, trinketIds)`
      (resolve `content.GetTrinket` → `GetBuff` → `AddBuff(Permanent, Trinket)` + refresh HP).

## Фаза T2 — Core: wire-пики

3. [x] `DuelHeroPick` + `HeroFightUnitSpec`: необязательный `TrinketIds`; `AddHero`/`AddPlayerUnit`
      применяют тринкеты.

## Фаза T3 — WPF контент + сеть

4. [x] `DuelContent`: `TrinketCatalog` из `JsonTrinkets.json` (link в csproj) + `GetTrinket`;
      `TextFightContent` + `TestDuelContent` — `GetTrinket`.
5. [x] `DuelPartyConfig`: `TrinketIds` per slot, сериализация `|`-полем №5 (обратная совместимость);
      `DuelLobbyViewModel`/`SinglePlayerLobbyViewModel` `ToPicks` передают тринкеты.

## Фаза T4 — WPF лобби

6. [x] `HeroSlotViewModel`: `TrinketSlots` (2 × `LobbyTrinketViewModel`), фильтр по
      `HeroClassRequirements` (пусто = любой класс), cycle + reroll; `SelectedTrinketIds`.
7. [x] `HeroSlotsPanel.xaml`: 2 слота тринкетов (стрелки + имя + тултип) + reroll.

## Фаза T5 — WPF боевой HUD

8. [x] `HeroStatsViewModel` + `HeroViewModel`/`RaidHudViewModel.ApplyActor` — `Trinket1Text`/
      `Trinket2Text` из `Hero.Trinkets`; `HeroInfoPanelView`/`HeroTrinketsView` биндятся.

## Фаза T6 — Тесты

9. [x] Core: `DuelTrinketTests` — бафф тринкета меняет стат (TRINKET_ACC_B1 → ACC +4), id на герое,
      неизвестный id игнорируется, lockstep не ломается. WPF: `LobbySlotTests` — слоты тринкетов,
      фильтр класса, `SelectedTrinketIds`; `DuelPartyConfig` round-trip + старая строка (4 поля).

## Фаза T7 — Доки

10. [x] `docs/mechanics/combat/09_buffs.md` (тринкеты = permanent source), `TESTING.md`,
      `CHANGELOG.md`, `PLAN.md` шаги `[x]`.

## Проверка

11. [x] `dotnet test Darkest-Dungeon-Unity.slnx` все сьюты зелёные; `check-using-placement` — OK;
      правки `src\`/`tests\`/`docs\` → `unity-compile-check` не требуется.

## Затронутые файлы

- Core: `Hero.cs`, `IDuelContent.cs`, `DuelController.cs`, `DuelHeroPick.cs`,
  `Fight\HeroFightUnitSpec.cs`, `Fight\TextFightContent.cs`.
- WPF: `Data\DuelContent.cs`, `Combat\DuelClasses.cs` (не трогаем), `Networking\DuelPartyConfig.cs`,
  `ViewModels\HeroSlotViewModel.cs`, новый `LobbyTrinketViewModel.cs`, `ViewModels\HeroStatsViewModel.cs`,
  `ViewModels\HeroViewModel.cs`, `ViewModels\RaidHudViewModel.cs`, `ViewModels\DuelBattleViewModel.cs`
  (TrinketsText), `Views\HeroSlotsPanel.xaml`, `Views\HeroTrinketsView.xaml`, `Wpf.csproj` (link).
- Тесты: `DuelTrinketTests.cs`, правки `LobbySlotTests.cs`, `TestDuelContent.cs`; доки.

# Plan: Фикс биндинга DataContext тултипа у скиллов (SkillTooltipView)

## Цель (проверяема)

Убрать 22 binding-ошибки `Cannot find element that provides DataContext` для `SkillTooltipView`.
Контент тултипа (`Button.ToolTip`) не находится в дереве с DataContext до открытия, поэтому
`DataContext="{Binding}"` (no-path) вычисляется в откреплённом состоянии и падает на каждой кнопке-скилле.
Контент тултипа при этом работает — ошибки это диагностический шум; поведение не меняется.

## Шаги

1. [x] `Views\DuelBattleView.xaml:271` — тултип кнопки-скилла заменён на явный `<ToolTip>` с
      `DataContext="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"` и
      `<views:SkillTooltipView />` без биндинга. Self-источник всегда резолвится, `PlacementTarget`
      в закрытом состоянии null (резолвится молча), при открытии ToolTipService ставит
      `PlacementTarget` = кнопка → DataContext = `DuelSkillViewModel` → контент наследует.
      (Первый вариант — `AncestorType={x:Type ToolTip}` на вьюхе — давал свой лог `Cannot find
      source: FindAncestor ... ToolTip`, т.к. FindAncestor вычисляется на откреплённом контенте.)
2. [x] `dotnet build` WPF-проекта: 0 ошибок; WPF-тесты зелёные (включая новый
      `SkillTooltip_ResolvesDataContext_FromPlacementTarget` в `RenderCaptureTests`, который
      симулирует open-time `PlacementTarget` и проверяет DataContext тултипа и контента).
3. [x] Доки не меняются (поведение не изменилось; `TESTING.md:242` уже покрывает ручную проверку);
      правки только в `src\Wpf\`/`tests\Wpf\` → `unity-compile-check` не требуется.

## Критерии приёмки

- В отладке/trace боёвого экрана нет `Cannot find element that provides DataContext` на `SkillTooltipView`.
- Тултип над кнопкой способности открывается, как раньше (бейдж уровня, имя, `BaseInfo`, таблица эффектов).

## Затронутые файлы

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelBattleView.xaml` (1 строка).
