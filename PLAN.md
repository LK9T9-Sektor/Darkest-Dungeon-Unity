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

1. [ ] Конфиг: `BattleTestConfig` / `BattleTestSideSpec` / `BattleTestSlotSpec` (+ `.meta`).
2. [ ] `CoreBattleDriver` (MonoBehaviour): владеет `DuelController`, сборка из конфига,
   маппинг `ICombatUnit`↔вид, поллинг/диффинг, гейт ввода по `IsLocalTurn`, AI-роутинг,
   пейсинг, сид.
3. [ ] `BattleEventsAdapter : IBattleEvents` — мост ядро→вид (попапы, гало, анимации, торч,
   pull/push, звук).
4. [ ] Вид: `BattleUnitView`, `BattleFormationView` (по `FormationDisplayOrder`),
   `BattleSkillPanel`, `BattleTargetOverlay`, `BattlePopupLayer`, `BattleRoundIndicator`,
   `BattleAnnouncement`, `BattleCameraDriver`, `BattleChoreographer` (анимации по `SkillArtInfo`).
5. [ ] Инфраструктура: `WorldToScreenBridge`, `RulesSource`, `AudioSink` (FMOD-стаб первым).
6. [ ] Конфиг-панель (паттерн `FightScreen`/`RuntimeUiFactory`).

### M3 — Editor-тула + сцена (оба дерева, `Assets\Editor\`)

1. [ ] `BattleTestSceneBuilder.cs` (MenuItem + batch): собирает `BattleTest.unity` — чистая камера,
   `EventSystem`, живой инстанс `DarkestDungeonManager.prefab` (контент/спрайты/локализация), поле
   боя (бэкдроп + формации), оверлей-канвас, конфиг-панель.
2. [ ] `tools\unity-generate-battle-test-scene.ps1` (batch в обоих проектах).
3. [ ] Сгенерировать сцену в обоих деревьях; закоммитить сцену + `.meta`.

### M4 — Интеграция и верификация

1. [ ] `unity-compile-check.ps1` (оба дерева) — компиляция + script-reference check.
2. [ ] Ручной чеклист `TESTING.md`: конфиг → запуск → контроль игроком (skill/target/pass/move)
   → AI → попапы/анимации → победа → сид-детерминизм.
3. [ ] Кросс-деревные отличия (Spine/uGUI 2017.4 vs 6000).

### M5 — Документация (в тех же коммитах)

1. [ ] `docs\mechanics\presentation\presentation_unity_battle_view.md` — порядок срабатывания +
   gotchas (событий нет → poll/diff; `StartFight` инвертирует local/remote; FMOD-стаб).
2. [ ] `TESTING.md` (раздел), `INDEX.md`, `docs\mechanics\00_index.md`, `CHANGELOG.md`.

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