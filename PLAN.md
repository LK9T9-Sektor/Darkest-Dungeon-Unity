# PLAN.md — активный план

## Активный план: боевой контракт 1.1 + аудио-каталог и запись

### Goal

Довести контракт записи до состояния, пригодного для полноценного WPF-реплея боя и аудио:
формация (`Rank`/`Size`), стресс, статусы/баффы, границы хода/раунда, полные результаты скилла
по всем целям, RNG; плюс каталог FMOD-событий и запись реально проигранного аудио.

Текущий срез: **боевой контракт + аудио-каталог + запись аудио**. Карта — следующий P1,
WPF-воспроизведение FMOD — отдельный срез, затем run/orchestration WPF-игры.

### Design decisions

- Схема `1.1.0`; reader принимает `1.0.0` и `1.1.0`. Старые записи без `rank`/`size`
  показываются по порядку списка (`fallbackRank = index + 1`).
- `AttackExecuted` остаётся визуальным событием (кто/что/куда); точные результаты всех целей —
  новый `SkillResolved`.
- Новые поля `CombatantState` — optional-параметры в конце записи, чтобы не ломать старые вызовы.
- Аудио-события: subsystem `audio`, файл `audio_session.jsonl`, тип `AudioPlayed`.
  Банк в событие не дублируется — WPF сопоставляет `EventPath` с каталогом.
- Каталог строится из `unity\Assets\FMODStudioCache.asset` (2007 уникальных event paths,
  40 банков, 20 music events), а не из `.strings.bank`; динамические пути логируются фактически.
- Для аудио в WPF использовать нативные `fmod.dll`/`fmodstudio.dll` x64 из
  `unity\Assets\Plugins\FMOD\Libs\x86_64`; банки играть напрямую, без декодирования в WAV.
