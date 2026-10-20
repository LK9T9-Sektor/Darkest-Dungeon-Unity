# SPEC_GAME_EVENTS_SCHEMA.md — схема записываемых игровых событий (v1.2.0)

> Домен: `GameEvents` (`src\Core\Sektor.DarkestDungeon.Core.GameEvents`) +
> `Clients.EventLog` (`src\Clients\Sektor.DarkestDungeon.Clients.EventLog`).
> Источник истины — DTO в C#; этот файл их описание для читателя JSONL (требование §6 спеки
> `docs\SPEC_GAME_EVENTS.md`). Примеры строк ниже — из **реального вывода** тестов
> (`JsonEventLoggerTests`, `GameEventBusTests`, `GameEventLogReaderTests`) и эталонного прогона
> (`docs/spec_game_events/examples/`); сокращения (`…`) — для читаемости.
>
> **Версии:** текущая схема — `1.2.0` (`JsonEventLogger.CurrentSchemaVersion`). Reader принимает
> `1.0.0` (эталон `docs/spec_game_events/examples/`, 38 событий), `1.1.0` и `1.2.0`. В `1.0.0`
> нет `CombatantState.Rank`/`Size`/`Stress`/`Statuses`/`Buffs`, `StateSnapshot.Battle` и новых
> боевых/аудио событий; WPF для старых записей берёт `fallbackRank = index + 1`. В `1.2.0`
> добавлены estate-события и `manifest.Kind` (`raid`/`estate`); старый manifest без `Kind`
> считается рейдом.

## 1. Формат файлов

Run-директория записи (её создаёт рекордер S3). В редакторе Unity и в игре, запущенной
внутри репозитория, это общая папка `<repo>\captures\run_<timestamp>\`; если корень репозитория
не найден (собранная игра), используется fallback `persistentDataPath\Replays\run_<timestamp>\`.
WPF при открытии боевого реплея берёт свежий `run_*` с `Kind = raid`, estate-реплея — с
`Kind = estate`; при отсутствии записей battle replay играет встроенный эталон
`Content\Captures\Reference`. Папка `captures` в `.gitignore`.

```
<run>/
  manifest.json            # см. ниже
  combat_session.jsonl     # по одному файлу на Subsystem
  map_session.jsonl
  torch_session.jsonl
  audio_session.jsonl      # v1.1: реально проигранное аудио
  estate_session.jsonl     # v1.2: estate snapshot/actions (только estate-run)
  ...
