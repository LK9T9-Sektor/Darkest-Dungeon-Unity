# estate_capture.md — запись и воспроизведение имения

## 1. Назначение и когда работает

Estate-run пишется, когда загружается сцена `EstateManagement` с `EstateSceneManager.Instanse`.
Это отдельный run от рейда (`manifest.Kind = estate`), стартует автоматически и завершается при
выгрузке сцены. Записываются ресурсы/золото/реликвии, ростер, здания, квесты, активный town event
и действия игрока. WPF-экран «ИМЕНИЕ (ЗАХВАТ)» проигрывает последний estate-run: неделя и ресурсы
сверху, здания сеткой слева/в центре, карточки героев в скролле справа, таймлайн/аудио снизу;
квесты на экране не показываются (открываются отдельно).

**Не входит:** активности героев (Abbey/Tavern/Sanitarium/Blacksmith/Guild, лечение/скиллы/шмот),
арт зданий/портреты, интерактивный estate в WPF, UI-навигация (открытие глоссария, инвентаря,
окон зданий и карточки героя) — она не пишется в capture.

## 2. Модель данных

- `EstateSnapshot`: `Week`, `Resources[]` (`EstateResourceAmount`: `Id`, `Amount`), `Roster[]`
  (`EstateHeroState`), `Buildings[]` (`EstateBuildingState`: `Id`, `Name`, `UpgradeRatio`),
  `Quests[]` (`EstateQuestState`), `TownEventId`/`TownEventTone`.
- `EstateHeroState`: `Id` (`hero_<rosterId>`), `Class`, `Name`, `Level`, `ResolveLevel`, `Hp`,
  `MaxHp`, `Stress`, `Status`, `IsAlive`, `IsAfflicted`, `IsVirtued`, `Quirks[]`, `Diseases[]`.
- `EstateQuestState`: `Id`, `Dungeon`, `Type`, `Difficulty`, `Length`, `GoalId`, `IsPlot`.
- Действия: `EstateHeroRecruited`, `EstateHeroDismissed`, `EstateHeroResurrected`,
  `EstateBuildingUpgraded`, `EstateQuestSelected`, `EstateProvisionsPurchased`,
  `EstateHeirloomExchanged`, `EstateTownEventTriggered`.
- Subsystem: `estate` → `estate_session.jsonl`; аудио — в `audio_session.jsonl`.

## 3. Порядок срабатывания (трассировка)

1. `GameEventRecorder.OnSceneLoaded` видит `EstateSceneManager.Instanse` и запускает run с
   `Kind = estate` (`GameEventRecorder.cs`).
2. `EstateSceneManager.Start` после `CurrencyPanel.UpdateCurrency`,
   `RosterPanel.InitializeRoster`, `TownManager.InitializeBuildings` вызывает
   `PublishEstateTownEventTriggered` и `PublishEstateSnapshot`.
3. Действия:
   - `HeroRosterPanel.CreateSlot` → recruited/resurrected + snapshot;
   - `Campaign.DismissHero` → dismissed + snapshot;
   - `Estate.BuyUpgrade(string, TownUpgrade, bool)` → building upgraded + snapshot;
   - `RaidPreparationManager.DungeonPanelQuestSelected` → quest selected + snapshot;
   - `ShopManager` purchase/sellback → provisions + snapshot;
   - `ExchangeEntry.ConfirmButtonClicked` → heirloom exchange + snapshot.
4. Выход из `EstateManagement` → `GameEventRecorder.StopRecording`, manifest финализируется.

## 4. Очередь и обновления

Действие и snapshot идут в одном `Sequence`; snapshot публикуется после изменения состояния.
WPF применяет `estateSnapshot` целиком и не пересчитывает дельты; action-события только
показываются в таймлайне. Темп WPF — 100 мс на видимый шаг; `AudioPlayed` не темпуются и не
попадают в основной таймлайн (они копятся в `AudioLog`).