- Legacy Unity (`unity\`) меняется минимально; `unity-2017\` и `src\External\` не трогаем.
- JSON — только на границе клиента (`Clients.EventLog`), allowlist-биндер, ошибки через `Result`.

### Steps / status

1. [x] Аудио-каталог: `tools\extract-fmod-catalog.ps1`, `docs\audio\audio_catalog.json`,
   `docs\audio\audio_catalog.md`, `docs\audio\00_index.md`; проверить counts и bank mapping.
2. [x] Core `GameEvents`: `CombatantState` (rank/size/stress/statuses/buffs/death), `BattleState`
   в `StateSnapshot`, `SkillResolved`, `UnitMoved`, turn/round/battle-end события, `AudioPlayed`,
   новые subsystem/input/context константы; XML docs.
3. [x] Unity recorder: боевой старт со снапшотом, `SkillResolved` по всем целям + `UnitMoved`,
   stress/heal, turn/round hooks в `Round`, battle end; доступ к статусам/баффам `Character`;
   RNG первой цели в `SkillResolved.Rng`.
4. [x] Unity audio recorder: хуки `DarkestSoundManager.PlayOneShot`/музыки/нарратива и прямых
   `RuntimeManager.CreateInstance`; запись `AudioPlayed` с фактическим путём, kind/action/volume.
5. [x] `Clients.EventLog` + WPF replay: allowlist новых типов, чтение `1.0.0`/`1.1.0`,
   fallback формации, stress/statuses в `CaptureUnitViewModel`, журнал и аудио-секвенция.
6. [x] Тесты, эталонный захват, документация (`SPEC_GAME_EVENTS*`, `TESTING.md`,
   `presentation_wpf.md`, changelog), `dotnet test`, `check-using-placement`, Unity compile check,
   `git diff --check`.
7. [x] Зафиксировать следующие P1-срезы: полный `MapSnapshot`/map events и WPF FMOD playback.

### Affected files

- `PLAN.md`.
- `src\Core\Sektor.DarkestDungeon.Core.GameEvents\`: `CombatantState.cs`, `StateSnapshot.cs`,
  `BattleState.cs`, `SkillResolved.cs`, `SkillTargetResult.cs`, `UnitMoved.cs`,
  `TurnStarted.cs`, `TurnEnded.cs`, `RoundStarted.cs`, `RoundEnded.cs`, `BattleEnded.cs`,
  `AudioPlayed.cs`, `AudioPosition.cs`, `AudioParameter.cs`, `AudioEventKinds.cs`,
  `AudioActions.cs`, `StateSnapshotContexts.cs`, `SubsystemNames.cs`, `GameInputKinds.cs`.
- `src\Clients\Sektor.DarkestDungeon.Clients.EventLog\`: `GameEventTypeBinder.cs`,
  `GameEventLogReader.cs`, `JsonEventLogger.cs`.
- `unity\Assets\Scripts\Diagnostics\GameEventRecording.cs`.
- `unity\Assets\Scripts\Managers\RaidSceneManager.cs`.
- `unity\Assets\Scripts\Mechanics\Battle\Round.cs`.
- `unity\Assets\Scripts\Mechanics\Battle\BattleSolver.cs`, `SkillResultEntry.cs`,
  `unity\Assets\Scripts\Mechanics\RandomSolver.cs` (запись hit/crit roll).
- `unity\Assets\Scripts\Character\Character.cs` (read-only accessor для баффов).
- `unity\Assets\Scripts\Managers\DarkestSoundManager.cs`.
- `unity\Assets\FMODStudioCache.asset`, `unity\Assets\Resources\FMODStudioSettings.asset`.
- `tools\extract-fmod-catalog.ps1`, `docs\audio\*`, `docs\INDEX.md`.
- `src\Wpf\Sektor.DarkestDungeon.Wpf\ViewModels\CaptureReplayViewModel.cs`,
  `CaptureUnitViewModel.cs`, `Views\CaptureReplayView.xaml`.
- `tests\Clients\Sektor.DarkestDungeon.Clients.EventLog.Tests\GameEventLogReaderTests.cs`,
  `tests\Core\Sektor.DarkestDungeon.Core.GameEvents.Tests\GameEventBusTests.cs`,
  `tests\Wpf\Sektor.DarkestDungeon.Wpf.Tests\CaptureReplay*.cs`.
- `docs\SPEC_GAME_EVENTS_SCHEMA.md`, `docs\SPEC_GAME_EVENTS.md`, `docs\TESTING.md`,
  `docs\mechanics\presentation\presentation_wpf.md`, `docs\changelog\00_index.md`.

### Acceptance criteria

- Каталог содержит все 2007 уникальных `event:/` путей, bank mapping и 20 music events;
  генератор воспроизводим и не зависит от бинарного `.strings.bank`.
- Захват боя содержит rank/size/stress/statuses/buffs на старте и в `turnEnd`, `SkillResolved`
  со всеми целями (включая miss/stress), `UnitMoved`, `TurnStarted/Ended`,
  `RoundStarted/Ended`, `BattleEnded`; `Rng` первой цели содержит hit/crit roll и шанс
  (`Seed` = null), per-target rolls мультицелевого скилла — ограничение v1.1.
- Захват audio содержит `AudioPlayed` для одного music start, одного SFX и одного narration
  с фактическим `EventPath`; WPF-каталог находит их банки.
- WPF-реплей старых `1.0.0` не падает и расставляет юнитов по порядку; `1.1.0` показывает
  формацию, стресс, статусы и аудио-журнал.
- `dotnet test` зелёный; `check-using-placement`, `unity-compile-check` и `git diff --check`
  проходят; изменения в `unity-2017\`/`src\External\` отсутствуют.
- Незакоммиченные изменения предыдущего WPF-этапа сохранены и включены в общий результат.

### Out of scope

- Полный граф карты и map-события (P1, следующий срез).
- WPF FMOD playback и выбор банков (отдельный срез).
- Полноценная WPF-игра: run/orchestration, кампания/город, save/load, контент/ассеты.
- Изменение legacy-поведения боя ради паритета; только инструментация.

### Временные артефакты

- `%TEMP%\game-events-audio-catalog.json` — промежуточный вывод генератора; можно удалить.
- `%TEMP%\capture-replay-*.png` — скриншоты проверки layout; можно удалить.
