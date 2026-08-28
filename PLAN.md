# PLAN.md — Активный план задач

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

## Правила

- Сначала ядро (`src\Core`), потом адаптеры; `src\External\` — read-only.
- Доки обновляются в том же коммите; `CHANGELOG.md` — только user-visible.
- Всё новое — минимальными диффами, без оппортунистических рефакторов legacy.