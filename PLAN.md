# PLAN.md — планы задач

Два состояния плана, оба — в git:

- **активный** — на этой странице: цель, шаги, статусы;
- **отложенный** (незавершённый) — `docs\plans\<ГГГГ-ММ-ДД>-<слаг>-deferred.md` (дата — момент
  отложения; внутри «Состояние» и исходный план), индекс — `docs\plans\00_index.md`.

Завершённые планы не хранятся (их держит git-история); user-visible итог — коротким файлом в
`docs\changelog\<ГГГГ-ММ-ДД>-<слаг>.md` (индекс — `docs\changelog\00_index.md`).
Мелкие задачи без плана в архив не попадают. `.opencode\` для планов не используется.

---

## Отложенные планы

- `docs\plans\2026-10-19-fixed-battle-scene-deferred.md` — фаза 2-A (пин высоты боевой сцены +
  `UI_Overlay` match-height). Возобновление: когда будут доступны эталонные снимки с другого ПК и
  прогон сетки аспектов.

## Активный план: WPF-потребитель JSONL — «Воспроизвести захват»

### Goal

WPF читает эталонный run Unity (`manifest.json` + `*_session.jsonl`) и проигрывает его как
визуальную демонстрацию: проверяет схему, восстанавливает состояние (состав партии, комната,
факел, HP) и по очереди проигрывает `attackExecuted`; вход — кнопка «ВОСПРОИЗВЕСТИ ЗАХВАТ» в
арене. Это первый потребитель контракта `docs\SPEC_GAME_EVENTS_SCHEMA.md` и проверка, что
захваченных данных достаточно для клиента.

### Design decisions

- Чтение JSONL — в `Clients.EventLog` (`GameEventLogReader`), на границе клиента: JSON не
  попадает в Core; десериализация с allowlist-биндером (только типы событий контракта и их
  коллекции), ошибки — `Result`, без исключений. Reader проверяет `SchemaVersion`, `Sequence`
  (1..N, без дыр/дублей) и соответствие `Subsystem` имени файла.
- Эталон (`docs\spec_game_events\examples\`) линкуется в output WPF как `Content\Captures\Reference`
  (`None Include` + `CopyToOutputDirectory`), без чтения репозитория в рантайме.
- Воспроизведение — отдельный экран `CaptureReplayView` (вход из арены): слоты 1×1 показывают
  атакующего/цель текущего `attackExecuted`; сбоку — комната, факел, HP партии/монстра и
  таймлайн. Герои играют захваченный клип через `SpineHeroControl` (мэппинг
  `crusader/attack_stun` → `Crusader/AttackStun`, `highwayman/attack_slice` →
  `Highwayman/AttackSlice` и т.п.); монстр (`brigand_cutthroat`) в v1 — карточка с именем/HP,
  без Spine (его ассеты — отдельный backlog-пункт). Неизвестная анимация → idle, а не падение.
- Темп: `attackExecuted` ждёт завершения нецикличной анимации (`AnimationCompletedCommand`);
  остальные события применяются сразу или с короткой задержкой; после последнего события
  остаётся финальное состояние и кнопка «Повторить».
- Core/duel logic не трогаем: это presentation-плеер, а не второй бой.

### Steps / status

1. [ ] `Clients.EventLog`: `GameEventLogReader` + модели чтения (manifest, события),
   allowlist-биндер, проверка `Sequence`/`Subsystem`; тесты на эталон, битую последовательность и
   неизвестный `$type`.
2. [ ] WPF: линковка эталона в `Content\Captures\Reference`, каталог/мэппинг анимаций.
3. [ ] WPF: `CaptureReplayViewModel` — проектор состояния (party/room/torch/HP/queue) и команды
   play/replay; темп по `attackExecuted`.
4. [ ] WPF: `CaptureReplayView` (1×1 слоты + боковая панель), вход из арены
   (`SpineBattleViewModel`/`SpineBattleView`), навигация в `MainWindow`.
5. [ ] Доки: `SPEC_GAME_EVENTS_SCHEMA` (consumer note), `TESTING.md` (ручной чек-лист),
   changelog; `dotnet build`/`dotnet test` WPF-решения.

### Affected files

- `src/Clients/Sektor.DarkestDungeon.Clients.EventLog/GameEventLogReader.cs` (+ модели чтения).
- `tests/Clients/Sektor.DarkestDungeon.Clients.EventLog.Tests/GameEventLogReaderTests.cs`.
- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/CaptureReplayViewModel.cs`,
  `Views/CaptureReplayView.xaml(.cs)`, `Data/CaptureAnimationCatalog.cs`,
  `Sektor.DarkestDungeon.Wpf.csproj`.
- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/SpineBattleViewModel.cs`,
  `Views/SpineBattleView.xaml`, `MainWindow.xaml.cs` (точка входа).
- `docs/SPEC_GAME_EVENTS_SCHEMA.md`, `docs/TESTING.md`,
  `docs/changelog/2026-10-20-wpf-capture-replay.md`, `PLAN.md`.

### Acceptance criteria

- Из арены «ВОСПРОИЗВЕСТИ ЗАХВАТ» открывает эталон и проигрывает 38 событий: 6 атак в порядке
  `Sequence`, HP/факел/комната обновляются, финал — факел 7, монстр 0 HP, комната `room2_1`;
  геройские атаки идут захваченными клипами, монстр/неизвестный клип не роняют плеер.
- Reader: валидный эталон читается, дыра/дубль `Sequence`, чужой `Subsystem` и неизвестный `$type`
  дают `Result`-ошибку без исключений.
- `dotnet test` (Core/Clients) и `dotnet build` WPF — зелёные; Unity-деревья не тронуты.

### Out of scope

- Экстракция Spine-ресурсов монстров (бандит) и полноценная сцена 2 героя vs монстр.
- Карта/фон/иконки, произвольный выбор run, редактирование/скраббинг таймлайна.
- `item*`, rng, `combat_01` — по-прежнему вне v1.

## Backlog (следующие шаги, план ещё не заведён)

- По линии игровых событий: экстракция ресурсов (фоны/иконки/спайн бандитов) в gitignored-общую
  папку под все клиенты (WPF/Avalonia), арена hero vs бандиты (`brigand_cutthroat_A`, контент уже
  грузится), `item*`-события, заполнение rng, нумерация `combat_01`; расширение плеера захвата
  (выбор произвольного run, монстровый Spine, скраббинг).
- URP-хвосты: порт освещения SkeletonLit (URP lit), замена BlurOptimized (showoff-блюр)
  URP-аналогом, мёртвый `CharacterSprite.shader` (Spine/DungeonSprite, 0 ссылок) — отдельные
  задачи, не трогаем.
- Фикс `tools\unity-compile-check.ps1`: сверка версии падает на этом ПК — установка 2017.4.40f1
  в `E:\ProgramFiles\Unity2017.4.40f1` отдаёт `ProductVersion 2017.4.40.7214086` (без `f1`), а
  `ProjectVersion.txt` ждёт `2017.4.40f1` (`StartsWith` не срабатывает). `unity-2017`
  проверен ручным batch-прогоном — зелёный (компиляция + script-reference check).
- Монстры и остальные герои в карточке дуэли; интеграция анимаций боя (атаки, скиллы).
