# DUEL_ARCHITECTURE.md — Дуэль и мультиплеерный бой: состояние, критика, план

Документ-навигатор по дуэли (PvP-бою) и мультиплеерному бою: что это, где живёт в коде,
почему текущее состояние не идеально и что делать дальше. Цель — чтобы агент (ИИ/человек)
быстро понял картину, не читая сотни файлов. Правила репозитория — в `AGENTS.md`, карта
документов — в `INDEX.md`, долг — в `KNOWN_ISSUES.md`.

## 1. Что такое дуэль

Дуэль — режим боя **1v1 (PvP)**: две партии по 4 героя сталкиваются друг с другом.
Модель локстапа: обе стороны строят **одинаковые** формации детерминированно,
`Heroes` — партия хоста, `Monsters` — партия клиента (соперника). Обмениваются только
**вводами** (`DuelPayload`), состояние считается локально. Порядок хода — по инициативе
(скорость), как в Unity.

- Сид сессии — детерминированный из ID игроков (`DuelSeed.ComputeSessionSeed`).
- Проводятся: host/join (`DuelSessionManager`), обмен `party_config`, readiness-барьер,
  RPC-входы (`rpc.hero_skill`).

## 2. Происхождение (важно!)

Дуэль **не была придумана в WPF** — это ре-имплементация **мультиплеерного PvP-боя Unity**,
который существует в `unity\Assets\Scripts\Networking\`:

- `RaidSceneMultiplayerManager.cs` (**2285 строк**, MonoBehaviour, наследует одиночный
  `RaidSceneManager`): партия второго игрока спавнится как **сторона монстров**
  (`MultiplayerSync.MonsterSideRaidParty` → `BattleGround.SpawnMultiplayerEncounter`),
  lockstep-сид из ID игроков, тестовый рейд (`tutorial_room`/`weald`) захардкожен.
- `MultiplayerSync.cs` (**426 строк**): `party_config`, `LocalPartyData`/`RivalPartyData`,
  readiness (`PreparationCheck`), RPC-входы.

В Unity эта оркестрация **не «разнесена»**: она вшита в презентационный MonoBehaviour-слой
вместе с рейдом/UI/сценой/транспортом. Извлекать её «как есть» нельзя было — поэтому WPF
написал чистую C#-версию поверх уже вынесенного движка.

## 3. Инвентарь по слоям (после Фазы A)

### Чистое ядро — движок (`src\Core\Sektor.DarkestDungeon.Core.Combat`)

`BattleSolver`, `BattleGround`/`Round`, `FormationParty`/`FormationUnit`, скиллы/эффекты/статы,
`RandomSolver`, контент (`HeroClassFileParser`, `HeroCatalog`, `HeroGeneration`), AI-инфраструктура
(`MonsterBrain`, `SkillSelectionDesire`, `TargetSelectionDesire`, желания 9+8+6).

### Чистое ядро — дуэль (`src\Core\Sektor.DarkestDungeon.Core.Duel`, Фаза A)

- `DuelController` (+ `DuelHeroPick`) — оркестрация: фазовая машина, сборка партий из пиков,
  смена рангов (`TryMove`), смертность, стыковка раундов, исполнение скиллов.
- `DuelPhase`, `DuelSeed`, `DuelBattleContext` (адаптер `IBattleContext`), `DuelBattleEvents`
  (адаптер `IBattleEvents` + debug-лог).
- `DuelPayload` — wire-протокол (`skillId|targetId`, `move|rank`, `pass|0`).
- `IDuelContent` — порт контента (герой/квирк/бафф); ядро не грузит файлы.
- `DuelAi` + `DuelSkillSelection`/`DuelTargetSelection` — ИИ соперника на core-brain (Фаза B).
- netstandard2.0, C# 7.3, без движковых ссылок; доставляется пост-билдом в
  `Assets\Plugins\Internal` обоих деревьев (для будущего cutover Unity).

### WPF-клиент (`src\Wpf\...`)

- `DuelContent` — реализация `IDuelContent` поверх `DuelClasses`/`QuirkCatalog`/`BuffCatalog`.
- `AiRivalLink` — **тонкая** обёртка: таймер + `DuelAi` + `RivalActionReceived`.
- `NetworkRivalLink`, `DuelSessionManager`, `DuelPartyConfig`, транспорты — сеть/сессия.
- ViewModels/Views — биндинги и отрисовка.

### Unity (`unity\Assets\Scripts\Networking\`) — legacy, НЕ разнесён

`RaidSceneMultiplayerManager` + `MultiplayerSync` + `SteamRaidBridge`/`SteamSessionManager`/
`DarkestPhotonLauncher` — вся мультиплеерная оркестрация в презентационном слое. Ядро дуэли
Unity пока не потребляет (cutover — в роадмапе).

## 4. Критика (почему «не очень хорошо»)

1. **Дублирование оркестрации**: та же PvP-модель описана и в Unity (MonoBehaviour), и в
   `Core.Duel` (после A) — два источника истины для wire-протокола/локстапа. Unity-версию
   надо сводить к `Core.Duel` (фаза 6 EXTRACTION_PLAN: тонкие адаптеры).
2. **God-classes**: `RaidSceneMultiplayerManager` (2285) + `MultiplayerSync` (426) +
   `RaidSceneManager` (~6000, из KNOWN_ISSUES §2) — логика и представление вперемешку.
3. **Логика в презентации**: до Фазы A вся дуэльная оркестрация жила в `src\Wpf`
   (`DuelController` 413 строк и др.). Теперь вынесено; в Unity осталось.
4. **ИИ**: раньше случайные ходы (`AiRivalLink.BuildRandomAction` на `System.Random`).
   После B — через `DuelAi`/`MonsterBrain` с целью «минимальный HP». Полный потенциал
   core-desires (приоритеты, коулдауны, роли) ещё не использован (см. роадмап).
5. **Wire-протокол в двух местах**: до A строки `pass`/`move`/`skill|target` были в WPF VM
   и в Unity RPC-коде. Теперь константы в `DuelPayload` (ядро), Unity всё ещё свои.
6. **Нестабильный сид Unity**: `player.ID + player.ToString().GetHashCode()`
   (`RaidSceneMultiplayerManager.cs:32`) — см. KNOWN_ISSUES §5; `DuelSeed` в ядре использует
   стабильный хэш.
7. **Hardcode-тестовый рейд** в `RaidSceneMultiplayerManager` (`MultiplayerTestSave`,
   `tutorial_room`/`weald`).

## 5. Роадмап

- [x] **A** — оркестрация в `Core.Duel`; WPF тоньше; тесты в `tests\Core\...Core.Duel.Tests`.
- [x] **B** — ИИ соперника через `MonsterBrain`/desires (`DuelAi`, цель по минимальному HP).
- [ ] **C** — этот документ + обновление карты (`INDEX`/`ARCHITECTURE`/`KNOWN_ISSUES`/...).
- [ ] **Unity cutover**: перевести `RaidSceneMultiplayerManager`/`MultiplayerSync` на
  `Core.Duel` (тонкие адаптеры, фаза 6 EXTRACTION_PLAN); убрать дубль протокола и сид-хак.
- [ ] **Умный ИИ**: расширить желания (`SkillSelectionPreferred`, коулдауны, лечащие приоритеты)
  для героев-соперников; можно отдать в общий `MonsterBrain`-контент.
- [ ] **Механики дуэли** (из PLAN.md P1): эффекты скиллов, resolve/аффекции, modes (Абоминация),
  тринкеты — это ядро, дуэль получит их автоматически.

## 6. Как читать

- Быстрый старт агента: `AGENTS.md` → `INDEX.md` → `DUEL_ARCHITECTURE.md` → `PLAN.md`.
- Долг и границы: `KNOWN_ISSUES.md` (§2, §5, §7). Сеть: `NETWORK.md` (§6 локстап).
- Фазы выноса: `EXTRACTION_PLAN.md` (фаза 3 — движок, фаза 6 — тонкие Unity-адаптеры).