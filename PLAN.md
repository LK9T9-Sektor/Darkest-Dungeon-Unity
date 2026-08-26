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

- [ ] **B1** Реализации `IBattleContext`/`IBattleEvents`/`IBattleGround`/`ICombatUnit` в WPF над core-моделью
- [ ] **B2** Контроллер дуэли: старт по двум `party_config`, локальный `Round`+`BattleSolver`, приём вводов, фидбек в `BattleScreenViewModel`

### Этап C. WPF сетевой glue (над `ITransport`)

- [ ] **C1** `DuelSessionManager` — хост/джоин Steam-комнаты, `RunCallbacks`, события
- [ ] **C2** `DuelBridge` — wire: `party_config` + вводы (`hero_skill_selected`/`move`/`pass`) + барьер `player_loaded`
- [ ] **C3** Сид сессии по формуле §6 (упорядоченные player id → `sessionSeed`)
- [ ] **C4** `party_config` DTO: 4× {класс, имя, сид, флаги скиллов}
- [ ] **C5** Steam runtime в WPF: `steam_api64.dll` + `steam_appid.txt` рядом с exe

### Этап D. Лобби с выбором героев (WPF)

- [ ] **D1** Лобби: 4 слота героев (выбор класса), Host/Join, копирование session id
- [ ] **D2** Отправка `party_config` при джойне; старт дуэли по обоим config

### Этап E. Де-риск и интеграция

- [ ] **E1** Дуэль двух WPF-инстансов на `InMemoryTransport` (без Steam): локстап + выбор героев
- [ ] **E2** Переключение на `SteamTransport` (Steam-комната, LAN→интернет)

### Тесты

- [ ] Детерминизм (A7)
- [ ] Codec `party_config`, round-trip
- [ ] Флоу дуэли на `InMemoryTransport` (два клиента, барьер, старт)

## Приёмка

- [ ] Два WPF-клиента сходятся в локстапе (одинаковый исход при одном сиде)
- [ ] Выбор героев в лобби, `party_config` обменивается до старта
- [ ] Бой идёт через core `BattleSolver` без Unity
- [ ] Steam-комната создаётся/входит по session id; дуэль играбельна