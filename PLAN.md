# PLAN.md — WPF↔WPF Steam-дуэль с выбором героев в лобби

> **СТАТУС (восстановление контекста).** Весь план выполнен и запушен в ветку `wpf`
> (`git push origin wpf`). Рабочее дерево чистое. Ниже — что сделано, как проверить, что осталось.

## Итог: сделано (по этапам, с коммитами)

| Этап | Что | Коммиты |
|---|---|---|
| **A. Модель героя в ядре** | `Attribute`/`SingleAttribute`/`PairedAttribute`/`FlatModifier`; статусы (`Statuses\`, 9 шт. + база); `Character` (движок баффов `ApplyAllBuffRules` + `BattleRulesContext`), `Hero`, `HeroClass`, `CharacterMode`, `Trait`, `Resolve`; поле (`FormationUnit`/`Party`/`UnitInfo`/`Ranks`/`BattleGround`); `HeroGeneration` (герой из сида); **тесты детерминизма** | `f9c3ee7`, `85388c0`, `457799b` |
| **B. WPF battle-runtime** | `DuelBattleEvents`/`DuelBattleContext` (реализуют `IBattleEvents`/`IBattleContext`), `DuelClasses` (образцы 4 классов), `DuelController` (старт дуэли, `BattleSolver`) | `be6c6d4` |
| **C. Сетевой glue** | `DuelSessionManager` (хост/джоин, pump, события, `RivalInputReceived`), `DuelWire`, `DuelPartyConfig`, `DuelSeed` (сид §6), `InMemoryTransport`, `DuelTransportFactory` (Steam/InMemory); интеграционный тест локстапа | `5b7e6a1` |
| **D. Лобби** | `DuelLobbyView`/`DuelLobbyViewModel`/`HeroSlotViewModel` (4 слота героев, Host/Join/Copy/Leave), кнопка «Multiplayer Duel» в MainWindow | `c4ff68e` |
| **Вводы (ходы 1:1 как в Unity)** | `Round.StartBattle/NextRound/InsertInitiativeRoll` (порядок по скорости), `FormationUnitInfo.UpdateNextRound`; `DuelController` — **обе стороны строят одинаковые формирования** (Heroes=отряд хоста, Monsters=отряд клиента; хост вводит за Heroes, клиент за Monsters), `DuelPhase`, `ExecuteLocalSkill`/`ApplyRemoteSkill`; wire `rpc.hero_skill`; тесты `DuelTurnFlowTests` | `69fd29a` |
| **Рендер состояния** | `DuelBattleViewModel` (снапшот юнитов/скиллов/статуса/лога из core) + `DuelBattleView` (клики: скилл → цель → Execute → отправка ввода), `Refresh()`; тесты `DuelRenderTests` | `8677072` |
| **Steam runtime (E2)** | `steam_api64.dll` копируется пост-билдом в вывод WPF; `steam_appid.txt` — dev-локальный (gitignored) | `c4ff68e` |

## Как проверить (всё зелёное)

```
dotnet test tests/Core/Sektor.DarkestDungeon.Core.Combat.Tests   # 28
dotnet test tests/Core/Sektor.DarkestDungeon.Core.Content.Tests  # 10
dotnet test tests/Wpf/Sektor.DarkestDungeon.Wpf.Tests            # 4 (локстап, turn-флоу, рендер)
dotnet build src/Wpf/Sektor.DarkestDungeon.Wpf                   # 0 ошибок
```

**Играть:** 2 инстанса WPF, `steam_appid.txt` рядом с exe, Steam запущен → «Multiplayer Duel» → на одном Host, на втором Join по session id → обмен партиями → живой бой по кликам (свой ход подсвечен, скиллы активны, цели золотые).

## Ключевые точки архитектуры (для продолжения)

- **Локстап**: `NETWORK.md` §6. Обе стороны строят ИДЕНТИЧНЫЕ формирования (`DuelController.StartDuel(hostPicks, clientPicks, seed, isHost)`) — это было критично (иначе не сходилось). По сети — только вводы (`rpc.hero_skill` = `skillId|targetId`; действующий юнит определяется порядком хода) и `party_config`.
- **RNG**: `RandomSolver` статический (общий на процесс). Для дуэли это ок (1 дуэль на процесс); в одиночных тестах пере-сидим перед каждым действием обеих сторон.
- **Ядро**: `src\Core\Sektor.DarkestDungeon.Core.Combat` (netstandard2.0, C# 7.3), структура папок = `Assets\Scripts\` (правило AGENTS.md). Интерфейсы: `ICharacter`, `ICombatUnit`, `IBattleGround`, `IBattleContext`, `IBattleEvents`.
- **WPF**: `src\Wpf\Sektor.DarkestDungeon.Wpf` (net8.0-windows, latest), ссылки на Core.Combat + Lan.Contracts + Lan.Steam.

## Что осталось / возможные следующие шаги

1. **Живой рендер в существующий `BattleScreenView` (мокап)** — сейчас бой в отдельном `DuelBattleView`; можно перевести мокап на `DuelBattleViewModel` (заменить хардкод).
2. **Победа/поражение в UI** — статус «Battle finished» есть, но без явной панели победителя.
3. **`steam_appid.txt` автосоздание** рядом с exe (как в Unity-тулзах) — сейчас dev-локальный.
4. **Проверка на реальном Steam** (2 машины / 2 аккаунта) — не гонялось; in-memory-тесты покрыли логику.
5. **Выбор героев из полного контента** (`HeroClass`-данные сейчас образцы в `DuelClasses`), полный статус-набор эффектов в дуэли.
6. **Полировка UI** (хелс-бары, анимации, тултипы, звук).

## Цель

Мультиплеерная дуэль «как в Unity» между двумя WPF-клиентами: Steam-комната (лобби), выбор 4 героев
**до комнаты** (в лобби), детерминированный локстап (`NETWORK.md` §6). Обе стороны локально считают
состояние через общее ядро, по сети идут только `party_config` и вводы.

## Ключевые решения

- **Выбор героев — ДО комнаты** (в лобби): одно состояние, `party_config` отправляется сразу при джойне, проще протокол.
- **Только hero-срез модели**: «враги» = отряд соперника (по §6), обе стороны — герои. `Monster`/`MonsterData`/AI-brain не нужны (вводы от игроков).
- **Локстап обязан сходиться**: общая детерминированная модель + `RandomSolver` с сидом сессии — в ядре.
- **Де-риск:** этап 1 — `InMemoryTransport` (два WPF-инстанса), этап 2 — `SteamTransport`.

## Этапы

### Этап A. Модель героя в ядре (обязательный фундамент)

- [x] **A1** `Attribute`/`AttributeModifier` в `src\Core\...\Character\` (модифицируемые статы)
- [x] **A2** Статусы в `Character\Statuses\`: `StatusEffect`(база) + конкретные — реализуют core-интерфейсы
- [x] **A3** `Character`(база) + `Hero` + `HeroClass` + `CharacterMode` + `Stress` + `Resolve` + `Trait` — реализуют `ICharacter`
- [x] **A4** Поле/формации как чистое состояние (`FormationUnit`, `FormationParty`, `FormationUnitInfo`, `FormationRanks`, `BattleGround`)
- [x] **A5** `HeroGeneration` — детерминированная генерация героя из сида
- [x] **A6** Движок баффов (`ApplyAllBuffRules`/`RemoveConditionalBuffs`)
- [x] **A7** Тест детерминизма: одинаковый сид → идентичный исход (ключевой тест локстапа)

### Этап B. WPF battle-runtime

- [x] **B1** `DuelBattleEvents`/`DuelBattleContext` в `src\Wpf\...\Combat\` (реализуют `IBattleEvents`/`IBattleContext` над core-моделью)
- [x] **B2** `DuelClasses` (образцы классов героев, общие для обоих клиентов) + `DuelController` (старт дуэли из пиков, `BattleSolver`, ход/раунд, приём вводов)

### Этап C. WPF сетевой glue (над `ITransport`)

- [x] **C1** `DuelSessionManager` — хост/джоин сессии над `ITransport`, `Pump`, события
- [x] **C2** `DuelWire`/`DuelBridge` — wire: `party_config` + `player_loaded` барьер + `rpc.*` вводы
- [x] **C3** `DuelSeed` — сид сессии по формуле §6 (упорядоченные player id → sessionSeed) + `StableHash`
- [x] **C4** `DuelPartyConfig` DTO: класс|сид ×4, Serialize/Deserialize
- [x] **C5** `DuelTransportFactory` (SteamTransport/InMemory) + `InMemoryTransport`; Steam runtime (`steam_api64.dll`/`steam_appid.txt` рядом с exe) — на этапе упаковки
- [x] Интеграционный тест `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests`: две сессии + две дуэли над InMemory → обмен config, барьер, сид, локстап сходится (зеркальные юниты совпадают)

### Этап D. Лобби с выбором героев (WPF)

- [x] **D1** `DuelLobbyView`/`DuelLobbyViewModel`/`HeroSlotViewModel`: 4 слота героев (цикл по классам), Host/Join, Copy ID, Leave, статус, pump-таймер; кнопка «Multiplayer Duel» в MainWindow
- [x] **D2** Отправка `party_config` при джойне; барьер готовности → `DuelController.StartDuel` (сид сессии §6)

### Этап E. Де-риск и интеграция

- [x] **E1** Локстап + выбор героев проверены интеграционным тестом на `InMemoryTransport` (две стороны сходятся)
- [x] **E2** `SteamTransport` подключён (`DuelTransportFactory`); `steam_api64.dll` копируется в вывод WPF пост-билдом; `steam_appid.txt` — dev-локальный (gitignored)

### Тесты

- [ ] Детерминизм (A7)
- [ ] Codec `party_config`, round-trip
- [ ] Флоу дуэли на `InMemoryTransport` (два клиента, барьер, старт)

## Приёмка

- [ ] Два WPF-клиента сходятся в локстапе (одинаковый исход при одном сиде)
- [ ] Выбор героев в лобби, `party_config` обменивается до старта
- [ ] Бой идёт через core `BattleSolver` без Unity
- [ ] Steam-комната создаётся/входит по session id; дуэль играбельна