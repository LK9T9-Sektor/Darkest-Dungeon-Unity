# PLAN: BattleTest — тест-сцена боёв как первый Unity-потребитель ядра (прототип Фазы 6)

---

# PLAN: WPF — монстры/боссы размером 1–4 ранга: size-aware ранги в ядре + PvE-режим

## Goal

В Unity монстры и боссы занимают 1–4 позиции (`display: .size N`): size-2 монстр на ранге 1
занимает ранги 1–2, следующий юнит стартует с ранга 3; карточка рисуется шириной `×size`; ёмкость
формации = 4 слота (`4 − Σ size`). В WPF это сейчас не работает: (1) ядро парсит `size` и использует
его в таргетинге/запуске/AI, но **назначает ранги size-agnostic** (`i+1`); (2) WPF-бой — только
герои-vs-герои (`StartDuel`), монстры в бою не участвуют; (3) карточки фиксированной ширины, без
`Size`. Делаем: size-aware ранги в ядре (паритет Unity), рендер размера в WPF-виде, PvE-режим
(`StartFight` герои-vs-монстры) с водителем `PveBattleViewModel` (аналог Unity `CoreBattleDriver`).

## Design decisions (user-confirmed)

- **PvE-водитель** — отдельный `PveBattleViewModel` по образцу Unity `CoreBattleDriver`,
  переиспользует `DuelBattleView` для отображения. Герои игрока ходят через `ApplyRemoteSkill`
  (в `StartFight` герои — «remote»), монстры — `Solver.UseMonsterBrain` (локальная сторона).
- **Size-aware ранги в ядре** — да, в этом же плане (`FormationParty`, `TurnMover`, `SurpriseResolver`,
  `DuelBattleEvents.MoveUnit` пересчитывают ранги по кумулятивной сумме `Size`, как Unity).