```

`Kind` в manifest отделяет рейдовые run от estate-run; WPF выбирает свежий run нужного kind.

- **JSONL**: UTF-8, одна строка = одно событие, запись с `AutoFlush` (краш теряет максимум одну
  строку; manifest может не дописаться). Порядок строк = порядок `Publish` (возрастающий
  `Sequence`).
- **manifest.json** (пишется сразу и переписывается при появлении каждого файла и на Dispose):

```json
{
  "SchemaVersion": "1.2.0",
  "StartedAt": "2026-10-19T12:00:00.1234567Z",
  "Seed": 42,
  "Notes": "first mission",
  "Files": [
    "torch_session.jsonl",
    "combat_session.jsonl",
    "audio_session.jsonl"
  ],
  "Kind": "raid"
}
```

`SchemaVersion` = `JsonEventLogger.CurrentSchemaVersion` — поднимать в том же коммите, что меняет
DTO (несовместимые логи различаются только этой версией). Reader принимает `1.0.0`, `1.1.0` и
`1.2.0`; `9.9.9` и любая другая версия отвергаются с `Result`-ошибкой. `Kind` (`raid`/`estate`)
нужен WPF для выбора свежего run; отсутствие `Kind` = `raid`.

**Чтение (C#):** `JsonConvert.DeserializeObject<GameEvent>(строка, settings)` с
`TypeNameHandling = TypeNameHandling.Auto` и `GameEventTypeBinder` — `$type` в корне строки
резолвит конкретный тип. Биндер разрешает только allowlist событий и коллекций
(`List<T>` и `T[]` для строк и DTO контракта); `System.IO.FileInfo` и прочие типы отвергаются
до создания объекта. Запись идёт через
`SerializeObject(событие, typeof(GameEvent), settings)`: без declared-базового типа `Auto` не пишет
`$type` у корня (ловушка, покрыта тестом roundtrip).

**Замечания к формату:**
- Порядок свойств в строке не гарантируется (payload производного типа идёт перед базовыми
  полями) — читатель ориентируется по именам (PascalCase).
- Коллекции (`Hierarchy`, `Party`, `manifest.Files`, ...) могут нести собственный `$type`
  (объявлены как `IReadOnlyList<T>`/`List<T>`), причём имя рантайма разное:
  `System.Private.CoreLib` (.NET) / `mscorlib` (Mono в Unity) — `Newtonsoft` резолвит оба, но это
  видно в сырых строках.
- Вложенные concrete-типы (`AnimationRef`, `GameInput`, `GameRng`) `$type` не несут.

## 2. Базовые поля (есть у всех событий)

| Поле | Тип | Кто заполняет | Правила |
|---|---|---|---|
| `$type` | string | логгер | assembly-qualified имя события (см. §1) |
| `EventId` | GUID | шина (`GameEventBus.Publish`) | уникален, стамп при публикации |
| `Timestamp` | ISO8601 UTC | шина | момент `Publish` |
| `Sequence` | int | шина | монотонный счётчик от 1, порядок `Publish` |
| `Subsystem` | string | издатель (константы `SubsystemNames`) | ключ файла: `combat`, `map`, `inventory`, `torch`, `audio`, `ai`, `estate`; пустое значение → файл `unspecified_session.jsonl` (страховка) |
| `Input` | object\|null | издатель | `{"Kind":"playerClick"\|"aiDecision"\|"move"\|"pass"\|"retreat"\|"tick"\|"system","Target":…,"Skill":…,"Slot":…}`; `null` = системное событие без актора (снапшоты, шаг коридора, снутье факела, аудио) |
| `Rng` | object\|null | издатель | `{"Seed":…,"Roll":…,"Crit":…,"Modifier":…}`; у `SkillResolved` — бросок/шанс/крит первой цели (hit roll для miss/dodge, crit roll при проверке крита; `Seed` = `null`); у `AudioPlayed` — только шанс нарратива в `Modifier` (§5.7) |

Инварианты базы: события создаются только для публикации через шину (шина перезаписывает
`EventId`/`Timestamp`/`Sequence` на копии, дошедшей до подписчиков); в `Core.GameEvents` нет
исключений и нет JSON.

**Новое в 1.1:** `CombatantState` получил `Rank`/`Size`/`Stress`/`MaxStress`/`AtDeathsDoor`/
`IsDead`/`IsAfflicted`/`IsVirtued`/`Statuses`/`Buffs`, `StateSnapshot` — `Battle`, появились
`SkillResolved`, `UnitMoved`, `TurnStarted`/`TurnEnded`, `RoundStarted`/`RoundEnded`,
`BattleEnded`, `AudioPlayed`. Все новые параметры — optional-хвосты записей, поэтому старые
конструкторы и записи `1.0.0` остаются читаемыми.

**Новое в 1.2:** `GameEventManifest.Kind` (`raid`/`estate`), subsystem `estate`, `estateSnapshot`
и action-события `estateHeroRecruited`/`Dismissed`/`Resurrected`, `estateBuildingUpgraded`,
`estateQuestSelected`, `estateProvisionsPurchased`, `estateHeirloomExchanged`,
`estateTownEventTriggered`; DTO `EstateResourceAmount`, `EstateHeroState`,
`EstateBuildingState`, `EstateQuestState`. Reader понимает `1.0.0`/`1.1.0`/`1.2.0`; старый
manifest без `Kind` считается `raid`.

**Схема id (Unity v1, `GameEventRecording.cs`):** герой — `hero_<RosterId>` (стабильно во всём
рейде; `CombatId` героев присваивается только при старте боя), монстр — `monster_<CombatId>`
(идёт в пределах боя; battleStart-снапшот публикуется уже после присвоения CombatId). Снапшоты и
боевые события используют **одни и те же** id. Логические имена ассетов: `Skeleton`/`Class` —
контентный ключ (`crusader`, `brigand_cutthroat_A`), `Icon` — `<class>_portrait`; без путей.

## 3. Типы событий v1.2

### 3.1 `sceneSnapshot` (Subsystem `map`)

- **Триггер:** Unity v1 — один раз при входе в рейд (старт сэйва / загрузка уровня).
- **Поля:** `SceneName` string — логич. имя сцены; `Hierarchy` — плоский список
  `{"Id","ParentId","Components"[]}` (без вложенности); `Globals` `{"Torch","Gold"}`;
  `Heroes[]` — `CombatantState` (в 1.1 с rank/size/stress/statuses/buffs, см. §3.2; логические
  имена ассетов, не пути); `Party[]` string — id героев в порядке формации.
- **Инварианты:** каждый id из `Party` присутствует в `Heroes`; `0 <= Torch <= 100`;
  `ParentId` = `null` либо id узла того же списка (циклов нет); снапшот самодостаточен —
  следующие события применяются к нему без истории.
- **Unity v1:** `Hierarchy` содержит `party_root` + героев (`hero_*`); полное дерево сцены не
  дампится.
- **Пример** (сокращён `…`, реальная строка — вывод теста; payload → базовые поля):

```json
{"$type":"Sektor.DarkestDungeon.Core.GameEvents.SceneSnapshot, Sektor.DarkestDungeon.Core.GameEvents","SceneName":"Dungeon","Hierarchy":{"$type":"…List`1…","$values":[{"Id":"party_root","ParentId":null,"Components":…},{"Id":"hero_1","ParentId":"party_root","Components":…}]},"Globals":{"Torch":100,"Gold":1000},"Heroes":…,"Party":…,"EventId":"75408e6b-3f59-40fe-a8a9-c49aa350ce73","Timestamp":"2026-10-19T12:00:00.1234567Z","Sequence":1,"Subsystem":"map","Input":null,"Rng":null}
```

### 3.2 `stateSnapshot` (Subsystem `combat`/`map` — задаётся издателем)

- **Триггер:** `Context` = `battleStart` | `battleEnd` | `turnEnd` | `roomEntered` |
  `inventoryOpened` (`StateSnapshotContexts`). Unity v1.1 публикует `battleStart` (после
  `SpawnEncounter`), `turnEnd` (в `PostHeroTurn`/`PostMonsterTurn`) и `battleEnd` (перед
  `FinishEncouter`); `roomEntered`/`inventoryOpened` — после порта map/inventory.
- **Поля:** `Context` string; `Globals` `{"Torch","Gold"}`; `Combatants[]` — все участники
  (`IsEnemy` различает стороны); `Battle` (`BattleState?`) — состояние боевого цикла, `null` вне
  боя.
- **`CombatantState`:** `Id`, `Name`, `Hp`, `MaxHp`, `IsEnemy`, `Icon`, `Skeleton`; в 1.1 —
  `Rank` (1..4; `0` = запись до 1.1), `Size` (1..4; `1` по умолчанию), `Stress`, `MaxStress`
  (`0` = неприменимо), `AtDeathsDoor`, `IsDead`, `IsAfflicted`, `IsVirtued`, `Statuses` и
  `Buffs` (`null` = не захвачено).
- **`BattleState`:** `Round`, `Turn` (`"Hero"`/`"Monster"`; в Unity определяется по
  `Character.IsMonster`, а не по сломанному `Round.PreMonsterTurn`), `ActiveUnitId`,
  `OrderedUnitIds` (оставшийся порядок), `Surprise`, `Status`, `Retreat`.
- **Инварианты:** `Combatants` полон для контекста (бой: обе стороны); `Rank` монотонно
  возрастает от фронта; `Statuses`/`Buffs` содержат только `IsApplied`-эффекты; самодостаточность
  — как у `sceneSnapshot`.
- **Диагностика:** пустые слоты формации (`FormationUnit.Character == null`) в `Combatants` не
  попадают; ошибка построения снапшота логируется `[GameEventLog] ... failed` и не прерывает бой.
- **Пример (payload):** `{"Context":"battleStart","Globals":{"Torch":99,"Gold":1000},"Combatants":[{"Id":"hero_1","Name":"crusader","Hp":33,"MaxHp":33,"IsEnemy":false,"Icon":"crusader_portrait","Skeleton":"crusader","Rank":1,"Size":1,"Stress":4,"MaxStress":100,"AtDeathsDoor":false,"IsDead":false,"IsAfflicted":false,"IsVirtued":false,"Statuses":[],"Buffs":[]},{"Id":"monster_3","Name":"brigand_cutthroat","Hp":12,"MaxHp":12,"IsEnemy":true,"Icon":"brigand_cutthroat_portrait","Skeleton":"brigand_cutthroat","Rank":1,"Size":1,"Stress":0,"MaxStress":0,"AtDeathsDoor":false,"IsDead":false,"IsAfflicted":false,"IsVirtued":false,"Statuses":[],"Buffs":[]}],"Battle":{"Round":1,"Turn":"Hero","ActiveUnitId":"hero_1","OrderedUnitIds":["hero_2","monster_3"],"Surprise":"Nothing","Status":"Active","Retreat":false}}`

### 3.3 `partyMoved` (Subsystem `map`)

- **Триггер:** Unity v1 — переход партии между локациями (комната ↔ сектор коридора).
- **Поля:** `FromId`, `ToId` string — id комнаты или сектора.
- **Инварианты:** `FromId != ToId`; оба — id из текущей карты.
- **Пример:** `{"$type":"…PartyMoved,…","FromId":"room1_1","ToId":"hallroom2_1_room1_1","EventId":"49ed29ae-e048-420a-b7ef-a8a3b72fb693","Timestamp":"2026-10-19T12:00:00.1234567Z","Sequence":3,"Subsystem":"map","Input":null,"Rng":null}`

### 3.4 `roomDiscovered` (Subsystem `map`)

- **Триггер:** комната открыта впервые (knowledge `Hidden → Scouted/Visited`).
- **Поля:** `RoomId` string; `RoomType` string — `AreaType.ToString()` (например `Entrance`,
  `BattleTresure`; последнее — историческая опечатка в enum ядра).
- **Инварианты:** `RoomId` непустой; повторное открытие не публикуется.
- **Пример (payload):** `{"RoomId":"room2_1","RoomType":"BattleTresure"}`

### 3.5 `attackExecuted` (Subsystem `combat`)

- **Триггер:** скилл исполнен. В Unity v1 — из `RaidSceneManager` (hero/monster пути) кадром после
  `ExecuteSkillAnimationIntro`: `AttackerAnimation` = фактическое имя проигранного клипа
  (`FormationUnit.CurrentState.AnimationName` → `SkeletonAnimation.state`); если активный скелет в
  этот момент переключается Mecanim-контроллером, имя берётся из текущего/следующего клипа
  `Animator` (`GetCurrent/GetNextAnimatorClipInfo(0)`), а пустое значение логируется
  `[GameEventLog] no attack animation`. `TargetAnimation` **всегда `null`** (у юнитов нет
  hit-реакций; FX-скелеты — отдельные сущности).
- **Поля:** `AttackerId`, `TargetId`, `SkillName` string; `AttackerAnimation`/`TargetAnimation`
  — `{"Skeleton","Animation","Track","Loop"}` либо `null` (анимация не инструментирована).
- **Инварианты:** `AttackerId != TargetId`; `SkillName` непустой; анимации — **факт** того, что
  проигрывалось (запись рантайм-хуком; `null` допустим и не приравнивается к idle).
- **Пример:**

```json
{"$type":"Sektor.DarkestDungeon.Core.GameEvents.AttackExecuted, Sektor.DarkestDungeon.Core.GameEvents","AttackerId":"hero_1","TargetId":"monster_3","SkillName":"stunning_blow","AttackerAnimation":{"Skeleton":"crusader","Animation":"attack_stun","Track":0,"Loop":false},"TargetAnimation":null,"EventId":"29d60fe8-9b2e-42fd-bc9b-63751d53ec1e","Timestamp":"2026-10-19T12:00:00.1234567Z","Sequence":5,"Subsystem":"combat","Input":{"Kind":"playerClick","Target":"monster_3","Skill":"stunning_blow","Slot":null},"Rng":null}
```

### 3.6 `heroStatChanged` (Subsystem `combat`)

- **Триггер:** атомарное изменение стата юнита (урон/хил/стресс — Unity v1: из точки применения).
- **Поля:** `HeroId`, `Stat` (напр. `hp`), `OldValue`, `NewValue` int, `Source`
  (`SkillResultEntry.Type.ToString()`; в бою v1 — `Hit`), `Invariant` string|null — документация
  инварианта для потребителя (напр. `newValue >= 0`).
- **Инварианты:** сам инвариант лежит в поле `Invariant` (клэмпы выполняет игра, не логгер);
  значения int — дробные статы (дейтл-модификаторы) добавляются отдельным шагом.
- **Пример (payload):** `{"HeroId":"monster_3","Stat":"hp","OldValue":12,"NewValue":8,"Source":"Hit","Invariant":"newValue >= 0"}`

### 3.7 `torchChanged` (Subsystem `torch`)

- **Триггер:** значение факела изменилось (шаг коридора, использование факела, снутье,
  зажигание); Unity v1 детектит в точке изменения факела.
- **Поля:** `OldValue`, `NewValue` int (0..100), `Reason` string; v1-коды из Unity
  (`TorchMeter`): `corridorStep` (шаг коридора — дефолт), `snuffed` (снутие кликом),
  `torchUsed` (предмет/костёр — `IncreaseTorch`), `modifier` (модификатор).
- **Инварианты:** `0 <= NewValue <= 100`; `OldValue != NewValue`; `Reason` непустой.
- **Пример:** `{"$type":"…TorchChanged,…","OldValue":60,"NewValue":55,"Reason":"snuffed","EventId":"95916262-32a2-49c2-91b0-d586a9670ccf","Timestamp":"2026-10-19T12:00:00.1234567Z","Sequence":7,"Subsystem":"torch","Input":null,"Rng":null}`

### 3.8 `skillUnavailable` (Subsystem `combat`)

- **Триггер:** попытка взять скилл, который нельзя применить (негативный тест-кейс, §5 спеки).
  **v1: из Unity не публикуется** — причина отказа живёт в локальных ветках
  `RaidCombatSkillsPanel.SetUsableCombatSkills`, а disabled-кнопка не шлёт клик (точки попытки
  нет); после порта проверок в WPF событие может публиковать потребитель.
- **Поля:** `Skill` string, `Reason` string, `CooldownRemaining` int|null.
- **Инварианты:** `Skill` и `Reason` непустые; `CooldownRemaining` заполняется только при
  `Reason == "cooldown"`.
- **Пример (payload):** `{"Skill":"smite","Reason":"cooldown","CooldownRemaining":2}`

### 3.9 `actionRejected` (Subsystem `combat`/`map`/`inventory` — задаётся издателем)

- **Триггер:** отклонённое действие игрока (напр. хил на полный HP), §5 спеки.
  **v1: из Unity не публикуется** — молчаливый drop в `FormationOverlaySlot` не даёт дешёвой
  честной точки; см. §3.8.
- **Поля:** `Reason` string, `Subsystem` — домен действия, `Input` object **обязателен**
  (в этом весь смысл события), `Rng` — null.
- **Инварианты:** `Input != null`; `Reason` непустой.
- **Пример:** `{"$type":"…ActionRejected,…","Reason":"targetFullHp","EventId":"18e3a6ec-…","…","Subsystem":"combat","Input":{"Kind":"playerClick","Target":"hero_1","Skill":"battle_heal","Slot":null},"Rng":null}`

### 3.10 `skillResolved` (Subsystem `combat`, v1.1)

- **Триггер:** сразу после `BattleSolver.ExecuteSkill` для каждой цели, до аудио скилла; публикует
  `RaidSceneManager.ExecuteSkillBase`. Это **механическая истина** боя, `attackExecuted` остаётся
  визуальным событием (кто/что/куда и какой клип играл).
- **Поля:** `AttackerId`, `SkillName`, `Targets[]` (`SkillTargetResult`), `Effects[]` — имена
  применённых эффектов скилла, `Input`, `Rng`.
- **`SkillTargetResult`:** `TargetId`, `ResultType` (`Hit`/`Crit`/`Miss`/`Dodge`/`Heal`/
  `CritHeal`/`Utility`), `Amount` (урон/хил/стресс; `0` для промаха), `IsHit`, `IsZeroed`,
  `IsHarmful`, `HpAfter`, `StressAfter`, `RankAfter` (после move-эффектов).
- **`Rng`:** для первой цели события `Roll` — бросок крита, если проверка крита выполнялась, иначе бросок попадания;
  `Modifier` — соответствующий шанс (`CritChance + CritMod` или `hitChance`); `Crit` — результат
  крита; `Seed` не пишется (`null`). Для мультицелевого скилла RNG остальных целей в `Targets`
  не дублируется — это ограничение v1.1 (§5.7).
- **Инварианты:** одна запись на каждую цель, включая miss; `Targets` непустой для боевого
  скилла; `Amount >= 0`; `HpAfter`/`StressAfter` — фактическое состояние после скилла;
  `StressAfter = 0` для юнитов без стресса (монстры).
- **Пример (payload):** `{"AttackerId":"hero_2","SkillName":"slice","Targets":[{"TargetId":"monster_3","ResultType":"Crit","Amount":8,"IsHit":true,"IsZeroed":false,"IsHarmful":true,"HpAfter":4,"StressAfter":31,"RankAfter":1}],"Effects":["damage"],"Input":{"Kind":"playerClick","Target":"monster_3","Skill":"slice","Slot":null},"Rng":{"Seed":null,"Roll":0.07,"Crit":true,"Modifier":0.1}}`

### 3.11 `unitMoved` (Subsystem `combat`, v1.1)

- **Триггер:** после `SkillResolved`, если скилл сдвинул ранг (push/pull/shuffle/swap); сравнение
  `CaptureRanks()` до скилла с фактическими `Rank` после.
- **Поля:** `UnitId`, `FromRank`, `ToRank`, `Cause` (в v1.1 — `"skill"`), `SwapWithId` (если
  известен), `Input`, `Rng`.
- **Инварианты:** `FromRank != ToRank`; `1 <= FromRank, ToRank <= 4`; `UnitMoved` идёт после
  `SkillResolved` того же скилла.
- **Пример (payload):** `{"UnitId":"hero_2","FromRank":2,"ToRank":1,"Cause":"skill","SwapWithId":"hero_1","Input":null,"Rng":null}`

### 3.12 `turnStarted` (Subsystem `combat`, v1.1)

- **Триггер:** `Round.PreHeroTurn`/`PreMonsterTurn` после удаления актора из `OrderedUnits`.
- **Поля:** `ActorId`, `Round`, `Turn` (`"Hero"`/`"Monster"`), `OrderedUnitIds` — оставшийся
  порядок, `Input`, `Rng`.
- **Инварианты:** `ActorId` не входит в `OrderedUnitIds`; `Round >= 1`.
- **Пример (payload):** `{"ActorId":"hero_2","Round":1,"Turn":"Hero","OrderedUnitIds":["monster_3"],"Input":null,"Rng":null}`

### 3.13 `turnEnded` (Subsystem `combat`, v1.1)

- **Триггер:** `PostHeroTurn`/`PostMonsterTurn`; сразу за ним публикуется `stateSnapshot`
  `turnEnd`.
- **Поля:** `ActorId`, `Round`, `Input`, `Rng`.
- **Инварианты:** `ActorId` — тот же, что в парном `turnStarted`; `Round >= 1`.
- **Пример (payload):** `{"ActorId":"hero_2","Round":1,"Input":null,"Rng":null}`

### 3.14 `roundStarted` (Subsystem `combat`, v1.1)

- **Триггер:** `Round.NextRound` после инкремента `RoundNumber`; первый раунд — из `StartBattle`.
- **Поля:** `Round`, `OrderedUnitIds` — полный порядок раунда, `Input`, `Rng`.
- **Инварианты:** `Round >= 1`; `OrderedUnitIds` непустой.
- **Пример (payload):** `{"Round":2,"OrderedUnitIds":["hero_1","hero_2","monster_3"],"Input":null,"Rng":null}`

### 3.15 `roundEnded` (Subsystem `combat`, v1.1)

- **Триггер:** `Round.NextRound` перед пересборкой порядка (начиная со второго раунда).
- **Поля:** `Round` — завершившийся раунд, `Input`, `Rng`.
- **Инварианты:** `Round >= 1`; идёт до `roundStarted` следующего раунда.
- **Пример (payload):** `{"Round":1,"Input":null,"Rng":null}`

### 3.16 `battleEnded` (Subsystem `combat`, v1.1)

- **Триггер:** `RaidSceneManager` перед `FinishEncouter`, после выхода из боевого цикла.
- **Поля:** `Winner` (`heroes`/`monsters`/`unknown`), `Rounds`, `Retreat`, `Input`, `Rng`.
- **Инварианты:** `Rounds >= 0`; winner — эвристика по живым юнитам
  (`BattleWinners`); если живы обе стороны — `unknown`; при retreat событие сейчас не
  публикуется, потому что цикл выходит раньше (`yield break`).
- **Пример (payload):** `{"Winner":"heroes","Rounds":2,"Retreat":false,"Input":null,"Rng":null}`

### 3.17 `audioPlayed` (Subsystem `audio`, v1.1)

- **Триггер:** фактический вызов FMOD — `DarkestSoundManager.PlayOneShot`, музыка/эмбиент/
  нарратив и прямые `RuntimeManager.CreateInstance` в `RaidSceneManager` (старт боя, курио).
  С 1.2 пишется также в estate-run (town music/UI); нужен только активный recorder.
- **Поля:** `EventPath` — точный путь, запрошенный в рантайме (включая динамические
  `<class>_<skill>`); `Kind` (`music`/`ambience`/`voice`/`sfx`/`ui`/`props`/`camp`/`town`,
  по префиксу пути); `Action` (`oneShot`/`start`/`stop`/`pause`/`resume`); `Volume`;
  `Position` `{X,Y,Z}` для 3D; `Parameters[]` `{Name,Value,Label}`; `Input`, `Rng`.
- **Инварианты:** `EventPath` непустой и начинается с `event:/`; банк в событие **не**
  дублируется — WPF сопоставляет путь с `docs\audio\audio_catalog.json`; `Rng` заполняется
  только у нарратива (`Modifier = Chance`), без seed/roll.
- **Пример (payload):** `{"EventPath":"event:/char/hero/highwayman/attack","Kind":"sfx","Action":"oneShot","Volume":0.8,"Position":{"X":1,"Y":0,"Z":2},"Parameters":[{"Name":"material","Value":1,"Label":null}],"Input":null,"Rng":null}`

### 3.18 `estateSnapshot` (Subsystem `estate`, v1.2)

- **Триггер:** вход в `EstateManagement` после инициализации ростера/зданий/currency, затем после
  каждого estate-действия (`RecruitHero`, `DismissHero`, `BuyUpgrade`, quest/provision/exchange).
- **Поля:** `Week`, `Resources[]` (`EstateResourceAmount`: `Id`, `Amount` — `gold`/`bust`/
  `portrait`/`deed`/`crest`), `Roster[]` (`EstateHeroState`), `Buildings[]`
  (`EstateBuildingState`: `Id`, `Name`, `UpgradeRatio` 0..1), `Quests[]` (`EstateQuestState`),
  `TownEventId`/`TownEventTone` (могут быть `null`), `Input`, `Rng`.
- **`EstateHeroState`:** `Id` (`hero_<rosterId>`), `Class`, `Name`, `Level`, `ResolveLevel`,
  `Hp`, `MaxHp`, `Stress`, `Status`, `IsAlive`, `IsAfflicted`, `IsVirtued`, `Quirks[]`,
  `Diseases[]`.
- **`EstateQuestState`:** `Id`, `Dungeon`, `Type`, `Difficulty`, `Length`, `GoalId`, `IsPlot`.
- **Инварианты:** snapshot самодостаточен; после каждого действия публикуется новый snapshot,
  поэтому WPF не пересчитывает ресурсы/ростер вручную. `Rng` — `null`.
- **Пример (payload):** `{"Week":2,"Resources":[{"Id":"gold","Amount":750}],"Roster":[{"Id":"hero_1","Class":"crusader","Name":"Reynauld","Level":1,"ResolveLevel":1,"Hp":33,"MaxHp":33,"Stress":10,"Status":"Available","IsAlive":true,"IsAfflicted":false,"IsVirtued":false,"Quirks":[],"Diseases":[]}],"Buildings":[{"Id":"abbey","Name":"Abbey","UpgradeRatio":0.5}],"Quests":[],"TownEventId":null,"TownEventTone":null,"Input":null,"Rng":null}`

### 3.19 `estateHeroRecruited` (Subsystem `estate`, v1.2)

- **Триггер:** `HeroRosterPanel.CreateSlot` после `Estate.RecruitHero` (StageCoach/town event).
- **Поля:** `HeroId`, `Class`, `Name`, `Level`, `ResolveLevel`, `Cost[]` (может быть `null`),
  `Input`, `Rng`.
- **Инварианты:** `HeroId` совпадает с id в следующем `estateSnapshot`; затем публикуется snapshot.

### 3.20 `estateHeroDismissed` (Subsystem `estate`, v1.2)

- **Триггер:** `Campaign.DismissHero` перед удалением из ростера.
- **Поля:** `HeroId`, `Class`, `Name`, `Input`, `Rng`.
- **Инварианты:** после удаления публикуется новый `estateSnapshot` без героя.

### 3.21 `estateHeroResurrected` (Subsystem `estate`, v1.2)

- **Триггер:** `HeroRosterPanel.CreateSlot`, если `Estate.RecruitHero` вернул `DeathRecord`.
- **Поля:** `HeroId`, `Class`, `Name`, `Cost[]`, `Input`, `Rng`.

### 3.22 `estateBuildingUpgraded` (Subsystem `estate`, v1.2)

- **Триггер:** успешный `Estate.BuyUpgrade(string treeId, TownUpgrade upgrade, bool isFree)`.
- **Поля:** `BuildingId` (префикс `treeId` до `.`), `TreeId`, `UpgradeId`, `UpgradeRatio`,
  `Cost[]` (для free — `null`), `Input`, `Rng`.
- **Инварианты:** публикуется после списания валюты; затем — `estateSnapshot`.

### 3.23 `estateQuestSelected` (Subsystem `estate`, v1.2)

- **Триггер:** `RaidPreparationManager.DungeonPanelQuestSelected`.
- **Поля:** `QuestId`, `Dungeon`, `Type`, `Difficulty`, `Length`, `Input`, `Rng`.

### 3.24 `estateProvisionsPurchased` (Subsystem `estate`, v1.2)

- **Триггер:** `ShopManager` purchase/sellback (`ShopInventoryPurchase`, `SellSingleItem`,
  `SellOutEverything`, `ShopInventorySellBackSlot`).
- **Поля:** `ItemId`, `Amount`, `GoldCost` (положительный при покупке, отрицательный/0 при
  продаже), `IsSellback`, `Input`, `Rng`.
- **Инварианты:** после действия публикуется `estateSnapshot` с точным золотом.

### 3.25 `estateHeirloomExchanged` (Subsystem `estate`, v1.2)

- **Триггер:** `ExchangeEntry.ConfirmButtonClicked` после изменения `Estate.Currencies`.
- **Поля:** `FromId`, `FromAmount`, `ToId`, `ToAmount`, `Input`, `Rng`.
- **Инварианты:** затем `estateSnapshot` содержит новые остатки.

### 3.26 `estateTownEventTriggered` (Subsystem `estate`, v1.2)

- **Триггер:** `EstateSceneManager.Start`, если `Campaign.TriggeredEvent != null`.
- **Поля:** `TownEventId`, `Tone`, `Input`, `Rng`.
- **Инварианты:** следующий `estateSnapshot` содержит `TownEventId`/`TownEventTone`.

## 4. Статус чек-листа §8 спеки (v1.2)

- [x] `Input`/`Rng` присутствуют у всех событий; `null` с причиной — §2 этой схемы.
- [x] Каждое событие применимо к ближайшему снапшоту (самодостаточность — §3.1/§3.2).
- [x] Негативные события: типы `skillUnavailable`/`actionRejected` покрыты контрактом; из Unity v1
      не публикуются (§3.8/§3.9); `moveBlocked`, нехватка предмета — **отложены** вместе с
      item*/move-событиями.
- [x] Unity-издатели v1.2 зафиксированы в §2 (id-схема) и §3 (триггеры/причины); compile-check
      `unity\` зелёный (коллизии имён Unity7 закрыты алиасами — `Diagnostics\README`, gotcha).
- [x] Ассеты — логические имена (`Icon`, `Skeleton`, `AnimationRef`), не пути.
- [x] Иерархия снапшота — плоский список с `ParentId`.
- [x] `manifest.json`: `SchemaVersion`, `Seed`, список файлов, `Kind`.
- [x] Схема покрывает 100% типов v1.2 (26 типов, §3) с инвариантами.
- [x] Боевой цикл v1.1: `Rank`/`Size`, `Stress`, статусы/баффы, `Battle`, `SkillResolved`,
      `UnitMoved`, `TurnStarted/Ended`, `RoundStarted/Ended`, `BattleEnded`.
- [x] Аудио v1.1: `audio_session.jsonl`, `AudioPlayed` с фактическим путём/kind/action/volume/
      position/parameters; каталог `docs\audio\audio_catalog.json` — 2007 путей, 40 банков,
      20 music events.
- [ ] Примеры всех типов Spine-анимаций скелета (атака/hit/idle/death) — эталонный захват S3
      покрывает атаки (`attack_*` в `docs/spec_game_events/examples/`); hit/idle/death не входят в
      события v1 (`attackExecuted.TargetAnimation` всегда `null`).
- [x] RNG боевых бросков: `SkillResolved.Rng` содержит бросок/шанс/крит первой цели (hit roll для
      miss/dodge, crit roll при проверке крита); `AudioPlayed.Rng` — только `Modifier` нарратива.
      Per-target rolls мультицелевого скилла не дублируются (§5.7).
- [x] Estate v1.2: отдельный run (`Kind = estate`), `estateSnapshot` + 8 action-событий
      (`estateHeroRecruited/Dismissed/Resurrected`, `estateBuildingUpgraded`,
      `estateQuestSelected`, `estateProvisionsPurchased`, `estateHeirloomExchanged`,
      `estateTownEventTriggered`).
- Отложено (не в v1.2): `item*`-события, `spineEventTriggered`, нумерация `combat_01` (файлы
  боёв), полный map/`MapSnapshot`, WPF FMOD playback, дробные статы и активности героев
  (Abbey/Tavern/Sanitarium/Blacksmith/Guild).

## 5. Отклонения от спеки

1. Дискриминатор — `$type` (Newtonsoft), не `"type"` из STJ-примера §1.2: спека допускала
   Newtonsoft-вариант, STJ в проекте отсутствует.
2. Сериализация живёт в `Clients.EventLog` (граница клиента), не в Core — правило AGENTS
   (JSON на границе; Newtonsoft уже есть в обоих Unity-деревьях).
3. DTO — `record` + полифилл `IsExternalInit`: TFM остался `netstandard2.0` (доставка в оба
   Unity-дерева), поднят только `LangVersion`.
4. Один файл `<subsystem>_session.jsonl` за run; нумерация `combat_01.jsonl` (по номеру боя) —
   отложена: рекордер пока не знает границ боёв. В 1.1 добавлен `audio_session.jsonl`, в 1.2 —
   `estate_session.jsonl` и `Kind` в manifest.
5. `Sequence` стампит шина (порядок `Publish`), не издатель.
6. `heroStatChanged`: int-значения (hp/stress); дробные статы — отдельным шагом.
7. RNG: `AudioPlayed` пишет шанс нарратива в `Modifier`; `SkillResolved.Rng` — бросок первой
   цели (hit roll для miss/dodge, иначе crit roll, если проверка была). `Seed` не пишется,
   per-target rolls мультицелевого скилла не дублируются — ограничение среза, не формата; поле
   nullable.
8. `BattleEnded`: winner — эвристика по живым (`heroes`/`monsters`/`unknown`); при retreat
   событие не публикуется (цикл выходит до `PublishBattleEnded`). Reader/UI принимают оба
   варианта.
9. `AudioPlayed`: с 1.2 пишется и в рейде, и в estate-run (town music/UI); нужен только активный
   recorder. Записи `1.0.0`/`1.1.0` содержат только рейдовое аудио.
10. `GameEventTypeBinder` дополнительно разрешает массивы (`T[]`) для allowlist-элементов:
    собственные записи `JsonEventLogger` сериализуют некоторые коллекции как массивы.

## 6. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.GameEvents/` — `GameEvent.cs`, `GameEventBus.cs`,
  `IGameEventBus.cs`, `GameInput.cs`/`GameInputKinds.cs`, `GameRng.cs`, `SubsystemNames.cs`,
  `StateSnapshotContexts.cs`; события §3 (по одному типу на файл): `SceneSnapshot`,
  `StateSnapshot`, `PartyMoved`, `RoomDiscovered`, `AttackExecuted`, `HeroStatChanged`,
  `TorchChanged`, `SkillUnavailable`, `ActionRejected`, `SkillResolved`, `UnitMoved`,
  `TurnStarted`, `TurnEnded`, `RoundStarted`, `RoundEnded`, `BattleEnded`, `AudioPlayed`;
  estate v1.2: `EstateSnapshot`, `EstateHeroRecruited`, `EstateHeroDismissed`,
  `EstateHeroResurrected`, `EstateBuildingUpgraded`, `EstateQuestSelected`,
  `EstateProvisionsPurchased`, `EstateHeirloomExchanged`, `EstateTownEventTriggered`;
  DTO (`SceneNode`, `SceneGlobals`, `CombatantState`, `CombatantStatusState`,
  `CombatantBuffState`, `BattleState`, `SkillTargetResult`, `AudioPosition`, `AudioParameter`,
  `AnimationRef`, `BattleWinners`, `AudioEventKinds`, `AudioActions`, `EstateResourceIds`,
  `EstateResourceAmount`, `EstateHeroState`, `EstateBuildingState`, `EstateQuestState`).