## 5. Проверки и клэмпы

| Условие | Где | Границы |
|---|---|---|
| Kind run | `GameEventRecorder` | `raid`/`estate`; старый manifest = `raid` |
| Выбор захвата | `GameEventCaptureLocator` | только `run_*` с manifest и нужным kind |
| Snapshot | `GameEventRecording.PublishEstateSnapshot` | ресурсы из `Estate.Currencies`, ростер из `Campaign.Heroes`, здания из `BuildingType` |
| Ошибка диагностики | `GameEventRecording`/`GameEventRecorder` | лог `[GameEventLog] ... failed`, игра продолжается |

## 6. Нюансы и подводные камни

- **Пустые/битые данные не должны ломать estate:** все публикаторы обёрнуты, `Hero`/`Quest`/
  `TownEvent` проверяются на `null`; snapshot строится из безопасных копий.
- **Snapshot после действия, а не до:** `DismissHero` публикует событие до удаления, snapshot —
  после; `HeroRosterPanel.CreateSlot` — после добавления героя.
- **`UpgradeRatio` — доля купленных апгрейдов, не «уровень»:** WPF показывает процент; для v1 этого
  достаточно, точный уровень дерева — отдельный срез.
- **`estateProvisionsPurchased.GoldCost` для sellback может быть 0:** точные остатки всё равно
  берутся из следующего `estateSnapshot`.
- **Аудио:** estate-run теперь пишет town music/UI; старые `1.0.0`/`1.1.0` — только рейд.
  В одном run `Sequence` общий для `estate_session.jsonl` и `audio_session.jsonl`, поэтому в
  отдельно взятом estate-файле номера идут с пропусками (например 1, 144, 145…): reader
  склеивает файлы из manifest и проверяет непрерывность уже по объединённому списку.
- **UI-навигация не пишется:** клики по глоссарию, инвентарю, окнам зданий и карточке героя
  событий не создают; в capture попадают только состояние (`estateSnapshot`) и действия
  экономики/ростера/квестов. Для визитов нужен отдельный контракт.
- **Закрытие сцены:** запись останавливается при выгрузке `EstateManagement`; следующий рейд —
  новый run.

## 7. Взаимодействия

- `docs/SPEC_GAME_EVENTS_SCHEMA.md` §3.18–§3.26 — wire-формат estate-событий.
- `docs/mechanics/campaign/campaign_overview.md` — модели имения/зданий/квестов.
- `docs/mechanics/presentation/presentation_wpf.md` — экран estate replay.
- `docs/plans/2026-10-21-estate-ui-events-deferred.md` — будущая запись UI-навигации
  (глоссарий/инвентарь/окна зданий/карточка героя).

## 8. Файлы-источники

- `unity/Assets/Scripts/Diagnostics/GameEventRecorder.cs`,
  `GameEventRecording.cs`.
- `unity/Assets/Scripts/Managers/EstateSceneManager.cs`,
  `Campaign/Estate.cs`, `Campaign/Campaign.cs`,
  `Managers/RaidPreparationManager.cs`, `Managers/ShopManager.cs`,
  `UI/Panels/HeroRosterPanel.cs`, `UI/Controls/ExchangeEntry.cs`.
- `src/Core/Sektor.DarkestDungeon.Core.GameEvents/Estate*.cs`.
- `src/Clients/Sektor.DarkestDungeon.Clients.EventLog/GameEventRunKinds.cs`,
  `GameEventManifest.cs`, `GameEventCaptureLocator.cs`.
- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/EstateReplayViewModel.cs`,
  `Views/EstateReplayView.xaml`.
- `tests/Clients/Sektor.DarkestDungeon.Clients.EventLog.Tests/EstateCaptureReaderTests.cs`,
  `tests/Wpf/Sektor.DarkestDungeon.Wpf.Tests/EstateReplayViewModelTests.cs`.