- **Ёмкость формации** — 4 слота; слоты монстров в лобби ограничены `4 − Σ size`.
- Легаси Unity не правится (read-only); `src\External\` не трогается.

## Steps / status

### M1 — Core: size-aware назначение рангов (паритет Unity)

1. [x] `FormationParty.AddUnit` (core) — ранг = `последний.Rank + последний.Size` (или 1, если пусто);
   `RemoveUnit` — кумулятивный пересчёт по `Size`.
2. [x] `TurnMover.TryMove`, `SurpriseResolver.ShuffleParty`, `DuelBattleEvents.MoveUnit` — тот же
   кумулятивный пересчёт рангов (герои size=1 → ранги 1..4 не меняются).
3. [x] Тесты: формация size-2 + size-1 монстров → ранги 1,3 (не 1,2); таргетинг/запуск по рангам
   учитывает занимаемый диапазон; `dotnet test` зелёный.

### M2 — WPF вид: рендер размера

1. [x] `DuelUnitViewModel.Size` (из `unit.Size`); `DuelUnitCardView` ширина = `185 × Size` (конвертер
   или свойство); ранг-бейдж показывает фронтовый ранг.
2. [x] `DuelBattleView` колонки уже `MinWidth=820` (4×201px) — широкая карточка занимает N слотов,
   StackPanel растягивает; проверить вёрстку с size-2/3.

### M3 — WPF PvE-режим (`StartFight`, как Unity BattleTest)

1. [x] Кнопка **PvE** в `MainMenuView` → `PveLobbyViewModel`/`PveLobbyView`: 4 слота героев
   (переиспользовать `HeroSlotViewModel`) + слоты монстров из `DuelContent` (цикл по каталогу,
   суммарный size ≤ 4, seed).
2. [x] Старт: `DuelController.StartFight(heroSpecs, monsterSpecs, seed)`; `PveBattleViewModel` —
   водитель: поллинг/диффинг как `DuelBattleViewModel`, герои игрока → `ApplyRemoteSkill`, монстры →
   `UseMonsterBrain`, ввод через переиспользуемый `DuelBattleView`.
3. [x] Регистрация VM↔View в `MainWindow.xaml` (+ интерфейс `IDuelBattleViewData` для переиспользования
   `DuelBattleView` без правки `DuelBattleViewModel`). Тесты `PveBattleTests` (3) зелёные.

### M4 — Документация (в тех же коммитах)

1. [x] `docs\mechanics\combat\16_formation_size.md` (size-ранги: кумулятивный пересчёт, занимаемый
   диапазон, ёмкость 4, рендер); `BATTLE_PARITY.md` — строка size (2.6 + резюме).
2. [x] `TESTING.md` (PvE-чеклист + size-рендер + историческая строка), `CHANGELOG.md`,
   `00_index.md` (16), `INDEX.md`, `presentation_wpf.md` (gotcha).

## Affected files

- `src\Core\Sektor.DarkestDungeon.Core.Combat\Raid\Party\FormationParty.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Duel\Mechanics\TurnMover.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Duel\Mechanics\SurpriseResolver.cs`,
  `src\Core\Sektor.DarkestDungeon.Core.Duel\DuelBattleEvents.cs`.
- `src\Wpf\...\ViewModels\DuelUnitViewModel.cs`, `Views\DuelUnitCardView.xaml`,
  `ViewModels\PveLobbyViewModel.cs` (новый), `Views\PveLobbyView.xaml` (новый),
  `ViewModels\PveBattleViewModel.cs` (новый), `Views\MainMenuView.xaml`, `MainWindow.xaml`.
- Тесты: `tests\Core\...` (size-ранги).
- Доки: `docs\BATTLE_PARITY.md`, `docs\TESTING.md`, `docs\CHANGELOG.md`,
  `docs\mechanics\combat\13_turn_order.md` (или новая), `docs\mechanics\00_index.md`.

## Acceptance criteria

- `dotnet test` зелёный (новые size-ранги тесты + существующие sweep/дуэль).
- `dotnet build` WPF зелёный.
- PvE-бой: герои игрока ходят, монстры (в т.ч. size 2–4) ходят AI, size-карточки занимают N слотов,
  ранги 1,3,… без перекрытия; команды не дрейфуют (прошлый фикс не сломан).
- Легаси Unity и `src\External\` не изменены.

## Out of scope

- Полный cutover игры (кампания/город/инвентарь в WPF) — отдельные фазы.
- `.summon/.control/.capture` и `GetMonsterSize`/`AvailableSummonSpace`-стабы — не обязательны для
  рендера размера (если не мешают PvE-свип-тестам, не трогаем; иначе — минимальный фикс).
- Мультиплеер PvE (герои игрока в сети) — не в этом плане.

---

# PLAN: WPF-дуэль — привязка формаций к своим половинам (умершие не двигают команды)

## Goal

Исправить снос поля при смерти юнитов в WPF-бою (`DuelBattleView.xaml`): когда персонаж умирает,
коллекция сжимается, `Auto`-колонка схлопывается, сетка перецентрируется и правая команда съезжает
влево. Требуется: команды привязаны к своим половинам, фронт (ранг 1) у центра, центр разделяет
их на две — по 4 позиции слева и справа. Решение пользователя: **reflow к центру** (выжившие
сдвигаются вперёд, как в ядре `RemoveUnit`), а не резерв пустых слотов.

## Steps / status

1. [x] `DuelBattleView.xaml`: колонки `Auto` → слот-пол: герои `MinWidth≈820` (4×201px+margin),
   центр `Width≈120`, монстры `MinWidth≈820`.
2. [x] Пере-якорить стеки: герои `HorizontalAlignment="Right"` (колонка 0), монстры
   `HorizontalAlignment="Left"` (колонка 2).
3. [x] `TargetLayer` Canvas → `Grid.ColumnSpan="3"` (математика стрелок в координатах Viewbox
   остаётся корректной).
4. [x] Доки в том же коммите: `docs\mechanics\presentation\presentation_wpf.md` (gotcha),
   `docs\CHANGELOG.md`, `docs\TESTING.md` (дуэльный чеклист).
5. [x] `dotnet build` WPF-проекта зелёный. Ручной прогон боя (герои/монстры умирают — команды
   не дрейфуют) — **на пользователе** (см. `TESTING.md` § WPF-дуэль).

## Affected files

- `src\Wpf\Sektor.DarkestDungeon.Wpf\Views\DuelBattleView.xaml`.
- Доки: `docs\mechanics\presentation\presentation_wpf.md`, `docs\CHANGELOG.md`, `docs\TESTING.md`.

## Acceptance criteria

- Умершие юниты уходят, выжившие на своей половине сдвигаются к центру (фронт у центра).
- Правая команда не смещается влево при смерти героев; центр всегда разделяет команды.
- `TargetLayer` стрелки/бейдж не ломаются (ColumnSpan учтён).
- `dotnet build` зелёный; легаси не тронуто.

---

# PLAN (прошлый): BattleTest

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

1. [x] `docs\mechanics\presentation\presentation_unity_battle_view.md` — порядок срабатывания +
   gotchas (событий нет → poll/diff; `StartFight` инвертирует local/remote; legacy-твины в глобальном
   неймспейсе).
2. [x] `TESTING.md` (раздел), `INDEX.md`, `docs\mechanics\00_index.md` (уже — `15_disease.md`),
   `CHANGELOG.md`.
3. [x] `docs\mechanics\presentation\presentation_unity_battle_rig.md` — продакшн-риг (дизайн-решение),
   ссылки из `INDEX.md`.

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