- `src/Clients/Sektor.DarkestDungeon.Clients.EventLog/` — `JsonEventLogger.cs`,
  `GameEventManifest.cs` (`Kind`), `GameEventRunKinds.cs`, `GameEventLogReader.cs`
  (allowlist-биндер, проверки `SchemaVersion`/`Sequence`/`Subsystem`, поддержка
  `1.0.0`/`1.1.0`/`1.2.0`), `GameEventTypeBinder.cs` (`List<T>`/`T[]`), `GameEventCapture.cs`,
  `GameEventFileNames.cs`, `GameEventCaptureLocator.cs` (поиск `<repo>\captures` и свежего
  `run_*` нужного kind).
- Unity-издатель: `unity\Assets\Scripts\Diagnostics\GameEventRecorder.cs` (run-директория,
  `Kind`, выбор raid/estate), `GameEventRecording.cs` (снапшоты, скиллы, ходы/раунды, бой,
  estate, аудио), `Managers\RaidSceneManager.cs` (боевой цикл, `ExecuteSkillBase`,
  стартовый звук/курио), `Mechanics\Battle\Round.cs` (turn/round hooks),
  `Mechanics\Battle\BattleSolver.cs`/`SkillResultEntry.cs` (hit/crit roll),
  `Mechanics\RandomSolver.cs` (roll overload), `Managers\DarkestSoundManager.cs`
  (музыка/нарратив/one-shot), `Character\Character.cs` (`GetBuffs`),
  `Managers\EstateSceneManager.cs` (snapshot/town event), `Campaign\Estate.cs` (building
  upgrade), `Campaign\Campaign.cs` (dismiss), `Managers\RaidPreparationManager.cs` (quest),
  `Managers\ShopManager.cs` (provisions), `UI\Panels\HeroRosterPanel.cs` (recruit/resurrect),
  `UI\Controls\ExchangeEntry.cs` (heirloom exchange).
