# PLAN: BattleTest — тест-сцена боёв как первый Unity-потребитель ядра (прототип Фазы 6)

## Goal

Новая сцена `BattleTest` в обоих деревьях (`unity\` активное, `unity-2017\` легаси): гибкий
настраиваемый бой (герои/монстры по слотам, скиллы/квирки/тринкеты, контроль сторон, seed, torch)
на **чистом ядре боя** (`Core.Combat`/`Core.Duel`) с полной детализацией визуала и **нулевой
связанностью** — Unity = тонкий вид-слой. Это первый реальный потребитель вынесенного ядра боя в
Unity = прототип Фазы 6 (`EXTRACTION_PLAN.md`). Легаси-движок и игра не трогаются (read-only).

## Design decisions (user-confirmed)

- **Движок:** `DuelController` напрямую (паттерн WPF), НЕ `FightSession` (у `FightSession` только
  skill+target и инвертированный local/remote). `StartFight` (heroes-vs-monsters) / `StartDuel`+`isHost`
  (hero-vs-hero). AI: монстры — `Solver.UseMonsterBrain`, AI-герои — `DuelAi`.
- **Вид — путь 2 (чистый новый слой):** переиспользуем только активы: Spine-префабы юнитов
  (`Resources/Prefabs/Heroes|Monsters`), спрайты/портреты, `UnitAnimator`, `AnimatedEffect`,
  `RaidPartyCamera` (с фиксом 1 ссылки на `RaidSceneManager.RoomView...`), чистые виджеты
  (`RoundIndicator`, `RaidAnnouncment`, `Backdrop`, `RankPlaceholders`, `HealthBar`,
  `StressOverlayPanel`, `TrayPanel`, `PopupDialog`). **НЕ переиспользуем:** `RaidSceneManager`,
  `BattleGround`, `PartyFormationManager`, `RaidPanel`-семейство, `RaidEvents` (слой C).
- **Событий в ядре нет** → вид поллит/диффит состояние после каждого действия: `CurrentUnit`
  → ход, `RoundNumber` → раунд, `IsDead` → смерть, `Rank`/`Party.Units` → движение,
  `Solver.SkillResult.SkillEntries` → урон/крит/хил/мисс, `Events.PopupShown` → статусы,
  `Events.Log` → лог. Паттерн — `DuelBattleViewModel`.
- **Сцену собирает editor-тула** (`Assets\Editor\BattleTestSceneBuilder.cs`, batch `-executeMethod`)
  в обоих деревьях; компоновка камеры/UI — чистая, без dungeon/raid-блота.
- Оба дерева; легаси не правим; новые `.cs` — с `.meta`; доки — в том же коммите.
- Ветка: `core/battle-test-scene` (создана).

## Steps / status

### M1 — Core PvE-сверка (ядро, тесты)

1. [x] `tests\Clients\Sektor.DarkestDungeon.Clients.Content.Tests\FightPveSweepTests.cs` —
   hero-vs-monster sweep по всем монстрам (225 из 230; 5 босс-пропов исключены с причиной:
   cauldron_empty_* — captor-сосуд хагги `prot 1`/initiative 0, ancestor_nebula/small_D —
   стадии босса), детерминизм по сиду, чистое завершение, без исключений.
2. [x] Закрыты в ядре дыры, всплывшие в sweep: **NRE в `DiseaseEffect.ApplyQueued`**
   (`AddRandomDisease()` == null); конкретные id `.disease` (`the_worries`, `rabies`) теперь
   парсятся и резолвятся (`IBattleContext.GetQuirk` → `Hero.AddQuirk(IQuirk)`). Рандомный пул
   болезней — стаб (документировано). `.summon/.control/.capture` — осознанно не парсятся
   (не всплыли); idle-DoT ×1.5 и корпус-подстановка `.kill` — остаются задокументированными
   разрывами (sweep их не воспроизводит).
3. [x] `dotnet test` зелёный (все 10 тест-проектов). `BATTLE_PARITY.md` + `00_index.md` +
   `docs\mechanics\combat\15_disease.md` обновлены в коммите.

### M2 — Новый Unity-вид-слой (оба дерева, `Assets\Scripts\Testing\BattleTest\`)

1. [x] Конфиг: `BattleTestConfig` / `BattleTestSideSpec` / `BattleTestSlotSpec` (+ `.meta`).
2. [x] `CoreBattleDriver` (MonoBehaviour): владеет `DuelController`, сборка из конфига
   (`StartFight` heroes-vs-monsters / `StartDuel` hero-vs-hero), маппинг `ICombatUnit`↔вид,
   поллинг/диффинг, гейт ввода по `IsPlayerControlled` + `IsLocalTurn`, AI-роутинг
   (`UseMonsterBrain`/`DuelAi`), пейсинг, сид, торч.
3. [x] `BattleEventsAdapter` — подписчик `DuelBattleEvents.PopupShown` → попап-слой (паттерн WPF;
   отдельная реализация `IBattleEvents` не нужна — ядро уже шлёт события).
4. [x] Вид Stage-1: `BattleUnitView` (префаб + HP/stress бары + выделение + смерть),
   `BattleFormationView` (по `FormationDisplayOrder`), `BattleHud` (раунд/аннонс/скиллы/цели/лог/
   победитель), `BattlePopupLayer` (урон/статусы), `BattleTestConfigPanel` (2×4 слота, режим,
   seed, torch, FIGHT). Stage-2 (анимации скиллов, слайдинг, гало, move-хендлинг) — позже.
5. [x] Инфраструктура: вместо `WorldToScreenBridge`/`RulesSource` — новый чистый код без проекции
   (бары/попапы в мире, UI в ScreenSpaceOverlay); FMOD-стаб не нужен (вид не вызывает звук).
6. [x] Оба дерева компилируются: `unity-compile-check.ps1` (6000 и 2017.4) зелёные.
   Примечание: legacy-твины (`CombatSkill`, `SkillResult`, `Team`, …) в глобальном неймспейсе
   перекрывают core-типы из `using` — конфликт решён полной квалификацией.

### M3 — Editor-тула + сцена (оба дерева, `Assets\Editor\`)

1. [x] `BattleTestSceneBuilder.cs` (MenuItem + batch `Generate`): собирает `BattleTest.unity` —
   чистая ортокамера, `Battlefield` (WorldSpace-канвас + 2 формации), `BattleHud`, драйвер +
   конфиг-панель, без EventSystem в сцене (создаётся рантаймом `RuntimeUiFactory.EnsureEventSystem`,
   чтобы не ссылаться на engine-GUID'ы). Без `DarkestDungeonManager.prefab` (контент грузит
   `FightContentLoader`, локализация не нужна).
2. [x] `tools\unity-generate-battle-test-scene.ps1` (batch в обоих проектах).
3. [x] Сцены сгенерированы в обоих деревьях и закоммичены (+ `.meta`). Script-reference check
   обоих деревьев зелёный.

### M4 — Интеграция и верификация

1. [x] `unity-compile-check.ps1` (оба дерева) — компиляция + script-reference check.
2. [ ] Ручной чеклист `TESTING.md`: конфиг → запуск → контроль игроком (skill/target/pass/move)
   → AI → попапы/анимации → победа → сид-детерминизм. **(требует открытия сцены в редакторе —
   автоматически не проверяется; Stage-1 вид ни разу не запускался в Play Mode — вероятны
   runtime-нюансы компоновки.)**
3. [x] Кросс-деревные отличия (Spine/uGUI 2017.4 vs 6000) — скомпилировано в обоих; вёрстка
   проверяется при ручном прогоне.

### M5 — Документация (в тех же коммитах)

1. [ ] `docs\mechanics\presentation\presentation_unity_battle_view.md` — порядок срабатывания +
   gotchas (событий нет → poll/diff; `StartFight` инвертирует local/remote; legacy-твины в глобальном
   неймспейсе).
2. [ ] `TESTING.md` (раздел), `INDEX.md`, `docs\mechanics\00_index.md` (уже — `15_disease.md`),
   `CHANGELOG.md`.

## Affected files

- `src\Core\Sektor.DarkestDungeon.Core.Combat\...`, `src\Core\Sektor.DarkestDungeon.Core.Duel\...`
  (только M1-фиксы).
- `tests\Core\Sektor.DarkestDungeon.Core.Duel.Tests\FightPveSweepTests.cs`.
- `unity\Assets\Scripts\Testing\BattleTest\*.cs` (+ `.meta`) — 15–20 файлов.
- `unity-2017\Assets\Scripts\Testing\BattleTest\*.cs` (+ `.meta`) — зеркально.
- `unity\Assets\Editor\BattleTestSceneBuilder.cs`, `unity-2017\Assets\Editor\...` (+ `.meta`).
- `unity\{,-2017}\Assets\Scenes\BattleTest.unity` (+ `.meta`).
- `tools\unity-generate-battle-test-scene.ps1`.
- Доки: `docs\BATTLE_PARITY.md`, `docs\EXTRACTION_STATUS.md`, `docs\TESTING.md`,
  `docs\INDEX.md`, `docs\mechanics\00_index.md`, `docs\mechanics\presentation\presentation_unity_battle_view.md`,
  `docs\CHANGELOG.md`.

## Acceptance criteria

- `dotnet test` зелёный (включая новые PvE-sweep тесты).
- `unity-compile-check.ps1` зелёный в обоих деревьях.
- Ручной чеклист боя проходит (TESTING.md): конфиг → бой → контроль игроком → AI → попапы/анимации
  → победа → сид-детерминизм.
- Легаси-файлы не изменены (только чтение).

## Out of scope

- Полный cutover игры (Campaign/Networking/остальное) — позже, отдельные фазы.
- «Игрок управляет настоящими монстрами» — требует расширения ядра; сначала Player-heroes-vs-AI,
  AI-vs-AI, hotseat hero-vs-hero.
- `Difficulty` — если в ядре нет уровней монстров, маппим на resolve-уровень героев или откладываем.