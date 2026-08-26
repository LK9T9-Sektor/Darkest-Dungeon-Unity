# PLAN.md — WPF↔WPF Steam-дуэль с выбором героев в лобби

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

- [ ] **A1** `Attribute`/`AttributeModifier` в `src\Core\...\Character\` (модифицируемые статы)
- [ ] **A2** Статусы в `Character\Statuses\`: `StatusEffect`(база) + `BleedingStatusEffect`, `PoisonStatusEffect`, `StunStatusEffect`, `MarkStatusEffect`, `RiposteStatusEffect`, `GuardStatusEffect`, `GuardedStatusEffect`, `DeathsDoorStatusEffect`, `DeathRecoveryStatusEffect`, `DamageOverTimeStatusEffect` — реализуют core-интерфейсы (`IDotStatusEffect` и др.)
- [ ] **A3** `Character`(база) + `Hero` + `HeroClass`(мин.: id, базовые статы, скиллы, теги, моды) + `Stress` + `Resolve` + `Trait`(мин.) — реализуют `ICharacter`
- [ ] **A4** Поле/формации как чистое состояние: `FormationUnit`(ICombatUnit), `FormationParty`(IFormationParty), `FormationUnitInfo`(IFormationUnitInfo), `FormationRanks`, `BattleGround`(IBattleGround) в `Raid\Battle\`/`Raid\Party\`
- [ ] **A5** `HeroGeneration` — детерминированная генерация героя из сида
- [ ] **A6** Движок баффов (`ApplyAllBuffRules`/`RemoveConditionalBuffs`) — нужно `BattleSolver.ApplyConditions`
- [ ] **A7** Тест детерминизма: одинаковый сид → идентичный исход боя на обеих «сторонах» (ключевой тест локстапа)

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