- WPF-потребитель: `src/Wpf/Sektor.DarkestDungeon.Wpf/MainWindow.xaml.cs` (выбор свежего
  `run_*` нужного kind через `GameEventCaptureLocator`), `ViewModels/CaptureReplayViewModel.cs`
  (боевая проекция), `ViewModels/EstateReplayViewModel.cs` (estate-проекция),
  `CaptureUnitViewModel.cs`, `Estate*ViewModel.cs`, `Views/CaptureReplayView.xaml`,
  `Views/EstateReplayView.xaml`, `Data/CaptureAnimationCatalog.cs`; входы — «АРЕНА 1×1» и
  «ИМЕНИЕ (ЗАХВАТ)» в меню. `Rank` пишется; точные x/y по-прежнему не пишутся.
- Тесты-образцы: `tests/Core/Sektor.DarkestDungeon.Core.GameEvents.Tests/GameEventBusTests.cs`,
  `tests/Clients/Sektor.DarkestDungeon.Clients.EventLog.Tests/JsonEventLoggerTests.cs`,
  `GameEventLogReaderTests.cs`, `GameEventCaptureLocatorTests.cs`, `EstateCaptureReaderTests.cs`,
  `tests/Wpf/Sektor.DarkestDungeon.Wpf.Tests/CaptureReplay*.cs`,
  `EstateReplayViewModelTests.cs`.
