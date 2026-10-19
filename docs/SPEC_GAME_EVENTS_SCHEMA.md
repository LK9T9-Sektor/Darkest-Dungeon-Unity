# SPEC_GAME_EVENTS_SCHEMA.md — схема записываемых игровых событий (v1.0.0)

> Домен: `GameEvents` (`src\Core\Sektor.DarkestDungeon.Core.GameEvents`) +
> `Clients.EventLog` (`src\Clients\Sektor.DarkestDungeon.Clients.EventLog`).
> Источник истины — DTO в C#; этот файл их описание для читателя JSONL (требование §6 спеки
> `docs\SPEC_GAME_EVENTS.md`). Все примеры строк ниже — **реальный вывод** из тестов
> (`JsonEventLoggerTests`, `GameEventBusTests`), не выдуманные данные.

## 1. Формат файлов

Run-директория записи (её создаёт рекордер S3):

```
<run>/
  manifest.json            # см. ниже
  combat_session.jsonl     # по одному файлу на Subsystem
  map_session.jsonl
  torch_session.jsonl
  ...
```

- **JSONL**: UTF-8, одна строка = одно событие, запись с `AutoFlush` (краш теряет максимум одну
  строку). Порядок строк = порядок `Publish` (возрастающий `Sequence`).
- **manifest.json** (пишется сразу и переписывается при появлении каждого файла и на Dispose):

```json
{
  "SchemaVersion": "1.0.0",
  "StartedAt": "2026-10-19T12:00:00.1234567Z",
  "Seed": 42,
  "Notes": "first mission",
  "Files": [
    "torch_session.jsonl",
    "combat_session.jsonl"
  ]
}
```

`SchemaVersion` = `JsonEventLogger.CurrentSchemaVersion` — поднимать в том же коммите, что меняет
DTO (несовместимые логи различаются только этой версией).

