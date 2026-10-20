# PLAN.md — активный план

## Активный план: estate capture 1.2 и WPF-реплей имения

### Goal

Записывать имение (`EstateManagement`) отдельным run и воспроизводить его в WPF: ресурсы, ростер,
здания/апгрейды, квесты/провизия и городские события. Estate не должен ломать игру при ошибке
диагностики. Рейдовый capture `1.1` и старый эталон `1.0.0` продолжают работать.

### Design decisions

- Estate — отдельный run (`Kind = estate`), рейд — `Kind = raid`; старые run без `Kind` = `raid`.
- Схема `1.2.0`: новые estate-события, `SubsystemNames.Estate`, `GameEventManifest.Kind`.
- После каждого estate-действия публикуется action-событие и свежий `estateSnapshot`, чтобы WPF
  получал точные ресурсы/ростер/здания без ручного пересчёта дельт.
- `ActiveRecorder(kind)` проверяет и recording, и контекст; аудио разрешено в обоих контекстах.
- WPF v1 — текст/карточки; арт, интерактивные окна и активности героев — позже.

### Steps / status

1. [x] Контракт `1.2.0`: `GameEventRunKinds`, `Kind` в manifest/JsonEventLogger, `SubsystemNames.Estate`,
   `EstateSnapshot`, `EstateResourceAmount`, `EstateHeroState`, `EstateBuildingState`,
   `EstateQuestState`, `EstateHeroRecruited`, `EstateHeroDismissed`, `EstateHeroResurrected`,
   `EstateBuildingUpgraded`, `EstateQuestSelected`, `EstateProvisionsPurchased`,
   `EstateHeirloomExchanged`, `EstateTownEventTriggered`; reader `1.0.0`/`1.1.0`/`1.2.0`;
   binder allowlist.
2. [x] Unity recorder: определить `EstateSceneManager`, запускать отдельный estate-run,
   `StartRecording(scene, kind)`, `ActiveRecorder(kind)`, сохранить `Kind` в manifest.
3. [x] Estate publishers: snapshot на входе; recruited/dismissed/resurrected; building upgraded;
   quest selected; provisions purchased/sellback; heirloom exchange; town event; snapshot после
   каждого действия. Все хуки защищены от исключений.
4. [x] WPF: `GameEventCaptureLocator` ищет свежий run по kind; battle replay — только `raid`;
   новый `EstateReplayView`/`EstateReplayViewModel` (неделя, ресурсы, здания, ростер, квесты,
   town event, таймлайн, аудио) + пункт меню.
5. [x] Тесты: Core DTO/шина, synthetic estate `1.2.0` в reader, выбор run по kind, WPF VM/render.
6. [x] Docs: `SPEC_GAME_EVENTS_SCHEMA.md` `1.2.0`, `SPEC_GAME_EVENTS.md`, `TESTING.md`,
   mechanics-док estate capture, changelog, этот план.
7. [x] Автопроверки: `dotnet test` (295/295), `check-using-placement`, `unity-compile-check`,
   `git diff --check`.
8. [ ] Ручная проверка (после компиляции Unity):
   1. Завершить рейд и вернуться в `EstateManagement`.
   2. Проверить новый `captures\run_*\manifest.json`: `Kind: "estate"`, в `Files` есть
      `estate_session.jsonl`; в нём `estateSnapshot` с ресурсами/золотом/реликвиями, ростером,
      зданиями, квестами и town event.
   3. Сделать 2–3 действия: найм/увольнение, апгрейд здания, выбор квеста, покупку провизии,
      обмен реликвий; после каждого в capture должны появиться action-событие и новый snapshot.
   4. WPF → «ИМЕНИЕ (ЗАХВАТ)»: проверить неделю, ресурсы, здания, ростер, квесты, таймлайн,
      аудио и кнопки «ПОВТОРИТЬ»/«НАЗАД».
   5. WPF → «АРЕНА 1×1» → «ВОСПРОИЗВЕСТИ ЗАХВАТ»: battle replay должен брать raid-run, не estate.
   6. Убедиться, что ошибки диагностики только логируются `[GameEventLog] ... failed` и не
      ломают имение/бой.
   7. Проверить fallback: без estate-run экран показывает «Захват имения недоступен».

### Affected files

- `src\Clients\Sektor.DarkestDungeon.Clients.EventLog\`: `GameEventRunKinds.cs`,
  `GameEventManifest.cs`, `JsonEventLogger.cs`, `GameEventLogReader.cs`, `GameEventTypeBinder.cs`,
  `GameEventCaptureLocator.cs`.
- `src\Core\Sektor.DarkestDungeon.Core.GameEvents\`: новые estate DTO/события,
  `SubsystemNames.cs`.
- `unity\Assets\Scripts\Diagnostics\GameEventRecorder.cs`, `GameEventRecording.cs`.
- `unity\Assets\Scripts\Managers\EstateSceneManager.cs`,
  `unity\Assets\Scripts\Campaign\Estate.cs`, `Campaign.cs`,
  `unity\Assets\Scripts\Managers\RaidPreparationManager.cs`,
  `unity\Assets\Scripts\Managers\ShopManager.cs`,
  `unity\Assets\Scripts\UI\Panels\HeroRosterPanel.cs`,
  `unity\Assets\Scripts\UI\Controls\ExchangeEntry.cs`.
- `src\Wpf\Sektor.DarkestDungeon.Wpf\`: `MainWindow.xaml.cs`, `MainMenuViewModel.cs`,
  `MainMenuView.xaml`, новые `EstateReplayViewModel.cs`/`EstateReplayView.xaml(.cs)`.
- Tests: Core GameEvents, Clients EventLog, WPF.
- Docs: `SPEC_GAME_EVENTS_SCHEMA.md`, `SPEC_GAME_EVENTS.md`, `TESTING.md`,
  `docs\mechanics\presentation\presentation_wpf.md`, новый
  `docs\mechanics\campaign\estate_capture.md`, `docs\changelog\00_index.md`, changelog.

### Acceptance criteria

- Вход в `EstateManagement` создаёт `captures\run_<timestamp>\` с `Kind = estate`,
  `estate_session.jsonl` и `audio_session.jsonl`.
- Snapshot содержит ресурсы/золото/реликвии, ростер, здания, квесты и активный town event.
- Действия (найм/увольнение/воскрешение/апгрейд/квест/провизия/обмен/событие) видны в capture.
- WPF estate screen играет последний estate-run; battle screen — только raid-run; старый эталон
  `1.0.0` всё ещё играет.
- Ошибка диагностики не прерывает бой/имение; пишется `[GameEventLog] ... failed`.
- `dotnet test`, `check-using-placement`, `unity-compile-check`, `git diff --check` зелёные.

### Out of scope

- Арт/портреты/спрайты зданий.
- Интерактивный estate в WPF.
- Активности героев (Abbey/Tavern/Sanitarium/Blacksmith/Guild, лечение/скиллы/шмот).
- Полный campaign/save replay.

### Временные артефакты

- `captures\` — runtime-записи; gitignored.
- `%TEMP%\eventlog-estate-*` — временные папки тестов.