- Аудио: `docs/audio/00_index.md`, `audio_catalog.json`, `audio_catalog.md`;
  генератор `tools/extract-fmod-catalog.ps1`.
- Эталон: `docs/spec_game_events/examples/` — `1.0.0` (§7).
- Спека: `docs/SPEC_GAME_EVENTS.md`; решения по срезу — git history (S2/S3/1.1), результаты —
  `docs/changelog/2026-10-20-unity-raid-event-recorder.md`,
  `docs/changelog/2026-10-20-wpf-capture-replay.md`,
  `docs/changelog/2026-10-20-game-events-1-1-audio.md`,
  `docs/changelog/2026-10-20-estate-capture.md`.

## 7. Эталонный захват (S3)

`docs/spec_game_events/examples/` — реальный прогон Unity 7 от 2026-10-20 (сцена `Dungeon`,
стартовый сэйв, 38 событий, `Sequence` 1..38 без дыр):

- `map_session.jsonl` — `sceneSnapshot`, 7 `partyMoved`, `roomDiscovered` (`room2_1`,
  `BattleTresure`);
- `combat_session.jsonl` — 2 `stateSnapshot` (`battleStart`), 5 `heroStatChanged`, 6
  `attackExecuted` (анимации `attack_slice`, `attack_uppercut`, `attack_stun`, `attack_pistol`,
  `attack_scroll`);