**Чтение (C#):** `JsonConvert.DeserializeObject<GameEvent>(строка, settings)` с
`TypeNameHandling = TypeNameHandling.Auto` — `$type` в корне строки резолвит конкретный тип.
Запись идёт через `SerializeObject(событие, typeof(GameEvent), settings)`: без declared-базового
типа `Auto` не пишет `$type` у корня (ловушка, покрыта тестом roundtrip).

**Замечания к формату:**
- Порядок свойств в строке не гарантируется (payload производного типа идёт перед базовыми
  полями) — читатель ориентируется по именам (PascalCase).
- Коллекции (`Hierarchy`, `Party`, ...) могут нести собственный `$type` (объявлены как
  `IReadOnlyList<T>`), причём имя рантайма разное: `System.Private.CoreLib` (.NET) /
  `mscorlib` (Mono в Unity) — `Newtonsoft` резолвит оба, но это видно в сырых строках.
- Вложенные concrete-типы (`AnimationRef`, `GameInput`, `GameRng`) `$type` не несут.

## 2. Базовые поля (есть у всех событий)

| Поле | Тип | Кто заполняет | Правила |
|---|---|---|---|
| `$type` | string | логгер | assembly-qualified имя события (см. §1) |
| `EventId` | GUID | шина (`GameEventBus.Publish`) | уникален, стамп при публикации |
| `Timestamp` | ISO8601 UTC | шина | момент `Publish` |
| `Sequence` | int | шина | монотонный счётчик от 1, порядок `Publish` |
| `Subsystem` | string | издатель (константы `SubsystemNames`) | ключ файла: `combat`, `map`, `inventory`, `torch`, `ai`; пустое значение → файл `unspecified_session.jsonl` (страховка) |
| `Input` | object\|null | издатель | `{"Kind":"playerClick"\|"aiDecision"\|"tick"\|"system","Target":…,"Skill":…,"Slot":…}`; `null` = системное событие без актора (снапшоты, шаг коридора, снутье факела) |
| `Rng` | object\|null | издатель | `{"Seed":…,"Roll":…,"Crit":…,"Modifier":…}`; **`null` в v1 почти всегда** — причина: солвер рейда ещё не инструментирован (заполняется отдельным шагом) |

Инварианты базы: события создаются только для публикации через шину (шина перезаписывает
`EventId`/`Timestamp`/`Sequence` на копии, дошедшей до подписчиков); в `Core.GameEvents` нет
исключений и нет JSON.

## 3. Типы событий v1

### 3.1 `sceneSnapshot` (Subsystem `map`)

- **Триггер:** S3 — один раз при входе в рейд (старт сэйва / загрузка уровня).
- **Поля:** `SceneName` string — логич. имя сцены; `Hierarchy` — плоский список
  `{"Id","ParentId","Components"[]}` (без вложенности); `Globals` `{"Torch","Gold"}`;
  `Heroes[]` `{"Id","Name","Hp","MaxHp","IsEnemy","Icon","Skeleton"}` (логические имена ассетов,
  не пути); `Party[]` string — id героев в порядке формации.
- **Инварианты:** каждый id из `Party` присутствует в `Heroes`; `0 <= Torch <= 100`;
  `ParentId` = `null` либо id узла того же списка (циклов нет); снапшот самодостаточен —
  следующие события применяются к нему без истории.
- **Пример** (сокращён `…`, реальная строка — вывод теста; payload → базовые поля):

```json
{"$type":"Sektor.DarkestDungeon.Core.GameEvents.SceneSnapshot, Sektor.DarkestDungeon.Core.GameEvents","SceneName":"Dungeon","Hierarchy":{"$type":"…List`1…","$values":[{"Id":"dungeon_root","ParentId":null,"Components":…},{"Id":"hero_1","ParentId":"party_slot_1","Components":…}]},"Globals":{"Torch":100,"Gold":1000},"Heroes":…,"Party":…,"EventId":"75408e6b-3f59-40fe-a8a9-c49aa350ce73","Timestamp":"2026-10-19T12:00:00.1234567Z","Sequence":1,"Subsystem":"map","Input":null,"Rng":null}
```

### 3.2 `stateSnapshot` (Subsystem `combat`/`map` — задаётся издателем)

- **Триггер:** `Context` = `battleStart` | `turnEnd` | `roomEntered` | `inventoryOpened`
  (`StateSnapshotContexts`); по спеке §4.1 — в начале боя, после хода, при смене комнаты.
- **Поля:** `Context` string; `Globals` `{"Torch","Gold"}`; `Combatants[]` — все участники
  (`IsEnemy` различает стороны).
- **Инварианты:** `Combatants` полон для контекста (бой: обе стороны); самодостаточность — как
  у `sceneSnapshot`.
- **Пример (payload):** `{"Context":"battleStart","Globals":{"Torch":99,"Gold":1000},"Combatants":[{"Id":"crusader_01","Name":"Crusader","Hp":22,"MaxHp":22,"IsEnemy":false,…},{"Id":"brigand_01","Name":"Brigand Cutthroat","Hp":12,"MaxHp":12,"IsEnemy":true,…}]}`

### 3.3 `partyMoved` (Subsystem `map`)

- **Триггер:** S3 — переход партии между локациями (комната ↔ сектор коридора).
- **Поля:** `FromId`, `ToId` string — id комнаты или сектора.
- **Инварианты:** `FromId != ToId`; оба — id из текущей карты.
- **Пример:** `{"$type":"…PartyMoved,…","FromId":"room1_1","ToId":"hallroom2_1_room1_1","EventId":"49ed29ae-e048-420a-b7ef-a8a3b72fb693","Timestamp":"2026-10-19T12:00:00.1234567Z","Sequence":3,"Subsystem":"map","Input":null,"Rng":null}`

### 3.4 `roomDiscovered` (Subsystem `map`)

- **Триггер:** комната открыта впервые (knowledge `Hidden → Scouted/Visited`).
- **Поля:** `RoomId` string; `RoomType` string (например `entrance`, `BattleTreasure`).
- **Инварианты:** `RoomId` непустой; повторное открытие не публикуется.
- **Пример (payload):** `{"RoomId":"room2_1","RoomType":"BattleTreasure"}`

### 3.5 `attackExecuted` (Subsystem `combat`)

- **Триггер:** скилл исполнен (S3 — из точки результата боя: `ExecuteSkillBase` → результат).
- **Поля:** `AttackerId`, `TargetId`, `SkillName` string; `AttackerAnimation`/`TargetAnimation`
  — `{"Skeleton","Animation","Track","Loop"}` либо `null` (анимация не инструментирована).
- **Инварианты:** `AttackerId != TargetId`; `SkillName` непустой; анимации — **факт** того, что
  проигрывалось (запись рантайм-хуком; `null` допустим и не приравнивается к idle).
- **Пример:**

```json
{"$type":"Sektor.DarkestDungeon.Core.GameEvents.AttackExecuted, Sektor.DarkestDungeon.Core.GameEvents","AttackerId":"crusader_01","TargetId":"brigand_01","SkillName":"smite","AttackerAnimation":{"Skeleton":"Crusader","Animation":"smite","Track":0,"Loop":false},"TargetAnimation":null,"EventId":"29d60fe8-9b2e-42fd-bc9b-63751d53ec1e","Timestamp":"2026-10-19T12:00:00.1234567Z","Sequence":5,"Subsystem":"combat","Input":{"Kind":"playerClick","Target":"brigand_01","Skill":"smite","Slot":null},"Rng":{"Seed":42,"Roll":0.03,"Crit":true,"Modifier":0.15}}
```

### 3.6 `heroStatChanged` (Subsystem `combat`)

- **Триггер:** атомарное изменение стата юнита (урон/хил/стресс — S3: из точки применения).
- **Поля:** `HeroId`, `Stat` (напр. `hp`), `OldValue`, `NewValue` int, `Source`
  (напр. `attack`), `Invariant` string|null — документация инварианта для потребителя
  (напр. `newValue >= 0`).
- **Инварианты:** сам инвариант лежит в поле `Invariant` (клэмпы выполняет игра, не логгер);
  значения int — дробные статы (дейтл-модификаторы) добавляются отдельным шагом.
- **Пример (payload):** `{"HeroId":"brigand_01","Stat":"hp","OldValue":12,"NewValue":7,"Source":"attack","Invariant":"newValue >= 0"}`

### 3.7 `torchChanged` (Subsystem `torch`)

- **Триггер:** значение факела изменилось (шаг коридора, использование факела, снутье,
  зажигание); S3 детектит в точке изменения факела.
- **Поля:** `OldValue`, `NewValue` int (0..100), `Reason` string (`corridorStep`, `itemUsed`,
  `snuffed`, `ignited`, …).
- **Инварианты:** `0 <= NewValue <= 100`; `OldValue != NewValue`; `Reason` непустой.
- **Пример:** `{"$type":"…TorchChanged,…","OldValue":60,"NewValue":55,"Reason":"snuffed","EventId":"95916262-32a2-49c2-91b0-d586a9670ccf","Timestamp":"2026-10-19T12:00:00.1234567Z","Sequence":7,"Subsystem":"torch","Input":null,"Rng":null}`

### 3.8 `skillUnavailable` (Subsystem `combat`)

- **Триггер:** попытка взять скилл, который нельзя применить (негативный тест-кейс, §5 спеки).
- **Поля:** `Skill` string, `Reason` string, `CooldownRemaining` int|null.
- **Инварианты:** `Skill` и `Reason` непустые; `CooldownRemaining` заполняется только при
  `Reason == "cooldown"`.
- **Пример (payload):** `{"Skill":"smite","Reason":"cooldown","CooldownRemaining":2}`

### 3.9 `actionRejected` (Subsystem `combat`/`map`/`inventory` — задаётся издателем)

- **Триггер:** отклонённое действие игрока (напр. хил на полный HP), §5 спеки.
- **Поля:** `Reason` string, `Subsystem` — домен действия, `Input` object **обязателен**
  (в этом весь смысл события), `Rng` — null.
- **Инварианты:** `Input != null`; `Reason` непустой.
- **Пример:** `{"$type":"…ActionRejected,…","Reason":"targetFullHp","EventId":"18e3a6ec-…","…","Subsystem":"combat","Input":{"Kind":"playerClick","Target":"crusader_01","Skill":"battle_heal","Slot":null},"Rng":null}`

## 4. Статус чек-листа §8 спеки (v1)

- [x] `Input`/`Rng` присутствуют у всех событий; `null` с причиной — §2 этой схемы.
- [x] Каждое событие применимо к ближайшему снапшоту (самодостаточность — §3.1/§3.2).
- [x] Негативные события: `skillUnavailable`, `actionRejected` — покрыты;
      `moveBlocked`, нехватка предмета — **отложены** (вместе с item*/move-событиями).
- [x] Ассеты — логические имена (`Icon`, `Skeleton`, `AnimationRef`), не пути.
- [x] Иерархия снапшота — плоский список с `ParentId`.
- [x] `manifest.json`: `SchemaVersion`, `Seed`, список файлов.
- [x] Схема покрывает 100% типов v1 (9 типов, §3) с инвариантами.
- [ ] Примеры всех типов Spine-анимаций скелета (атака/hit/idle/death) — **S3**, эталонный захват.
- Отложено (не в v1): `item*`-события, `spineEventTriggered`, нумерация `combat_01` (файлы
  боёв), заполнение `Rng` в солвере, дробные статы.

## 5. Отклонения от спеки

1. Дискриминатор — `$type` (Newtonsoft), не `"type"` из STJ-примера §1.2: спека допускала
   Newtonsoft-вариант, STJ в проекте отсутствует.
2. Сериализация живёт в `Clients.EventLog` (граница клиента), не в Core — правило AGENTS
   (JSON на границе; Newtonsoft уже есть в обоих Unity-деревьях).
3. DTO — `record` + полифилл `IsExternalInit`: TFM остался `netstandard2.0` (доставка в оба
   Unity-дерева), поднят только `LangVersion`.
4. Один файл `<subsystem>_session.jsonl` за run; нумерация `combat_01.jsonl` (по номеру боя) —
   добавляется в S3, когда рекордер будет знать границы боёв.
5. `Sequence` стампит шина (порядок `Publish`), не издатель.
6. `heroStatChanged`: int-значения (hp/stress); дробные статы — отдельным шагом.

## 6. Файлы-источники

- `src/Core/Sektor.DarkestDungeon.Core.GameEvents/` — `GameEvent.cs`, `GameEventBus.cs`,
  `IGameEventBus.cs`, `GameInput.cs`/`GameInputKinds.cs`, `GameRng.cs`, `SubsystemNames.cs`,
  события §3 (по одному типу на файл), DTO (`SceneNode`, `SceneGlobals`, `CombatantState`,
  `AnimationRef`, `StateSnapshotContexts`).
- `src/Clients/Sektor.DarkestDungeon.Clients.EventLog/` — `JsonEventLogger.cs`,
  `GameEventManifest.cs`.
- Тесты-образцы: `tests/Core/Sektor.DarkestDungeon.Core.GameEvents.Tests/GameEventBusTests.cs`,
  `tests/Clients/Sektor.DarkestDungeon.Clients.EventLog.Tests/JsonEventLoggerTests.cs`.
- Спека: `docs/SPEC_GAME_EVENTS.md`; решения по срезу: `PLAN.md` (S2).
