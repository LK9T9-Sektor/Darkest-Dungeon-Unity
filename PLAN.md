# PLAN.md — активный план

## Активный план: захваты в папке проекта и автоподхват в WPF

### Goal

Unity пишет захваты в общую папку репозитория `captures\`, а WPF при открытии реплея сам
берёт самый свежий `run_*` — без ручного копирования файлов. Предыдущий срез `1.1` зафиксирован
коммитом `1689f58`.

### Design decisions

- Папка захватов: `<repo>\captures\run_<yyyy-MM-dd_HH-mm-ss>\`; добавляется в `.gitignore`.
- Корень репозитория ищется вверх по дереву по маркеру `Darkest-Dungeon-Unity.slnx`:
  Unity — от `Application.dataPath`, WPF — от `AppContext.BaseDirectory`.
- Unity-редактор пишет в проект; собранная игра без найденного репозитория — fallback в
  `persistentDataPath\Replays` (текущее поведение).
- WPF берёт самый свежий `run_*` с `manifest.json`; если захватов нет — встроенный эталон
  `Content\Captures\Reference` (`1.0.0`) продолжает работать.
- Логика поиска общая для двух клиентов и живёт в `Clients.EventLog`; Unity/WPF передают
  только стартовую директорию и fallback.
- Диагностика не имеет права прерывать игру: пустые слоты формации пропускаются, ошибки
  публикации/снапшотов/скиллов ловятся и пишутся в консоль `[GameEventLog] ... failed`.

### Steps / status

1. [x] `GameEventCaptureLocator` в `Clients.EventLog`: константы папок/маркера, поиск корня,
   `captures`-root, новейший `run_*`, fallback на эталон; XML docs.
2. [x] Тесты локатора: поиск корня, fallback без маркера, выбор новейшего run с manifest,
   fallback на reference.
3. [x] Unity `GameEventRecorder`: путь run-директории через локатор (`Application.dataPath` →
   `captures`, fallback `persistentDataPath\Replays`).
4. [x] WPF `MainWindow`: `ResolveReplayDirectory(AppContext.BaseDirectory, reference)` и передача
   найденного пути в `CaptureReplayViewModel`.
5. [x] `.gitignore`: `/captures/`.
6. [x] Документация: `SPEC_GAME_EVENTS_SCHEMA.md` §1, `TESTING.md`, `presentation_wpf.md`,
   changelog + индекс, этот `PLAN.md`.
7. [x] `dotnet test` (278/278), `check-using-placement`, `git diff --check`.
8. [x] Error isolation: пропуск пустых слотов, защита `Publish`/снапшотов/скиллов/анимации;
   ошибки пишутся в лог, но не прерывают бой (NRE после `SpawnEncounter`).
9. [ ] `unity-compile-check` после закрытия редактора (сейчас Unity открыт) и ручной прогон
   Unity → WPF: рейд пишет `<repo>\captures\run_*`, способности снова нажимаются, WPF
   подхватывает захват без копирования; отдельно проверить fallback на эталон. Захват
   `run_2026-10-20_23-26-37` уже записан, но в нём `skillResolved.Targets` местами пуст
   (guard по `Stress`), поэтому после фикса нужен новый прогон.

### Affected files

- `src\Clients\Sektor.DarkestDungeon.Clients.EventLog\GameEventCaptureLocator.cs` (новый),
  `GameEventFileNames.cs` (публичные имена файлов для тестов/клиентов).
- `tests\Clients\Sektor.DarkestDungeon.Clients.EventLog.Tests\GameEventCaptureLocatorTests.cs`
  (новый).
- `unity\Assets\Scripts\Diagnostics\GameEventRecorder.cs`,
  `GameEventRecording.cs` (защитные обёртки и пропуск пустых слотов),
  `unity\Assets\Scripts\Managers\RaidSceneManager.cs` (null-safe `stressBefore` в `ExecuteSkillBase`).
- `src\Wpf\Sektor.DarkestDungeon.Wpf\MainWindow.xaml.cs`.
- `.gitignore`.
- `docs\SPEC_GAME_EVENTS_SCHEMA.md`, `docs\TESTING.md`,
  `docs\mechanics\presentation\presentation_wpf.md`, `docs\changelog\00_index.md`,
  новый changelog, `PLAN.md`.

### Acceptance criteria

- Запуск рейда в Unity-редакторе создаёт `<repo>\captures\run_<timestamp>\`; в консоли
  `recording started` показывает этот путь.
- WPF при открытии «ВОСПРОИЗВЕСТИ ЗАХВАТ» читает самый свежий `run_*` без копирования.
- Если `captures` пуста или отсутствует, WPF играет встроенный эталон `1.0.0`.
- Собранная Unity-игра без репозитория пишет в `persistentDataPath\Replays`.
- Ошибка capture-диагностики не прерывает бой: пустые слоты пропускаются, исключение пишется
  в консоль `[GameEventLog]`, способности продолжают работать.
- `dotnet test`, `check-using-placement`, `unity-compile-check` и `git diff --check` зелёные;
  `unity-2017\`/`src\External\` не изменяются.

### Out of scope

- Чтение незавершённой записи и выбор захвата из UI.
- FMOD playback в WPF.
- Полная карта/кампания и точные x/y позиции.

### Временные артефакты

- `captures\` — runtime-записи; gitignored, можно удалять целиком.
- `%TEMP%\eventlog-capture-locator-*` — временные папки тестов, удаляются автоматически.