- `torch_session.jsonl` — 3 `corridorStep`, 9 `snuffed`, 1 `torchUsed`, 3 `corridorStep`;
- `manifest.json` — `SchemaVersion` 1.0.0, `StartedAt`, `Files`.

Захват читается как исполняемая спецификация для WPF-потребителя: JSONL по `Sequence`,
применение событий к ближайшему снапшоту. WPF-экран читает самый свежий `<repo>\captures\run_*`
(если записей нет — эталон, линкуемый в output как `Content\Captures\Reference`) через
`GameEventLogReader` и проигрывает
`attackExecuted` по одному, ожидая завершения нецикличной анимации атакующего (fallback —
таймер, если Spine-контента нет). Финальное состояние — последний `stateSnapshot` (seq 38):
факел 7, комната `room2_1`, новый бой `brigand_blood` + `brigand_fusilier`; предыдущий
`monster_3` доведён до 0 HP на seq 28 и замещён этим снапшотом.

**1.1.0:** эталонный файл остаётся `1.0.0`; читатель и WPF проверяют 1.1 на synthetic-захвате
(`GameEventLogReaderTests.Read_CurrentSchemaCapture_RestoresCombatAndAudioPayloads`,
`CaptureReplayViewModelTests.CurrentSchemaCapture_ProjectsRanksStressStatusesBattleAndAudio`):
10 событий, `Rank`/`Size`/stress/statuses/buffs, `SkillResolved` с RNG, `UnitMoved`,
`TurnStarted`, `RoundStarted`, `BattleEnded`, `AudioPlayed`. Старый эталон в WPF по-прежнему
расставляет героев по порядку (fallback rank) и не падает.

**1.2.0:** эталонный файл остаётся `1.0.0`; estate-run проверяется synthetic-захватом
(`EstateCaptureReaderTests.Read_EstateCapture_RestoresManifestKindAndEvents`,
`EstateReplayViewModelTests.EstateCapture_ProjectsEconomyRosterBuildingsAndActions`):
`Kind = estate`, `estateSnapshot`, action-события, аудио, повторный snapshot. WPF battle replay
читает только `raid`, estate replay — только `estate`; manifest без `Kind` считается `raid`.
