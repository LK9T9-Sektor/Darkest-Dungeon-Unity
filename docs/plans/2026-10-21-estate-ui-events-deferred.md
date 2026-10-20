# 2026-10-21 — запись UI-навигации имения (отложено)

> Отложен: 2026-10-21 (ветка `core/game-events-contract`; опорные коммиты — `ad3183e`,
> запись estate-снапшотов и действий). Причина: сейчас закрыт WPF-экран; хуки решено
> добавлять позже отдельным срезом.

## Состояние

- **Сделано:** estate-run `1.2.0` пишет состояние имения (ресурсы, ростер, здания, квесты,
  town event) и действия экономики/ростера/квестов; WPF-экран «ИМЕНИЕ (ЗАХВАТ)» исправлен и
  показывает карточки зданий/героев и таймлайн.
- **Осталось:** записывать UI-навигацию — открытие глоссария, realm inventory, лога активности,
  окна town event, карточки героя и окон зданий. Сейчас эти клики в capture не попадают.
- **Почему отложено:** пользователь решил не смешивать это с фиксом WPF-экрана.
- **Как возобновить:** поднять контракт до `1.3.0`, добавить событие(я), хуки в Unity, поддержку
  в reader/WPF, тесты и документацию; проверить `unity-compile-check` при закрытом Unity.

---

## Исходный план: запись UI-навигации имения

### Goal

Записывать в estate-run все значимые визиты UI, чтобы WPF-таймлайн показывал не только
состояние и экономику, но и навигацию игрока: «открыт глоссарий», «открыт инвентарь»,
«открыто здание Abbey», «открыта карточка Reynald» и т.п. Старые захваты должны читаться.

### Design decisions

- Одно общее событие `EstateUiOpened` с `ScreenId` и необязательным `DetailId` (id здания/героя),
  а не отдельный тип на каждый экран. Id — строки, константы в `Core.GameEvents`
  (`EstateUiScreenIds`), без enum-диспетчера.
- Пишем **открытие** экрана, не закрытие: закрытие — шум для таймлайна. Переключение карточки
  героя считается новым открытием с другим `DetailId`.
- Схема `1.3.0`; reader поддерживает `1.0.0`…`1.3.0`; binder allowlist пополняется.
- WPF только подписывает событие в таймлайне (без карточек и окон).
- Не пишем hover/tooltip, движения мыши и содержимое realm inventory — это не навигация.

### Steps / status

1. [ ] `Core.GameEvents`: `EstateUiOpened` (`ScreenId`, `DetailId`), `EstateUiScreenIds`
   (`glossary`, `realm_inventory`, `activity_log`, `town_event`, `character`, `building`,
   `provision`), XML-доки; `JsonEventLogger.CurrentSchemaVersion = "1.3.0"`,
   `GameEventTypeBinder` allowlist.
2. [ ] `GameEventRecording`: `PublishEstateUiOpened(string screenId, string? detailId)` с
   безопасной обёрткой (как остальные estate-публикаторы).
3. [ ] Unity-хуки:
   - `EstateSceneManager.GlossaryClick/RealmInventoryClick/ActivityLogClick/TownEventClick` —
     публиковать только на открытии;
   - `EstateSceneManager.CharacterWindowSwitch(Hero hero, bool)` — `character` + `hero.Id`;
   - окна зданий (`UpgradableBuildingWindow.WindowOpened`, `AbbeyWindow`, `TavernWindow`,
     `SanitariumWindow`, `StatueWindow`, `GraveyardWindow`, `StageCoachWindow`, `NomadWagonWindow`,
     `BlacksmithHeroWindow` по фактическому месту открытия) — `building` + `Building.Name`;
   - при необходимости `provision` на входе в экран провизии.
4. [ ] `GameEventLogReader`: поддержка `1.3.0`; тесты reader на новый тип.
5. [ ] WPF `EstateReplayViewModel`: описание `EstateUiOpened` в `descriptions` (например
   «Открыт глоссарий», «Открыто здание: Abbey», «Открыта карточка: Reynald»).
6. [ ] Тесты: Core DTO/сериализация, reader `1.3.0`, WPF-описание/таймлайн.
7. [ ] Docs: `SPEC_GAME_EVENTS_SCHEMA.md`, `docs/mechanics/campaign/estate_capture.md`,
   `docs/mechanics/presentation/presentation_wpf.md`, changelog + индекс.
8. [ ] Проверки: `dotnet test`, WPF-тесты, `check-using-placement`,
   `unity-compile-check` (Unity закрыт), `git diff --check`.

### Affected files

- `src/Core/Sektor.DarkestDungeon.Core.GameEvents/` — новый DTO + ids + binder/logger.
- `src/Clients/Sektor.DarkestDungeon.Clients.EventLog/` — reader/binder.
- `unity/Assets/Scripts/Diagnostics/GameEventRecording.cs` — публикатор.
- `unity/Assets/Scripts/Managers/EstateSceneManager.cs` — хуки экранов.
- `unity/Assets/Scripts/UI/Windows/Buildings/*.cs` — хуки открытия зданий.
- `src/Wpf/Sektor.DarkestDungeon.Wpf/ViewModels/EstateReplayViewModel.cs` — описание.
- `tests/...` — Core/EventLog/WPF.
- Docs: schema, mechanics, changelog.

### Acceptance criteria

- Открытие глоссария, инвентаря, лога активности, town event, карточки героя и любого здания
  добавляет `estateUiOpened` в `estate_session.jsonl` с нужным `ScreenId`/`DetailId`.
- WPF-таймлайн показывает эти визиты по-русски.
- Захваты `1.0.0`–`1.2.0` по-прежнему читаются и играются.
- `dotnet test`, WPF-тесты, `check-using-placement`, `unity-compile-check` зелёные.

### Out of scope

- Hover/tooltip и прочие не-навигационные клики.
- Активности героев (лечение/стресс/скиллы/шмот) и содержимое инвентаря — отдельный срез.
- Визуальные окна/карточки UI-навигации в WPF — только строки таймлайна.
